using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessagesController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    public MessagesController(
        IUserRepository userRepository, 
        IMessageRepository messageRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _messageRepository = messageRepository;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto dto)
    {
        var username = User.GetUsername();

        if (username == null) return BadRequest("Sender name not found");

        if (username == dto.RecipientUsername.ToLower()) {
            return BadRequest("User can not send a message to himself");
        }

        var sender = await _userRepository.GetUserByUsernameAsync(username);
        if (sender == null) return BadRequest("Sender not found");

        var recipient = await _userRepository.GetUserByUsernameAsync(dto.RecipientUsername);
        if (recipient == null) return BadRequest("Recipient not found");

        var message = new Message 
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.Username,
            RecipientUsername = recipient.Username,
            Content = dto.Content
        };

        _messageRepository.AddMessage(message);
        if (await _messageRepository.SaveAllAsync()) 
        {
            return Ok(_mapper.Map<MessageDto>(message));
        }

        return BadRequest("Failed to send the message");
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser(
        [FromQuery] MessageParams messageParams)
    {
        var username = User.GetUsername();
        if (username == null) return BadRequest("username not found");
        messageParams.Username = username;

        var messages = await _messageRepository.GetMessagesForUser(messageParams);

        Response.AddPaginationHeader(new PaginationHeader(
            messages.CurrentPage,
            messages.PageSize,
            messages.TotalCount,
            messages.TotalPages));

        return messages;    
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
        var currentUsername = User.GetUsername();

        if (currentUsername == null) return BadRequest("Sender username not found");

        return Ok(await _messageRepository.GetMessageThread(currentUsername, username));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUsername();
        if (username == null) return BadRequest("client's username not found");

        var message = await _messageRepository.GetMessage(id);
        if (message == null) return BadRequest("the message not found");

        if (message.SenderUsername != username && message.RecipientUsername != username)
        {
            return Unauthorized("the client can not delete this message");
        }

        if (message.SenderUsername == username) message.SenderDeleted = true;
        if (message.RecipientUsername == username) message.RecipientDeleted = true;

        if (message.SenderDeleted && message.RecipientDeleted)
        {
            _messageRepository.DeleteMessage(message);
        }

        if (await _messageRepository.SaveAllAsync()) return Ok();

        return BadRequest("Problem deleting the message");
    }
}
