namespace API;

public struct UserDto
{
    public UserDto(string username, string token, string photoUrl)
    {
        Username = username;
        Token = token;
        PhotoUrl = photoUrl;
    }

    public string Username { get; set;}
    public string Token { get; set; }
    public string PhotoUrl { get; set; }
}
