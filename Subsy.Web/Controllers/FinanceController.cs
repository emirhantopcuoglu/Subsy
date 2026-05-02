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
            AllTimeSpending = dto.AllTimeSpending,
            Currency = dto.Currency,
            SubscriptionCount = dto.SubscriptionCount,
            YearlyPlanTotal = dto.YearlyPlanTotal,
            MonthlyPlanTotal = dto.MonthlyPlanTotal,
            TopSpendingService = dto.TopSpendingService is null
                ? null
                : new ServiceSummary
                {
                    SubscriptionName = dto.TopSpendingService.SubscriptionName,
                    TotalCost = dto.TopSpendingService.TotalCost
                },
            GroupedByService = dto.GroupedByService
                .Select(x => new ServiceSummary
                {
                    SubscriptionName = x.SubscriptionName,
                    TotalCost = x.TotalCost
                })
                .ToList(),
            GroupedByCategory = dto.GroupedByCategory
                .Select(x => new CategorySummary
                {
                    CategoryName = x.CategoryName,
                    TotalCost = x.TotalCost,
                    Count = x.Count
                })
                .ToList(),
            TwelveMonthTrend = dto.TwelveMonthTrend
                .Select(p => (p.Label, p.Amount))
                .ToList()
        };

        return View(model);
    }
}
