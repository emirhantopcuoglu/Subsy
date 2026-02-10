using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subsy.Application.Subscriptions.Queries.GetSubscriptionDashboard;
using System.Security.Claims;

namespace Subsy.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var dashboard = await _mediator.Send(new GetSubscriptionDashboardQuery(userId), ct);

        return Json(dashboard);
    }
}
