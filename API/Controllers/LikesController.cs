using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController : BaseApiController
{
    private readonly IUnitOfWork _uow;

    public LikesController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpPost("{username}")]
    public async Task<ActionResult> AddLike(string username)
    {
        var sourceUserId = User.GetUserId();
        var likedUser = await _uow.UserRepository.GetUserByUsernameAsync(username);
        if (likedUser == null) return NotFound();

        var sourceUser = await _uow.LikesRepository.GetUserWithLikes(sourceUserId);
        if (sourceUser == null) return NotFound();
        if (sourceUser.UserName == username) return BadRequest("A user can not like himself");

        var userLike = await _uow.LikesRepository.GetUserLike(sourceUserId, likedUser.Id);
        if (userLike != null) return BadRequest("The user already liked the target user");
        userLike = new UserLike(sourceUser, likedUser);

        sourceUser.LikedUsers?.Add(userLike);

        if (await _uow.Complete()) return Ok();

        return BadRequest("Failed to like user");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
    {
        likesParams.UserId = User.GetUserId();

        var users = await _uow.LikesRepository.GetUserLikes(likesParams);

        Response.AddPaginationHeader(new PaginationHeader(
            users.CurrentPage, users.PageSize, 
            users.TotalCount, users.TotalPages));

        return Ok(users);
    }
}
