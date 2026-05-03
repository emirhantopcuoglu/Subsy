using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Subsy.Application.Common;
using System.Security.Claims;

namespace Subsy.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
[EnableRateLimiting("admin")]
public abstract class AdminBaseController : Controller
{
    // Authenticated non-admin users receive 404 to avoid disclosing that an admin area exists.
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.User.IsInRole(Roles.Admin))
        {
            context.Result = NotFound();
            return;
        }

        // Load 2FA status for the warning banner in the layout
        var userManager = context.HttpContext.RequestServices
            .GetRequiredService<UserManager<IdentityUser>>();
        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
        {
            var user = await userManager.FindByIdAsync(userId);
            ViewData["Admin2faEnabled"] = user?.TwoFactorEnabled ?? false;
        }

        await next();
    }
}
