using System.Globalization;
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
        var upcomingTask = _repo.GetUpcomingAsync(q.UserId, today, upcomingEnd, ct);
        var allActiveTask = _repo.GetActiveByUserIdAsync(q.UserId, ct);

        await Task.WhenAll(activeCountTask, todayDueCountTask, upcomingTask, allActiveTask);

        var active = allActiveTask.Result;

        decimal totalThisMonth = active
            .Where(s => s.RenewalDate >= monthStart && s.RenewalDate < monthEnd)
            .Sum(s => s.Price);

        decimal paidThisMonth = active
            .Where(s => s.RenewalPeriodDays > 0)
            .Sum(s =>
            {
                var prev = s.RenewalDate.AddDays(-s.RenewalPeriodDays);
                return prev >= monthStart && prev < today ? s.Price : 0m;
            });

        decimal remainingThisMonth = active
            .Where(s => s.RenewalDate >= today && s.RenewalDate < monthEnd)
            .Sum(s => s.Price);

        decimal yearlyCost = active
            .Where(s => s.RenewalPeriodDays > 0)
            .Sum(s => s.Price * 30m / s.RenewalPeriodDays * 12m);

        var tr = new CultureInfo("tr-TR");
        var trend = new List<MonthlySpendPoint>();
        for (int i = 5; i >= 0; i--)
        {
            var mStart = monthStart.AddMonths(-i);
            var mEnd = mStart.AddMonths(1);
            var label = mStart.ToString("MMM", tr);
            decimal amount = active.Sum(s => GetAmountInMonth(s, mStart, mEnd));
            trend.Add(new MonthlySpendPoint(label, Math.Round(amount, 2)));
        }

        return new SubscriptionDashboardDto
        {
            ActiveCount = activeCountTask.Result,
            TodayDueCount = todayDueCountTask.Result,
            TotalThisMonth = totalThisMonth,
            YearlyCost = yearlyCost,
            PaidThisMonth = paidThisMonth,
            RemainingThisMonth = remainingThisMonth,
            SixMonthTrend = trend,
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

    private static decimal GetAmountInMonth(Domain.Entities.Subscription s, DateTime monthStart, DateTime monthEnd)
    {
        if (s.RenewalPeriodDays <= 0) return 0;

        var d = s.RenewalDate;

        while (d >= monthEnd)
            d = d.AddDays(-s.RenewalPeriodDays);

        while (d < monthStart)
            d = d.AddDays(s.RenewalPeriodDays);

        return (d >= monthStart && d < monthEnd) ? s.Price : 0m;
    }
}
