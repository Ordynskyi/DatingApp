namespace API;

public struct UserDto
{
    public UserDto(string? username, string token, string? photoUrl,
        string displayName, string gender)
    {
        Username = username;
        Token = token;
        PhotoUrl = photoUrl;
        DisplayName = displayName;
        Gender = gender;
    }

    public string? Username { get; set;}
    public string Token { get; set; }
    public string? PhotoUrl { get; set; }
    public string DisplayName { get; set; }
    public string Gender { get; set; }
}
