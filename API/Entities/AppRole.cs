using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class AppRole : IdentityRole<int>
{
    public required ICollection<AppUserRole> UserRoles { get; set; }
}
