using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(UserManager<AppUser> userManager, ILogger? logger)
    {
        if (await userManager.Users.AnyAsync()) return;

        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

        if (userData == null) 
        {
            throw new ArgumentException("Could not get the users data");
        }

        var options = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        };

        var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);

        if (users == null) 
        {
            throw new ArgumentException("Could not get users from the users data");            
        }

        foreach (var user in users)
        {
            user.UserName = user.UserName?.ToLower() ?? string.Empty;
            
            var result = await userManager.CreateAsync(user, "password");
            if (logger != null && !result.Succeeded) {
                foreach (var error in result.Errors) 
                {
                    logger.LogError(error.Description);
                }                
            }
        }
    }
}
