using API.Helpers;

namespace API;

public class MessageParams : PaginationParams
{
    public string Username { get; set; } = string.Empty;
    public string Container { get; set; } = "Unread";
}
