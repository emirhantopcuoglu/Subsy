using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subsy.Application.Admin.Queries.GetAllSubscriptionsAdmin;

namespace Subsy.Web.Areas.Admin.Controllers;

public sealed class SubscriptionsController : AdminBaseController
{
    private readonly IMediator _mediator;

    public SubscriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var subscriptions = await _mediator.Send(new GetAllSubscriptionsAdminQuery(), ct);
        return View(subscriptions);
    }
}
