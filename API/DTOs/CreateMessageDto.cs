namespace API.DTOs;

public struct CreateMessageDto
{
    public string RecipientUsername { get; set; }
    public string Content { get; set; }
}
