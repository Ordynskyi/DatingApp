namespace API.Entities;

public class AppUser
{
    public AppUser(int id, string userName, byte[] passwordHash, byte[] passwordSalt)
    {
        Id = id;
        UserName = userName;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
    }

    public int Id { get; set; }
    public string UserName { get; set;}
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
}
