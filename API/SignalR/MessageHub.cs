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

    public MessageHub(
        IMessageRepository messageRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _mapper = mapper;
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

        var messages = await _messageRepository
            .GetMessageThread(username, otherUsername);

        await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
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

        _messageRepository.AddMessage(message);
        if (await _messageRepository.SaveAllAsync()) 
        {
            var groupName = GetGroupName(username, dto.RecipientUsername);
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
}
