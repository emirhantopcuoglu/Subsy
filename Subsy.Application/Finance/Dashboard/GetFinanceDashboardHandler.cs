using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Finance.Dashboard;

public sealed class GetFinanceDashboardHandler
{
    private readonly ISubscriptionRepository _repo;

    public GetFinanceDashboardHandler(ISubscriptionRepository repo)
    {
        _repo = repo;
    }

    public async Task<FinanceDashboardDto> HandleAsync(GetFinanceDashboardQuery query, CancellationToken ct = default)
    {
        var subs = await _repo.GetAllByUserIdAsync(query.UserId, ct);

        var now = DateTime.Now;

        // Aktif ve ödemesi yapılmış (senin mevcut kuralın)
        var activePaidSubs = subs
            .Where(s => !s.IsArchived && s.RenewalDate > now)
            .ToList();

        var thisMonthSpending = activePaidSubs
            .Where(s => s.RenewalDate.Month == now.Month && s.RenewalDate.Year == now.Year)
            .Sum(s => s.Price);

        var totalSpending = activePaidSubs.Sum(s => s.Price);

        var grouped = activePaidSubs
            .GroupBy(a => a.Name)
            .Select(g => new ServiceSummaryDto
            {
                SubscriptionName = g.Key,
                TotalCost = g.Sum(x => x.Price)
            })
            .ToList();

        var topService = grouped
            .OrderByDescending(g => g.TotalCost)
            .FirstOrDefault();

        return new FinanceDashboardDto
        {
            TotalMonthlyCost = thisMonthSpending,
            AllTimeSpending = totalSpending,
            GroupedByService = grouped,
            TopSpendingService = topService,
            SubscriptionCount = activePaidSubs.Count
        };
    }
}
