using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subsy.Application.Subscriptions.Commands.ArchiveSubscription;
using Subsy.Application.Subscriptions.Commands.CreateSubscription;
using Subsy.Application.Subscriptions.Commands.DeleteSubscription;
using Subsy.Application.Subscriptions.Commands.MarkSubscriptionAsPaid;
using Subsy.Application.Subscriptions.Commands.UpdateSubscription;
using Subsy.Application.Subscriptions.Queries.Common;
using Subsy.Application.Subscriptions.Queries.GetActiveSubscriptions;
using Subsy.Application.Subscriptions.Queries.GetArchivedSubscriptions;
using Subsy.Application.Subscriptions.Queries.GetDueSubscriptions;
using Subsy.Application.Subscriptions.Queries.GetUserSubscriptions;
using Subsy.Web.Models;
using System.Security.Claims;

namespace Subsy.Web.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly IMediator _mediator;

        public SubscriptionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var dtos = await _mediator.Send(new GetUserSubscriptionsQuery(userId), ct);
            var vms = dtos.Select(MapToVm).ToList();

            return View(vms);
        }

        [HttpGet]
        public async Task<IActionResult> Active(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var dtos = await _mediator.Send(new GetActiveSubscriptionsQuery(userId), ct);
            return View(dtos.Select(MapToVm).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Due(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var dtos = await _mediator.Send(new GetDueSubscriptionsQuery(userId), ct);
            return View(dtos.Select(MapToVm).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Archived(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var dtos = await _mediator.Send(new GetArchivedSubscriptionsQuery(userId), ct);
            return View(dtos.Select(MapToVm).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            SubscriptionsViewModel vm,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                await _mediator.Send(
                    new CreateSubscriptionCommand(
                        userId,
                        vm.Name,
                        vm.Price,
                        vm.RenewalPeriod,
                        vm.RenewalDate),
                    ct);

                TempData["CreateMessage"] = "Abonelik başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(nameof(vm.RenewalDate), ex.Message);
                return View(vm);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(
            SubscriptionsViewModel vm,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                await _mediator.Send(
                    new UpdateSubscriptionCommand(
                        vm.Id,
                        userId,
                        vm.Name,
                        vm.Price,
                        vm.RenewalPeriod,
                        vm.RenewalDate),
                    ct);

                TempData["UpdateMessage"] = "Abonelik başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(nameof(vm.RenewalDate), ex.Message);
                return View(vm);
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsPaid(int id, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                await _mediator.Send(
                    new MarkSubscriptionAsPaidCommand(id, userId),
                    ct);

                TempData["MarkAsPaidMessage"] = "Abonelik ödendi olarak işaretlendi.";
                return RedirectToAction(nameof(Active));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Active));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Archive(int id, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            await _mediator.Send(new ArchiveSubscriptionCommand(id, userId), ct);
            TempData["ArchiveMessage"] = "Abonelik başarıyla sonlandırıldı.";
            return RedirectToAction(nameof(Active));
        }

        [HttpPost]
        public async Task<IActionResult> Unarchive(int id, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            await _mediator.Send(new ArchiveSubscriptionCommand(id, userId), ct);
            TempData["ArchiveMessage"] = "Abonelik başarıyla aktifleştirildi.";
            return RedirectToAction(nameof(Archived));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            await _mediator.Send(
                new DeleteSubscriptionCommand(id, userId),
                ct);

            TempData["DeleteMessage"] = "Abonelik kalıcı olarak silindi.";
            return RedirectToAction(nameof(Archived));
        }

        private static SubscriptionsViewModel MapToVm(SubscriptionDto d) => new()
        {
            Id = d.Id,
            Name = d.Name,
            Price = d.Price,
            RenewalPeriod = d.RenewalPeriod,
            RenewalDate = d.RenewalDate,
            IsArchived = d.IsArchived
        };
    }
}
