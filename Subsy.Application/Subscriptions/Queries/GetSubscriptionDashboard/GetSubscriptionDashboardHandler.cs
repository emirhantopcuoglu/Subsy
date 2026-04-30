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
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1);
        var upcomingEnd = today.AddDays(3);

        var activeCountTask = _repo.GetActiveCountAsync(q.UserId, today, ct);
        var todayDueCountTask = _repo.GetDueCountOnDateAsync(q.UserId, today, ct);
        var totalThisMonthTask = _repo.GetTotalInPeriodAsync(q.UserId, monthStart, monthEnd, ct);
        var upcomingTask = _repo.GetUpcomingAsync(q.UserId, today, upcomingEnd, ct);

        await Task.WhenAll(activeCountTask, todayDueCountTask, totalThisMonthTask, upcomingTask);

        return new SubscriptionDashboardDto
        {
            ActiveCount = activeCountTask.Result,
            TodayDueCount = todayDueCountTask.Result,
            TotalThisMonth = totalThisMonthTask.Result,
            Upcoming = upcomingTask.Result.Select(s => new UpcomingSubscriptionDto
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