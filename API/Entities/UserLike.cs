using System.Diagnostics.CodeAnalysis;

namespace API.Entities;

public class UserLike
{
    public UserLike()
    {
    }

    [SetsRequiredMembers]
    public UserLike(AppUser sourceUser, AppUser targetUser)
    {
        SourceUser = sourceUser;
        SourceUserId = sourceUser.Id;
        TargetUser = targetUser;
        TargetUserId = targetUser.Id;
    }

    public required AppUser SourceUser { get; set; }
    public int SourceUserId { get; set; }
    public required AppUser TargetUser { get; set; }
    public int TargetUserId { get; set; }
}
