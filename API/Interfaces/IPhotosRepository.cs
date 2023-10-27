using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IPhotosRepository
    {
        Task<PagedList<PhotoDto>> GetModerationPhotoDtosAsync(int page, int pageSize);
        Task<ModerationPhoto?> GetModerationPhotoAsync(int photoId);
    }
}
