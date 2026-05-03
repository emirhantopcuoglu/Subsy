using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subsy.Application.Admin.Queries.GetAuditLogs;

namespace Subsy.Web.Areas.Admin.Controllers;

public sealed class AuditLogsController : AdminBaseController
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery(search, page, 50), ct);
        ViewBag.Search = search;
        return View(result);
    }
}
