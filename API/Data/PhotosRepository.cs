using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotosRepository : IPhotosRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public PhotosRepository(
            DataContext context,
            IMapper mapper) 
        { 
            _context = context;
            _mapper = mapper;
        }

        public async Task<ModerationPhoto?> GetModerationPhotoAsync(int photoId)
        {
            return await _context.PhotosToModerate
                .Include(p => p.AppUser.Photos)
                .Include(p => p.AppUser.PhotosToModerate)
                .Where(p => p.Id == photoId)
                .FirstOrDefaultAsync();
        }

        public async Task<IList<PhotoDto>> GetModerationPhotoDtosAsync(int startIndex, int count)
        {
            return await _context.PhotosToModerate
                .OrderBy(p => p.Id)
                .Skip(startIndex)
                .Take(count)
                .ProjectTo<PhotoDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
    }
}
