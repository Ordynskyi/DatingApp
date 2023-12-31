﻿using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.SignalR;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static API.Interfaces.IUserRepository;

namespace API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;
    private readonly PhotoModerator _photoModerator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUnitOfWork uow, 
        IMapper mapper,
        IPhotoService photoService,
        PhotoModerator photoModerator,
        ILogger<UsersController> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _photoService = photoService;
        _photoModerator = photoModerator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MemberDto>>> GetUsers(
        [FromQuery]UserParams userParams)
    {
        var username = User.GetUsername() ?? string.Empty;
        if (username == null) return BadRequest("Username not found");
        
        userParams.CurrentUsername = username;
        var gender = await _uow.UserRepository.GetUserGender(username);
        if (string.IsNullOrEmpty(userParams.Gender)) 
        {
            userParams.Gender = gender == "male" 
                ? "female"
                : "male";
        }

        var members = await _uow.UserRepository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(
            new PaginationHeader(
                members.CurrentPage, members.PageSize,
                members.TotalCount, members.TotalPages));

        return Ok(members);
    }

    [HttpGet("{username}")]   // /api/users/2
    public async Task<ActionResult<MemberDto?>> GetUser(string username)
    {
        return await _uow.UserRepository.GetMemberAsync(username, IncludeProperty.Photos);
    }

    [HttpGet("authorized-user")]   // /api/authorized-user
    public async Task<ActionResult<MemberDto?>> GetAuthorizedUser()
    {
        return await _uow.UserRepository.GetMemberAsync(User.GetUserId(), IncludeProperty.Photos | IncludeProperty.ModerationPhotos);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await GetUserOrDefaultAsync(IncludeProperty.None);
        if (user == null) return NotFound();

        _mapper.Map(memberUpdateDto, user);

        if (await _uow.Complete()) return NoContent();

        return BadRequest("Failed to update the user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file) 
    {
        try
        {
            var photoDto = await _photoModerator.AddModerationPhotoAsync(User.GetUserId(), file);
            return CreatedAtAction(nameof(GetAuthorizedUser), new {username = User.GetUsername()}, photoDto);
        }
        catch(ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await GetUserOrDefaultAsync(IncludeProperty.Photos);
        if (user == null) return NotFound();

        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null) return NotFound();

        if (photo.IsMain) return BadRequest("this is already the main photo");

        var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);
        if (currentMain != null) currentMain.IsMain = false;
        photo.IsMain = true;

        if (await _uow.Complete()) return NoContent();

        return BadRequest("Problem setting the main photo");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await GetUserOrDefaultAsync(IncludeProperty.Photos);
        if (user == null) return NotFound();

        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null) return NotFound();

        if (photo.IsMain) return BadRequest("You can not delete your main photo");

        if (photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);

        if (await _uow.Complete()) return Ok();

        return BadRequest("Problem deleting photo");
    }

    [HttpDelete("delete-moderation-photo/{photoId}")]
    public async Task<ActionResult<IEnumerable<AppUser>>> DeleteModerationPhoto(int photoId)
    {
        try
        {
            await _photoModerator.ModeratePhoto(photoId, false);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        return Ok();
    }

    private async Task<AppUser?> GetUserOrDefaultAsync(IncludeProperty propFlags)
    {
        var username = User.GetUsername();
        if (string.IsNullOrEmpty(username)) return null;

        return await _uow.UserRepository.GetUserByIdAsync(User.GetUserId(), propFlags);
    }
}
