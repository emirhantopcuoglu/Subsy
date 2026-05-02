using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subsy.Application.Subscriptions.Queries.GetSubscriptionDashboard;
using Subsy.Web.Models;
using System.Security.Claims;

[Authorize]
public class DashboardController : Controller
{
    private readonly IMediator _mediator;
    public DashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var dto = await _mediator.Send(new GetSubscriptionDashboardQuery(userId), ct);

        var model = new DashboardViewModel
        {
            ActiveCount = dto.ActiveCount,
            TodayDueCount = dto.TodayDueCount,
            YearlyCost = dto.YearlyCost,
            MonthlyCost = dto.MonthlyCost,
            DailyAverage = dto.DailyAverage,
            NextPayment = dto.NextPayment is null ? null : new NextPaymentViewModel
            {
                Name = dto.NextPayment.Name,
                Amount = dto.NextPayment.Amount,
                Date = dto.NextPayment.Date,
                DaysLeft = dto.NextPayment.DaysLeft
            },
            Upcoming = dto.Upcoming.Select(MapUpcomingToVm).ToList()
        };

        return View(model);
    }

    private static SubscriptionsViewModel MapUpcomingToVm(UpcomingSubscriptionDto x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Price = x.Price,
        Currency = x.Currency,
        RenewalDate = x.RenewalDate,
        RenewalPeriodDays = x.RenewalPeriodDays,
        IsArchived = x.IsArchived
    };
}
