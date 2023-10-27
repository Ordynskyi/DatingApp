using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class AppUser : IdentityUser<int>
{
    public DateOnly DateOfBirth { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime LastActive { get; set; } = DateTime.UtcNow;
    public string Introduction { get; set; } = string.Empty;
    public string LookingFor { get; set; } = string.Empty;
    public string Interests { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public required List<Photo> Photos { get; set; }
    public required List<ModerationPhoto> PhotosToModerate { get; set; }

    public List<UserLike>? LikedByUsers { get; set; }
    public List<UserLike>? LikedUsers { get; set; }

    public List<Message>? MessagesSent { get; set; }
    public List<Message>? MessagesReceived { get; set; }

    public ICollection<AppUserRole>? UserRoles { get; set; }

    // public int GetAge() => DateOfBirth.CalculateAge();
}
