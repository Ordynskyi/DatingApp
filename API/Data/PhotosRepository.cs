using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotosRepository : IPhotosRepository
    {
        private readonly DataContext _context;
        public PhotosRepository(DataContext context) 
        { 
            _context = context; 
        }

        public async Task<ModerationPhoto?> GetModerationPhotoAsync(int photoId)
        {
            return await _context.PhotosToModerate
                .Include(p => p.AppUser.Photos)
                .Include(p => p.AppUser.PhotosToModerate)
                .Where(p => p.Id == photoId)
                .FirstOrDefaultAsync();
        }

        public async Task<PagedList<PhotoDto>> GetModerationPhotoDtosAsync(int page, int pageSize)
        {
            var query = _context.Users
                .AsNoTracking()
                .SelectMany(u => u.PhotosToModerate)
                .Select(p => new PhotoDto()
                {
                    Id = p.Id,
                    Url = p.Url
                });

            return await PagedList<PhotoDto>.CreateAsync(query, page, pageSize);
        }
    }
}
