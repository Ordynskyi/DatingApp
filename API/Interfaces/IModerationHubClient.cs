namespace API.Interfaces
{
    public interface IModerationHubClient
    {
        Task PhotoModerated(int photoId, PhotoDto? replacement);
        Task PhotoAdded(PhotoDto photo);
        Task ReceiveModerationPool(IEnumerable<PhotoDto> photo);
    }
}
