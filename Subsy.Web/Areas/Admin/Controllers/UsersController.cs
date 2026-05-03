using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subsy.Application.Admin.Commands.AssignAdminRole;
using Subsy.Application.Admin.Commands.BlockUser;
using Subsy.Application.Admin.Commands.DeleteUser;
using Subsy.Application.Admin.Commands.RevokeAdminRole;
using Subsy.Application.Admin.Commands.UnblockUser;
using Subsy.Application.Admin.Queries.GetAllUsers;
using System.Security.Claims;

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
        if (string.IsNullOrWhiteSpace(userId)) return BadRequest();

        await _mediator.Send(new AssignAdminRoleCommand(userId), ct);
        TempData["FlashSuccess"] = "Kullanıcıya admin rolü atandı.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeAdmin(string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId)) return BadRequest();

        try
        {
            await _mediator.Send(new RevokeAdminRoleCommand(userId, CurrentUserId), ct);
            TempData["FlashSuccess"] = "Kullanıcının admin rolü kaldırıldı.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["FlashError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId)) return BadRequest();

        try
        {
            await _mediator.Send(new BlockUserCommand(userId, CurrentUserId), ct);
            TempData["FlashSuccess"] = "Kullanıcı engellendi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["FlashError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unblock(string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId)) return BadRequest();

        await _mediator.Send(new UnblockUserCommand(userId), ct);
        TempData["FlashSuccess"] = "Kullanıcının engellemesi kaldırıldı.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId)) return BadRequest();

        try
        {
            await _mediator.Send(new DeleteUserCommand(userId, CurrentUserId), ct);
            TempData["FlashSuccess"] = "Kullanıcı ve tüm verileri silindi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["FlashError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
