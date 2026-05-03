using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subsy.Application.Admin.Commands.BroadcastEmail;

namespace Subsy.Web.Areas.Admin.Controllers;

public sealed class NotificationsController : AdminBaseController
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string subject, string body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
        {
            TempData["FlashError"] = "Konu ve içerik boş olamaz.";
            return RedirectToAction(nameof(Index));
        }

        var sent = await _mediator.Send(new BroadcastEmailCommand(subject, body), ct);
        TempData["FlashSuccess"] = $"Duyuru {sent} kullanıcıya başarıyla gönderildi.";
        return RedirectToAction(nameof(Index));
    }
}
