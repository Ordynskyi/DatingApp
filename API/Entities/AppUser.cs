namespace API.Entities;

public class AppUser
{
    public AppUser(int id, string username, byte[] passwordHash, byte[] passwordSalt)
    {
        Id = id;
        Username = username;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
    }

    public int Id { get; set; }
    public string Username { get; set;}
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
}
