using MediatR;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Queries.GetSubscriptionDashboard;

namespace Subsy.Application.Subscriptions.Queries.GetDashboard;

public sealed class GetDashboardHandler : IRequestHandler<GetSubscriptionDashboardQuery, SubscriptionDashboardDto>
{
    private readonly ISubscriptionRepository _repo;
    public GetDashboardHandler(ISubscriptionRepository repo) => _repo = repo;

    public async Task<SubscriptionDashboardDto> Handle(GetSubscriptionDashboardQuery q, CancellationToken ct)
    {
        var subs = await _repo.GetAllByUserIdAsync(q.UserId, ct);

        var active = subs.Where(s => s.IsArchived == false).ToList();

        var today = DateTime.Today;
        var todayDue = active.Where(s => s.RenewalDate.Date == today).ToList();

        var activeCount = active.Count(s => s.RenewalDate.Date >= today);

        var monthStart = new DateTime(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var totalThisMonth = active
            .Where(s => s.RenewalDate >= monthStart && s.RenewalDate < monthEnd)
            .Sum(s => s.Price);

        var upcomingEnd = today.AddDays(3);
        var upcoming = active
            .Where(s => s.RenewalDate.Date >= today && s.RenewalDate.Date <= upcomingEnd)
            .OrderBy(s => s.RenewalDate)
            .ToList();

        return new SubscriptionDashboardDto
        {
            ActiveCount = activeCount,
            TodayDueCount = todayDue.Count,
            TotalThisMonth = totalThisMonth,
            Upcoming = upcoming.Select(s => new UpcomingSubscriptionDto
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                RenewalDate = s.RenewalDate,
                RenewalPeriodDays = s.RenewalPeriodDays,
                IsArchived = s.IsArchived
            }).ToList()
        };
    }
}