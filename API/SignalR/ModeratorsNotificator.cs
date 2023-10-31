using API.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class ModeratorsNotificator
    {
        public static string ModeratorsGroupName = "Moderators";

        private readonly IHubContext<ModerationHub, IModerationHubClient> _hubContext;

        public ModeratorsNotificator(IHubContext<ModerationHub, IModerationHubClient> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task NotifyPhotoModerated(int photoId, PhotoDto? replacement)
        {
            return _hubContext.Clients.Groups(ModeratorsGroupName).PhotoModerated(photoId, replacement);
        }

        public Task NotifyPhotoAdded(PhotoDto photo)
        {
            return _hubContext.Clients.Groups(ModeratorsGroupName).PhotoAdded(photo);
        }
    }
}
