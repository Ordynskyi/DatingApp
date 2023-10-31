using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class ModerationHub : ModerationHubBase
    {
        public static string ModeratorsGroupName = "Moderators";
        private readonly PhotoModerator _photoModerator;
        private readonly ModeratorsNotificator _notificator;

        public ModerationHub(PhotoModerator photoModerator, ModeratorsNotificator notificator)
        {
            _photoModerator = photoModerator;
            _notificator = notificator;
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        public override Task ModeratePhoto(int photoId, bool approved)
        {
            try
            {
                return _photoModerator.ModeratePhoto(photoId, approved);
            }
            catch (ArgumentException e)
            {
                throw new HubException(e.Message);
            }
        }

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ModeratorsGroupName);
            var pool = await _photoModerator.GetCurrentPoolAsync();

            await Clients.Caller.ReceiveModerationPool(pool);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ModeratorsGroupName);
        }
    }
}
