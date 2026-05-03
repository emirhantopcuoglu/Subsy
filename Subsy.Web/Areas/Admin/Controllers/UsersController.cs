using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Subsy.Application.Admin.Commands.AssignAdminRole;
using Subsy.Application.Admin.Commands.BlockUser;
using Subsy.Application.Admin.Commands.DeleteUser;
using Subsy.Application.Admin.Commands.ForceLogout;
using Subsy.Application.Admin.Commands.RevokeAdminRole;
using Subsy.Application.Admin.Commands.SendPasswordResetEmail;
using Subsy.Application.Admin.Commands.UnblockUser;
using Subsy.Application.Admin.Queries.GetAllUsers;
using Subsy.Application.Admin.Queries.GetUserDetail;
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

    [HttpGet]
    public async Task<IActionResult> Details(string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId)) return BadRequest();

        var detail = await _mediator.Send(new GetUserDetailQuery(userId), ct);
        return View(detail);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForceLogout(string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId)) return BadRequest();

        try
        {
            await _mediator.Send(new ForceLogoutCommand(userId, CurrentUserId), ct);
            TempData["FlashSuccess"] = "Kullanıcının oturumu sonlandırıldı.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["FlashError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendPasswordReset(string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId)) return BadRequest();

        try
        {
            var userManager = HttpContext.RequestServices
                .GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser>>();

            var user = await userManager.FindByIdAsync(userId);
            if (user?.Email is null)
            {
                TempData["FlashError"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(
                "ResetPassword", "Account",
                new { userId = user.Id, token },
                Request.Scheme)!;

            await _mediator.Send(new SendPasswordResetEmailCommand(userId, callbackUrl), ct);
            TempData["FlashSuccess"] = $"{user.UserName} kullanıcısına parola sıfırlama bağlantısı gönderildi.";
        }
        catch (Exception ex)
        {
            TempData["FlashError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
