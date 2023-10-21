namespace API.DTOs;

public class MemberDto
{
    public MemberDto(
        int id,
        string username)
    {
        Id = id;
        Username = username;
    }

    public int Id { get; set; }
    public string Username { get; set;}
    public string PhotoUrl { get; set; } = string.Empty;
    public int Age { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime LastActive { get; set; } = DateTime.UtcNow;
    public string Introduction { get; set; } = string.Empty;
    public string LookingFor { get; set; } = string.Empty;
    public string Interests { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public List<PhotoDto> Photos { get; set; } = new ();
}
