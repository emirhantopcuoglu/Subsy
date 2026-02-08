using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Dashboard;

public sealed class SubscriptionDashboardService : ISubscriptionDashboardService
{
    private readonly ISubscriptionRepository _repository;

    public SubscriptionDashboardService(ISubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<SubscriptionDashboardDto> GetDashboardAsync(string userId, CancellationToken cancellationToken = default)
    {
        var subs = await _repository.GetAllByUserIdAsync(userId, cancellationToken);

        var today = DateTime.Today;

        var activeCount = subs.Count(s => s.RenewalDate >= today && !s.IsArchived);
        var todayDueCount = subs.Count(s => s.RenewalDate == today && !s.IsArchived);

        var totalThisMonth = subs
            .Where(s => s.RenewalDate.Month == today.Month && s.RenewalDate.Year == today.Year && !s.IsArchived)
            .Sum(s => s.Price);

        var upcoming = subs
            .Where(s => s.RenewalDate <= today.AddDays(3) && s.RenewalDate >= today && !s.IsArchived)
            .ToList();

        return new SubscriptionDashboardDto
        {
            ActiveCount = activeCount,
            TodayDueCount = todayDueCount,
            TotalThisMonth = totalThisMonth,
            Upcoming = upcoming
        };
    }
}
