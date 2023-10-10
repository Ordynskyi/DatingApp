namespace API;

public struct UserDto
{
    public UserDto(string username, string token)
    {
        Username = username;
        Token = token;
    }

    public string Username { get; set;}
    public string Token {get;set;}
}
