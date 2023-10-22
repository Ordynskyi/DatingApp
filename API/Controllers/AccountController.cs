using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseApiController
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountController(DataContext context, 
        ITokenService tokenService, IMapper mapper)
    {
        _context = context;
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

        using var hmac = new HMACSHA512();

        user.Username = lowerName;
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
        user.PasswordSalt = hmac.Key;
        
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        
        return new UserDto(
            user.Username,
            _tokenService.CreateToken(user),
            GetMainPhotoUrlOrEmpty(user),
            user.DisplayName,
            user.Gender);
    }

    [HttpPost("login")] // POST: api/account/login
    public async Task <ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var lowerName = loginDto.Username.ToLower();
        var user = await _context.Users
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(x => x.Username == lowerName);
            
        if (user == null) return Unauthorized("user not found");

        using var hmac = new HMACSHA512(user.PasswordSalt);
        var calculatedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < calculatedHash.Length; i++)
        {
            if (calculatedHash[i] != user.PasswordHash[i]) return Unauthorized("invalid password");
        }

        return new UserDto(
            user.Username,
            _tokenService.CreateToken(user),
            GetMainPhotoUrlOrEmpty(user),
            user.DisplayName,
            user.Gender);
    }

    private async Task<bool> UserExists(string username)
    {
        return await _context.Users.AnyAsync(user => user.Username == username);
    }

    private string GetMainPhotoUrlOrEmpty(AppUser user)
    {
        foreach (var photo in user.Photos)
        {
            if (photo.IsMain) return photo.Url;
        }

        return string.Empty;
    }
}
