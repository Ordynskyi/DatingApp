using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class MessageHub : Hub
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IHubContext<PresenceHub> _presenceHub;
    private readonly PresenceTracker _presenceTracker;

    public MessageHub(
        IMessageRepository messageRepository,
        IUserRepository userRepository,
        IMapper mapper,
        IHubContext<PresenceHub> presenceHub,
        PresenceTracker presenceTracker)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _presenceHub = presenceHub;
        _presenceTracker = presenceTracker;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext == null) return;
        
        var username = Context.User?.GetUsername();
        if (username == null) return;

        var otherUsername = httpContext.Request.Query["user"].ToString();        
        var groupName = GetGroupName(username, otherUsername);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var group = await AddToGroup(username, groupName);

        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await _messageRepository
            .GetMessageThread(username, otherUsername);

        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var group = await RemoveFromMessageGroup();
        if (group != null)
        {
            await Clients.Group(group.Name).SendAsync("UpdatedGroup");
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto dto)
    {
        var user = Context.User;
        if (user == null) throw new HubException("User not found");

        var username = user.GetUsername();

        if (username == null) throw new HubException("Sender name not found");

        if (username == dto.RecipientUsername.ToLower()) {
            throw new HubException("User can not send a message to himself");
        }

        var sender = await _userRepository.GetUserByUsernameAsync(username);
        if (sender == null) throw new HubException("Sender not found");

        var recipient = await _userRepository.GetUserByUsernameAsync(dto.RecipientUsername);
        if (recipient == null) throw new HubException("Recipient not found");

        var message = new Message 
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName ?? string.Empty,
            RecipientUsername = recipient.UserName ?? string.Empty,
            Content = dto.Content
        };

        var groupName = GetGroupName(message.SenderUsername, message.RecipientUsername);
        var group = await _messageRepository.GetMessageGroup(groupName);
        if (group != null && group.Connections.Any(x => x.Username == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            var connections = await _presenceTracker
                .GetConnectionsForUser(message.RecipientUsername);
            if (connections != null)
            {
                await _presenceHub.Clients
                    .Clients(connections)
                    .SendAsync(
                        "NewMessageReceived",
                        new {
                            username = sender.UserName, 
                            displayName = sender.DisplayName
                        });
            }
        }

        _messageRepository.AddMessage(message);
        if (await _messageRepository.SaveAllAsync()) 
        {
            await Clients.Group(groupName)
                .SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
        }
        else throw new HubException("Failed to send the message");
    }

    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other);
        return stringCompare < 0 
            ? $"{caller}-{other}"
            : $"{other}-{caller}";
    }

    private async Task<Group> AddToGroup(string username, string groupName)
    {
        var group = await _messageRepository.GetMessageGroup(groupName);
        var connection = new Connection(Context.ConnectionId, username);

        if (group == null)
        {
            group = new Group(groupName);
            _messageRepository.AddGroup(group);
        }

        group.Connections.Add(connection);

        if (await _messageRepository.SaveAllAsync()) return group;

        throw new HubException("Failed to add to a group");
    }

    private async Task<Group?> RemoveFromMessageGroup()
    {
        var group = await _messageRepository.GetGroupForConnection(Context.ConnectionId);
        if (group == null) return null;
        var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
        if (connection == null) return null;

        _messageRepository.RemoveConnection(connection);
        if (await _messageRepository.SaveAllAsync()) return group;

        throw new HubException("Failed to remove from a group");
    }
}
