namespace API;

public struct UserDto
{
    public UserDto(string userName, string token)
    {
        UserName = userName;
        Token = token;
    }

    public string UserName { get; set;}
    public string Token {get;set;}
}
