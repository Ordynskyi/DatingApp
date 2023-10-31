using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public abstract class ModerationHubBase : Hub<IModerationHubClient>
    {
        [Authorize(Policy = "ModeratePhotoRole")]
        public abstract Task ModeratePhoto(int photoId, bool approved);
    }
}
