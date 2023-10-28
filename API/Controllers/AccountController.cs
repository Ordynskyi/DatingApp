using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountController(UserManager<AppUser> userManager, 
        ITokenService tokenService, IMapper mapper)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    [HttpPost("register")] // POST: api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        var lowerName = registerDto.Username.ToLower();
        if (await UserExists(lowerName))
        {
            return BadRequest("User name is taken");
        }

        var user = _mapper.Map<AppUser>(registerDto);     

        user.UserName = lowerName;
        
        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded) 
        {
            return BadRequest(result.Errors);
        }
        
        var roleResults = await _userManager.AddToRoleAsync(user, "Member");
        if (!roleResults.Succeeded) return BadRequest(result.Errors);

        return new UserDto(
            user.UserName,
            await _tokenService.CreateToken(user),
            GetMainPhotoUrlOrDefault(user),
            user.DisplayName,
            user.Gender);
    }

    [HttpPost("login")] // POST: api/account/login
    public async Task <ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var lowerName = loginDto.Username.ToLower();
        var user = await _userManager.Users
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(x => x.UserName == lowerName);
            
        if (user == null) return Unauthorized("user not found");

        var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result) return Unauthorized("Invalid password");

        return new UserDto(
            user.UserName,
            await _tokenService.CreateToken(user),
            GetMainPhotoUrlOrDefault(user),
            user.DisplayName,
            user.Gender);
    }

    private async Task<bool> UserExists(string username)
    {
        return await _userManager.Users.AnyAsync(user => user.UserName == username);
    }

    private string? GetMainPhotoUrlOrDefault(AppUser user)
    {
        if (user.Photos == null) return null;
        foreach (var photo in user.Photos)
        {
            if (photo.IsMain) return photo.Url;
        }

        return string.Empty;
    }
}