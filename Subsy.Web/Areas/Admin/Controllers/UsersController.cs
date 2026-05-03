using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subsy.Application.Admin.Commands.AssignAdminRole;
using Subsy.Application.Admin.Commands.RevokeAdminRole;
using Subsy.Application.Admin.Queries.GetAllUsers;

namespace Subsy.Web.Areas.Admin.Controllers;

public sealed class UsersController : AdminBaseController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var users = await _mediator.Send(new GetAllUsersQuery(), ct);
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignAdmin(string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest();

        await _mediator.Send(new AssignAdminRoleCommand(userId), ct);
        TempData["FlashSuccess"] = "Kullanıcıya admin rolü atandı.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeAdmin(string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest();

        await _mediator.Send(new RevokeAdminRoleCommand(userId), ct);
        TempData["FlashSuccess"] = "Kullanıcının admin rolü kaldırıldı.";
        return RedirectToAction(nameof(Index));
    }
}
