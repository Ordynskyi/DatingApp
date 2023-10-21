namespace API;

public struct UserDto
{
    public UserDto(string username, string token, string photoUrl,
        string displayName)
    {
        Username = username;
        Token = token;
        PhotoUrl = photoUrl;
        DisplayName = displayName;
    }

    public string Username { get; set;}
    public string Token { get; set; }
    public string PhotoUrl { get; set; }
    public string DisplayName { get; set; }
}
