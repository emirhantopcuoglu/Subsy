using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.RateLimiting;
using Subsy.Application.Common;

namespace Subsy.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
[EnableRateLimiting("admin")]
public abstract class AdminBaseController : Controller
{
    // Authenticated non-admin users receive 404 to avoid disclosing that an admin area exists.
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.User.IsInRole(Roles.Admin))
        {
            context.Result = NotFound();
            return;
        }

        base.OnActionExecuting(context);
    }
}
