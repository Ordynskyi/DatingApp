using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, 
        ActionExecutionDelegate next)
    {
        var resultContext = await next();

        var user = resultContext.HttpContext.User;
        var identity = user.Identity;

        if (identity == null || !identity.IsAuthenticated) return;

        var userId = user.GetUserId();
        if (userId == -1) return;

        var uow = resultContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
        var appUser = await uow.UserRepository.GetUserByIdAsync(userId);
        if (appUser == null) return;
        appUser.LastActive = DateTime.UtcNow;
        await uow.Complete();
    }
}
