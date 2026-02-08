using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Subsy.Application.Subscriptions.Dashboard;
using System.Security.Claims;

namespace Subsy.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ISubscriptionDashboardService _dashboardService;
    private readonly UserManager<IdentityUser> _userManager;

    public DashboardController(
        ISubscriptionDashboardService dashboardService,
        UserManager<IdentityUser> userManager)
    {
        _dashboardService = dashboardService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var dashboard = await _dashboardService.GetDashboardAsync(userId!);

        return Json(dashboard); 
    }
}
