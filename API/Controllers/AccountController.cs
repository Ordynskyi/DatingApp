using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseApiController
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;

    public AccountController(DataContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")] // POST: api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        var lowerName = registerDto.UserName.ToLower();
        if (await UserExists(lowerName))
        {
            return BadRequest("User name is taken");
        }

        using var hmac = new HMACSHA512();

        var user = new AppUser(0, lowerName, 
            hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            hmac.Key
        );
        
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        
        return new UserDto(user.UserName, _tokenService.CreateToken(user));
    }

    [HttpPost("login")] // POST: api/account/login
    public async Task <ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var lowerName = loginDto.UserName.ToLower();
        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == lowerName);
        if (user == null) return Unauthorized("user not found");

        using var hmac = new HMACSHA512(user.PasswordSalt);
        var calculatedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < calculatedHash.Length; i++)
        {
            if (calculatedHash[i] != user.PasswordHash[i]) return Unauthorized("invalid password");
        }

        return new UserDto(user.UserName, _tokenService.CreateToken(user));
    }

    private async Task<bool> UserExists(string userName)
    {
        return await _context.Users.AnyAsync(user => user.UserName == userName);
    }
}
