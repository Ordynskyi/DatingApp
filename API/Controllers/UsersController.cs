using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly DataContext _contex;

    public UsersController(DataContext contex)
    {
        _contex = contex;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
    {
        return await _contex.Users.ToListAsync();
    }

    [HttpGet("{id}")]   // /api/users/2
    public async Task<ActionResult<AppUser>> GetUser(int id)
    {
        return await _contex.Users.FindAsync(id);
    }
}
