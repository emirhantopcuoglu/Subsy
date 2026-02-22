using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Finance.Dashboard.Queries;

public sealed class GetFinanceDashboardHandler : IRequestHandler<GetFinanceDashboardQuery, FinanceDashboardDto>
{
    private readonly ISubscriptionRepository _repo;

    public GetFinanceDashboardHandler(ISubscriptionRepository repo)
    {
        _repo = repo;
    }

    public async Task<FinanceDashboardDto> Handle(GetFinanceDashboardQuery query, CancellationToken ct = default)
    {
        var subs = await _repo.GetAllByUserIdAsync(query.UserId, ct);

        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var active = subs.Where(s => s.IsArchived == false).ToList();

        decimal monthlyEquivalent = active
            .Where(s => s.RenewalPeriodDays > 0)
            .Sum(s => s.Price * 30m / s.RenewalPeriodDays);

        decimal dueThisMonth = active
            .Where(s => s.RenewalDate >= monthStart && s.RenewalDate < monthEnd)
            .Sum(s => s.Price);

        decimal totalSnapshot = active.Sum(s => s.Price);

        var grouped = active
            .Where(s => s.RenewalPeriodDays > 0)
            .GroupBy(a => a.Name)
            .Select(g => new ServiceSummaryDto
            {
                SubscriptionName = g.Key,
                TotalCost = g.Sum(x => x.Price * 30m / x.RenewalPeriodDays)
            })
            .OrderByDescending(x => x.TotalCost)
            .ToList();

        var topService = grouped.FirstOrDefault();

        return new FinanceDashboardDto
        {
            TotalMonthlyCost = monthlyEquivalent,
            AllTimeSpending = totalSnapshot,

            GroupedByService = grouped,
            TopSpendingService = topService,
            SubscriptionCount = active.Count
        };
    }
}
