using API.Entities;
using API.Interfaces;
using AutoMapper;
using System.Diagnostics;
using static API.Interfaces.IUserRepository;

namespace API.SignalR;

public class PhotoModerator
{
    private static int PoolSize = 2;
    private static int PoolCapacity = 4;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPhotoService _photoService;
    private readonly ModerationPhotosPool _pool;
    private readonly ILogger<PhotoModerator> _logger;
    private readonly ModeratorsNotificator _notificator;
    private readonly IMapper _mapper;

    public PhotoModerator(
        IUnitOfWork unitOfWork,
        IPhotoService photoService,
        ModerationPhotosPool pool,
        ILogger<PhotoModerator> logger,
        ModeratorsNotificator notificator,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _photoService = photoService;
        _pool = pool;
        _logger = logger;
        _notificator = notificator;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PhotoDto>> GetCurrentPoolAsync()
    {
        using var accessor = await _pool.GetOperationAccessorAsync();
        if (accessor.Capacity == 0)
        {
            accessor.Capacity = PoolCapacity;
            await FillPool(accessor);
        }

        return accessor.GetPhotos(PoolSize);
    }

    /// <summary>
    /// Removes the photo from the pool and returns the replacement photo to moderate
    /// </summary>
    /// <returns>The photo for moderation instead of moderated photo</returns>
    public async Task<PhotoDto?> ModeratePhoto(int photoId, bool approved)
    {
        var photoToApprove = await _unitOfWork.PhotosRepository.GetModerationPhotoAsync(photoId);

        if (photoToApprove == null) throw new ArgumentException("The photo not found");

        var appUser = photoToApprove.AppUser;
        if (appUser == null) throw new ArgumentException("The photo owner user not found");

        if (approved)
        {
            var isFirstPhoto = appUser.Photos.Count == 0;

            appUser.Photos.Add(new Photo()
            {
                Url = photoToApprove.Url,
                PublicId = photoToApprove.PublicId,
                IsMain = isFirstPhoto,
            });
        }
        else if (!string.IsNullOrEmpty(photoToApprove.PublicId))
        {
            var deletionResult = await _photoService.DeletePhotoAsync(photoToApprove.PublicId);
            if (deletionResult.Error != null) throw new ArgumentException(deletionResult.Error.Message);
        }

        var removed = appUser.PhotosToModerate.Remove(photoToApprove);
        if (!removed || !await _unitOfWork.Complete()) throw new ArgumentException("Failed to moderate the photo");

        try
        {
            var replacement = await RemovePhotoFromPool(photoId);
            await _notificator.NotifyPhotoModerated(photoId, replacement);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error notifying than photo has moderated");
        }

        return null;
    }

    public async Task<PhotoDto> AddModerationPhotoAsync(int userId, IFormFile file)
    {
        var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId, IncludeProperty.ModerationPhotos);
        if (user == null) throw new ArgumentException("User not found");

        var result = await _photoService.AddPhotoAsync(file);

        if (result.Error != null) throw new ArgumentException(result.Error.Message);

        var photo = new ModerationPhoto()
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
            AppUser = user,
        };

        user.PhotosToModerate.Add(photo);

        if (!await _unitOfWork.Complete()) throw new ArgumentException("Problem adding photo");

        // add the photo to the pool if it is not full
        try
        {
            var photoDto = _mapper.Map<PhotoDto>(photo);
            using (var accessor = await _pool.GetOperationAccessorAsync())
            {
                if (accessor.ItemsCount < PoolSize)
                {
                    accessor.AddPhoto(photoDto);
                    await _notificator.NotifyPhotoAdded(photoDto);
                }
            }

            return photoDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying moderation hub about an added photo");
        }

        return new PhotoDto()
        {
            Id = photo.Id,
            Url = photo.Url,
            IsMain = false,
        };
    }

    private async Task<PhotoDto?> RemovePhotoFromPool(int photoId)
    {
        using var accessor = await _pool.GetOperationAccessorAsync();
                
        var index = accessor.FindIndex(photoId);
        if (index == -1) return null;

        var itemsCount = accessor.ItemsCount;
        accessor[index] = accessor[itemsCount - 1];
        accessor.RemoveAt(itemsCount - 1);
        itemsCount = accessor.ItemsCount;
                
        if (itemsCount >= PoolSize) return accessor[index];

        if (itemsCount == PoolSize - 1)
        {
            await FillPool(accessor);
            if (accessor.ItemsCount >= PoolSize) return accessor[PoolSize - 1];
        }

        return null;
    }

    private async Task FillPool(ModerationPhotosPool.IOperationAccessor accessor)
    {
        var itemsCount = accessor.ItemsCount;
        Debug.Assert(itemsCount < PoolCapacity);

        var photosToModerate = await _unitOfWork.PhotosRepository.GetModerationPhotoDtosAsync(itemsCount, PoolCapacity - accessor.ItemsCount);
        
        accessor.AddPhotos(photosToModerate);
    }
}
