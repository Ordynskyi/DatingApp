using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly ILikesRepository _likesRepository;

    public LikesController(
        IUserRepository userRepository,
        ILikesRepository likesRepository)
    {
        _userRepository = userRepository;
        _likesRepository = likesRepository;
    }

    [HttpPost("{username}")]
    public async Task<ActionResult> AddLike(string username)
    {
        var sourceUserId = User.GetUserId();
        var likedUser = await _userRepository.GetUserByUsernameAsync(username);
        if (likedUser == null) return NotFound();

        var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);
        if (sourceUser == null) return NotFound();
        if (sourceUser.Username == username) return BadRequest("A user can not like himself");

        var userLike = await _likesRepository.GetUserLike(sourceUserId, likedUser.Id);
        if (userLike != null) return BadRequest("The user already liked the target user");
        userLike = new UserLike(sourceUser, likedUser);

        sourceUser.LikedUsers.Add(userLike);

        if (await _userRepository.SaveAllAsync()) return Ok();

        return BadRequest("Failed to like user");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes(string predicate)
    {
        var users = await _likesRepository.GetUserLikes(predicate, User.GetUserId());

        return Ok(users);
    }
}
