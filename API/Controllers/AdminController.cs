using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPhotoService _photoService;

    public AdminController(
        UserManager<AppUser> userManager, 
        IUnitOfWork unitOfWork,
        IPhotoService photoService)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _photoService = photoService;
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.UserName)
            .Select(u => new 
            {
                u.Id,
                Username = u.UserName,
                Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
            })
            .ToListAsync();

        return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, [FromQuery]string roles)
    {
        if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");

        var selectedRoles = roles.Split(",").ToArray();

        var user = await _userManager.FindByNameAsync(username);
        
        if (user == null) return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);

        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
        
        if (!result.Succeeded) return BadRequest("Failed to add to roles");

        result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

        if (!result.Succeeded) return BadRequest("Failed to remove from roles");

        return Ok(await _userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("moderation-photos")]
    public async Task<ActionResult<PagedList<PhotoDto>>> GetPhotosForModeration([FromQuery] PaginationParams paginationParams)
    {
        return await _unitOfWork.PhotosRepository
            .GetModerationPhotoDtosAsync(paginationParams.PageNumber, paginationParams.PageSize);
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPut("moderate-photo")]
    public async Task<ActionResult> ModeratePhoto(int photoId, bool approved)
    {
        var photoToApprove = await _unitOfWork.PhotosRepository.GetModerationPhotoAsync(photoId);

        if (photoToApprove == null) return BadRequest("The photo not found");

        var appUser = photoToApprove.AppUser;
        if (appUser == null) return BadRequest("The photo owner user not found");

        if (approved)
        {
            var isFirstPhoto = appUser.Photos.Count == 0;

            appUser.Photos.Add(new Photo()
            {
                Url = photoToApprove.Url,
                PublicId = photoToApprove.PublicId,
                IsMain = isFirstPhoto,
            });
        } 
        else
        {
            var deletionResult = await _photoService.DeletePhotoAsync(photoToApprove.PublicId);
            if (deletionResult.Error != null) return BadRequest(deletionResult.Error.Message);
        }

        var removed = appUser.PhotosToModerate.Remove(photoToApprove);


        if (removed && await _unitOfWork.Complete()) return Ok();

        return BadRequest("Failed to moderate the photo");
    }
}
