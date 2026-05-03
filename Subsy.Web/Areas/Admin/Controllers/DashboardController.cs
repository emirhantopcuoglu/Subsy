using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subsy.Application.Admin.Queries.GetAdminStats;

namespace Subsy.Web.Areas.Admin.Controllers;

public sealed class DashboardController : AdminBaseController
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var stats = await _mediator.Send(new GetAdminStatsQuery(), ct);
        return View(stats);
    }
}
