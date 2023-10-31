using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IPhotosRepository
    {
        Task<ModerationPhoto?> GetModerationPhotoAsync(int photoId);
        Task<IList<PhotoDto>> GetModerationPhotoDtosAsync(int startIndex, int count);
    }
}
