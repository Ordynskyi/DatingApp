using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(DataContext context)
    {
        if (await context.Users.AnyAsync()) return;

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
            using var hmac = new HMACSHA512();
            user.Username = user.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("password"));
            user.PasswordSalt = hmac.Key;

            context.Users.Add(user);
        }

        await context.SaveChangesAsync();
    }
}
