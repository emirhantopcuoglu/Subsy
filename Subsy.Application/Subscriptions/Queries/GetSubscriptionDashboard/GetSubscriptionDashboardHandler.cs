using System.Globalization;
using MediatR;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Queries.GetSubscriptionDashboard;
using Subsy.Domain.Entities;

namespace Subsy.Application.Subscriptions.Queries.GetDashboard;

public sealed class GetDashboardHandler : IRequestHandler<GetSubscriptionDashboardQuery, SubscriptionDashboardDto>
{
    private readonly ISubscriptionRepository _repo;
    private readonly IUserProfileService _userProfileService;
    private readonly IExchangeRateService _exchangeRateService;

    public GetDashboardHandler(
        ISubscriptionRepository repo,
        IUserProfileService userProfileService,
        IExchangeRateService exchangeRateService)
    {
        _repo = repo;
        _userProfileService = userProfileService;
        _exchangeRateService = exchangeRateService;
    }

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
        var profileTask = _userProfileService.GetByUserIdAsync(q.UserId, ct);

        await Task.WhenAll(activeCountTask, todayDueCountTask, upcomingTask, allActiveTask, profileTask);

        var active = allActiveTask.Result;
        var preferredCurrency = profileTask.Result?.PreferredCurrency ?? "TRY";
        var rates = await _exchangeRateService.GetRatesAsync(preferredCurrency, ct);

        decimal ToPreferred(decimal amount, string currency)
        {
            if (currency.Equals(preferredCurrency, StringComparison.OrdinalIgnoreCase))
                return amount;
            if (rates.TryGetValue(currency.ToUpperInvariant(), out var rate) && rate > 0)
                return Math.Round(amount / rate, 2);
            return amount;
        }

        decimal totalThisMonth = active
            .Where(s => s.RenewalDate >= monthStart && s.RenewalDate < monthEnd)
            .Sum(s => ToPreferred(s.Price, s.Currency));

        decimal paidThisMonth = active
            .Where(s => s.RenewalPeriodDays > 0)
            .Sum(s =>
            {
                var prev = s.RenewalDate.AddDays(-s.RenewalPeriodDays);
                return prev >= monthStart && prev < today ? ToPreferred(s.Price, s.Currency) : 0m;
            });

        decimal remainingThisMonth = active
            .Where(s => s.RenewalDate >= today && s.RenewalDate < monthEnd)
            .Sum(s => ToPreferred(s.Price, s.Currency));

        decimal yearlyCost = active
            .Where(s => s.RenewalPeriodDays > 0)
            .Sum(s => ToPreferred(s.Price, s.Currency) * 30m / s.RenewalPeriodDays * 12m);

        var tr = new CultureInfo("tr-TR");
        var trend = new List<MonthlySpendPoint>();
        for (int i = 5; i >= 0; i--)
        {
            var mStart = monthStart.AddMonths(-i);
            var mEnd = mStart.AddMonths(1);
            var label = mStart.ToString("MMM", tr);
            decimal amount = active.Sum(s => GetAmountInMonth(s, mStart, mEnd, ToPreferred));
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

    private static decimal GetAmountInMonth(
        Subscription s, DateTime monthStart, DateTime monthEnd,
        Func<decimal, string, decimal> toPreferred)
    {
        if (s.RenewalPeriodDays <= 0) return 0;
        if (s.CreatedAt >= monthEnd) return 0;

        var d = s.RenewalDate;

        while (d >= monthEnd)
            d = d.AddDays(-s.RenewalPeriodDays);

        while (d < monthStart)
            d = d.AddDays(s.RenewalPeriodDays);

        if (d >= monthStart && d < monthEnd && s.CreatedAt <= d)
            return toPreferred(s.Price, s.Currency);

        return 0m;
    }
}
