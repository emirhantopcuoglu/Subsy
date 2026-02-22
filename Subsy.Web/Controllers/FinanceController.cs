using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subsy.Application.Finance.Dashboard.Queries;
using Subsy.Web.Models;
using System.Security.Claims;

namespace Subsy.Web.Controllers
{
    public class FinanceController : Controller
    {
        private readonly IMediator _mediator;

        public FinanceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Dashboard(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var dto = await _mediator.Send(new GetFinanceDashboardQuery(userId), ct);

            var model = new FinanceViewModel
            {
                TotalMonthlyCost = dto.TotalMonthlyCost,
                AllTimeSpending = dto.AllTimeSpending,
                SubscriptionCount = dto.SubscriptionCount,
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
                    .ToList()
            };

            return View(model);
        }
    }
}
