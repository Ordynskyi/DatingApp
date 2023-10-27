using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using static API.Interfaces.IUserRepository;

namespace API.Data;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UserRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await _context.Users
            .Include(p => p.Photos)
            .ToListAsync();
    }

    public async Task<AppUser?> GetUserByIdAsync(int id, IncludeProperty includeFlags = IncludeProperty.None)
    {
        if (includeFlags == 0) return await _context.Users.FindAsync(id);
        
        var query = _context.Users.AsQueryable();
        
        if ((includeFlags & IncludeProperty.Photos) != 0)
        {
            query = query.Include(p => p.Photos);
        }

        if ((includeFlags & IncludeProperty.ModeratePhotos) != 0)
        {
            query = query.Include(p => p.PhotosToModerate);
        }

        return await query.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<AppUser?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(x => x.UserName == username);
    }

    public void Update(AppUser user)
    {
        _context.Entry(user).State = EntityState.Modified;
    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
        var minDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
        var maxDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

        var query = _context.Users.AsQueryable()
            .Where(u => u.UserName != userParams.CurrentUsername &&
                        u.Gender == userParams.Gender &&
                        u.DateOfBirth >= minDate && u.DateOfBirth <= maxDate)
            .Select(CreateMemberDtoExpression(IncludeProperty.Photos));

        query = userParams.OrderBy switch
        {
            "created" => query.OrderByDescending(u => u.Created),
            _ => query.OrderByDescending(u => u.LastActive)
        };

        return await PagedList<MemberDto>.CreateAsync(
            query.AsNoTracking(),
            userParams.PageNumber,
            userParams.PageSize);
    }

    private Expression<Func<AppUser,MemberDto>> CreateMemberDtoExpression(IncludeProperty includePropFlags)
    {
        var includePhotos = (includePropFlags & IncludeProperty.Photos) != 0;
        var includeModeratePhotos = (includePropFlags & IncludeProperty.ModeratePhotos) != 0;

        return u => new MemberDto(u.Id, u.UserName)
        {
            Age = u.DateOfBirth.CalculateAge(),
            LastActive = u.LastActive,
            City = u.City,
            Country = u.Country,
            Created = u.Created,
            DisplayName = u.DisplayName,
            Interests = u.Interests,
            Introduction = u.Introduction,
            LookingFor = u.LookingFor,
            Photos = !includePhotos ? Array.Empty<PhotoDto>() :
                    u.Photos
                    .Select(p => new PhotoDto()
                    {
                        Id = p.Id,
                        IsMain = p.IsMain,
                        Url = p.Url,
                    }).ToList(),
            PhotosToModerate = !includeModeratePhotos ? Array.Empty<PhotoDto>() :
                    u.PhotosToModerate
                    .Select(p => new PhotoDto()
                    {
                        Id = p.Id,
                        Url = p.Url,
                    }).ToList(),
#pragma warning disable CS8602 //   Dereference of a possibly null reference.
            PhotoUrl = u.Photos.FirstOrDefault(p => p.IsMain).Url,
#pragma warning restore CS8602 //   Dereference of a possibly null reference.
        };
    }

    public async Task<MemberDto?> GetMemberAsync(string username, IncludeProperty includePropFlags)
    {
        return await _context.Users
            .Where(x => x.UserName == username)
            .Select(CreateMemberDtoExpression(includePropFlags))
            .FirstOrDefaultAsync();
    }

    public async Task<MemberDto?> GetMemberAsync(int userId, IncludeProperty includePropFlags)
    {
        return await _context.Users
            .Where(x => x.Id == userId)
            .Select(CreateMemberDtoExpression(includePropFlags))
            .FirstOrDefaultAsync();
    }

    public async Task<string?> GetUserGender(string username)
    {
        return await _context.Users.Where(x => x.UserName == username)
            .Select(x => x.Gender).FirstOrDefaultAsync();
    }
}
