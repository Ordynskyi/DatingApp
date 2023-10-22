using System.Security.Claims;

namespace API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUsername(this ClaimsPrincipal user) 
    {
        return user.FindFirstValue(ClaimTypes.Name);
    }

    
    /// <returns>The user Id or -1 if the operation has failed.</returns>
    public static int GetUserId(this ClaimsPrincipal user) 
    {
        var idString = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idString, out var result) 
            ? result 
            : -1;
    }
}
