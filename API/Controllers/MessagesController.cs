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
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public MessagesController(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
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

        var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
        if (sender == null) return BadRequest("Sender not found");

        var recipient = await _uow.UserRepository.GetUserByUsernameAsync(dto.RecipientUsername);
        if (recipient == null) return BadRequest("Recipient not found");

        var message = new Message 
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName ?? string.Empty,
            RecipientUsername = recipient.UserName ?? string.Empty,
            Content = dto.Content
        };

        _uow.MessageRepository.AddMessage(message);
        if (await _uow.Complete()) 
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

        var messages = await _uow.MessageRepository.GetMessagesForUser(messageParams);

        Response.AddPaginationHeader(new PaginationHeader(
            messages.CurrentPage,
            messages.PageSize,
            messages.TotalCount,
            messages.TotalPages));

        return messages;    
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUsername();
        if (username == null) return BadRequest("client's username not found");

        var message = await _uow.MessageRepository.GetMessage(id);
        if (message == null) return BadRequest("the message not found");

        if (message.SenderUsername != username && message.RecipientUsername != username)
        {
            return Unauthorized("the client can not delete this message");
        }

        if (message.SenderUsername == username) message.SenderDeleted = true;
        if (message.RecipientUsername == username) message.RecipientDeleted = true;

        if (message.SenderDeleted && message.RecipientDeleted)
        {
            _uow.MessageRepository.DeleteMessage(message);
        }

        if (await _uow.Complete()) return Ok();

        return BadRequest("Problem deleting the message");
    }
}
