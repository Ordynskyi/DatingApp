using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IUserRepository
{
    [Flags]
    public enum IncludeProperty
    {
        None = 0,
        Photos = 1,
        ModeratePhotos = 1 << 1,
    }

    void Update(AppUser user);
    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<AppUser?> GetUserByIdAsync(int id, IncludeProperty includeFlags = IncludeProperty.None);
    Task<AppUser?> GetUserByUsernameAsync(string username);
    Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
    Task<MemberDto?> GetMemberAsync(string username, IncludeProperty includePropFlags);
    Task<MemberDto?> GetMemberAsync(int userId, IncludeProperty includePropFlags);
    Task<string?> GetUserGender(string username);
} 
