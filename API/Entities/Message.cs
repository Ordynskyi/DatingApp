namespace API.Entities;

public class Message
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public required string SenderUsername { get; set; }
    public required AppUser Sender { get; set;}

    public int RecipientId { get; set; }
    public required string RecipientUsername { get; set; }
    public required AppUser Recipient { get; set; }
    public required string Content { get; set; }
    public DateTime? DateRead { get; set; }
    public DateTime MessageSent { get; set; } = DateTime.UtcNow;
    public bool SenderDeleted { get; set; }
    public bool RecipientDeleted { get; set; }
}
