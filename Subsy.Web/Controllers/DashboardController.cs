using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Subsy.Application.Subscriptions.Queries.GetSubscriptionDashboard;

namespace Subsy.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly GetSubscriptionDashboardHandler _handler;

    public DashboardController(GetSubscriptionDashboardHandler handler)
    {
        _handler = handler;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var dashboard = await _handler.HandleAsync(new GetSubscriptionDashboardQuery(userId), ct);

        return Json(dashboard); // şimdilik JSON kalsın
    }
}
