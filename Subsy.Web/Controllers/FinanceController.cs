using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subsy.Application.Finance.Dashboard.Queries;
using Subsy.Web.Models;
using System.Security.Claims;

namespace Subsy.Web.Controllers;

[Authorize]
public class FinanceController : Controller
{
    private readonly IMediator _mediator;

    public FinanceController(IMediator mediator) => _mediator = mediator;

    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var dto = await _mediator.Send(new GetFinanceDashboardQuery(userId), ct);

        var model = new FinanceViewModel
        {
            TotalMonthlyCost = dto.TotalMonthlyCost,
            TotalYearlyCost = dto.TotalYearlyCost,
            DailyAverage = dto.DailyAverage,
            Currency = dto.Currency,
            SubscriptionCount = dto.SubscriptionCount,
            TopSpendingService = dto.TopSpendingService,
            GroupedByCategory = dto.GroupedByCategory,
            Insights = dto.Insights,
            PaymentCalendar = dto.PaymentCalendar,
            CostTable = dto.CostTable
        };

        return View(model);
    }
}
