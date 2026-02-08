using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Subsy.Application.Finance.Dashboard;
using Subsy.Web.Models;

namespace Subsy.Controllers
{
    public class FinanceController : Controller
    {
        private readonly GetFinanceDashboardHandler _handler;

        public FinanceController(GetFinanceDashboardHandler handler)
        {
            _handler = handler;
        }

        public async Task<IActionResult> Dashboard(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var dto = await _handler.HandleAsync(new GetFinanceDashboardQuery(userId), ct);

            // Web ViewModel’e map (UI tarafı)
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
