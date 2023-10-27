using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

[Table("ModerationPhotos")]
public class ModerationPhoto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;

    [ForeignKey(nameof(AppUser))]
    public int AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
}

