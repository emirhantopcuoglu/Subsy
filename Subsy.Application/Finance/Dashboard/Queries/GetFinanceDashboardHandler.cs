using System.Globalization;
using MediatR;
using Subsy.Application.Common.Interfaces;
using Subsy.Domain.Entities;
using Subsy.Domain.Enums;

namespace Subsy.Application.Finance.Dashboard.Queries;

public sealed class GetFinanceDashboardHandler : IRequestHandler<GetFinanceDashboardQuery, FinanceDashboardDto>
{
    private readonly ISubscriptionRepository _repo;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUserProfileService _userProfileService;
    private readonly IExchangeRateService _exchangeRateService;

    public GetFinanceDashboardHandler(
        ISubscriptionRepository repo,
        IDateTimeProvider dateTime,
        IUserProfileService userProfileService,
        IExchangeRateService exchangeRateService)
    {
        _repo = repo;
        _dateTime = dateTime;
        _userProfileService = userProfileService;
        _exchangeRateService = exchangeRateService;
    }

    public async Task<FinanceDashboardDto> Handle(GetFinanceDashboardQuery query, CancellationToken ct = default)
    {
        var subsTask = _repo.GetActiveByUserIdAsync(query.UserId, ct);
        var profileTask = _userProfileService.GetByUserIdAsync(query.UserId, ct);

        await Task.WhenAll(subsTask, profileTask);

        var subs = subsTask.Result;
        var preferredCurrency = profileTask.Result?.PreferredCurrency ?? "TRY";

        var rates = await _exchangeRateService.GetRatesAsync(preferredCurrency, ct);

        decimal ToPreferred(decimal amount, string currency)
        {
            if (currency.Equals(preferredCurrency, StringComparison.OrdinalIgnoreCase))
                return amount;

            if (rates.TryGetValue(currency.ToUpperInvariant(), out var rate) && rate > 0)
                return Math.Round(amount / rate, 4);

            return amount;
        }

        var today = _dateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        // Monthly equivalent (normalized to 30 days)
        decimal monthlyEquivalent = subs
            .Where(s => s.RenewalPeriodDays > 0)
            .Sum(s => ToPreferred(s.Price, s.Currency) * 30m / s.RenewalPeriodDays);

        decimal totalSnapshot = subs.Sum(s => ToPreferred(s.Price, s.Currency));

        // Grouped by service name
        var grouped = subs
            .Where(s => s.RenewalPeriodDays > 0)
            .GroupBy(s => s.Name)
            .Select(g => new ServiceSummaryDto
            {
                SubscriptionName = g.Key,
                TotalCost = g.Sum(x => ToPreferred(x.Price, x.Currency) * 30m / x.RenewalPeriodDays)
            })
            .OrderByDescending(x => x.TotalCost)
            .ToList();

        // Grouped by category
        var categoryNames = new Dictionary<SubscriptionCategory, string>
        {
            [SubscriptionCategory.Entertainment] = "Eğlence",
            [SubscriptionCategory.Music] = "Müzik",
            [SubscriptionCategory.Software] = "Yazılım",
            [SubscriptionCategory.Gaming] = "Oyun",
            [SubscriptionCategory.Cloud] = "Bulut",
            [SubscriptionCategory.Education] = "Eğitim",
            [SubscriptionCategory.News] = "Haber",
            [SubscriptionCategory.Health] = "Sağlık",
            [SubscriptionCategory.Shopping] = "Alışveriş",
            [SubscriptionCategory.Other] = "Diğer"
        };

        var groupedByCategory = subs
            .Where(s => s.RenewalPeriodDays > 0)
            .GroupBy(s => s.Category)
            .Select(g => new CategorySummaryDto
            {
                CategoryName = categoryNames.GetValueOrDefault(g.Key, "Diğer"),
                TotalCost = g.Sum(x => ToPreferred(x.Price, x.Currency) * 30m / x.RenewalPeriodDays),
                Count = g.Count()
            })
            .OrderByDescending(x => x.TotalCost)
            .ToList();

        // 12-month trend
        var tr = new CultureInfo("tr-TR");
        var trend = new List<MonthlyTrendPoint>();
        for (int i = 11; i >= 0; i--)
        {
            var mStart = monthStart.AddMonths(-i);
            var mEnd = mStart.AddMonths(1);
            var label = mStart.ToString("MMM yy", tr);
            decimal amount = subs.Sum(s => GetAmountInMonth(s, mStart, mEnd, ToPreferred));
            trend.Add(new MonthlyTrendPoint(label, Math.Round(amount, 2)));
        }

        // Yearly vs monthly plan totals
        decimal yearlyPlanTotal = subs
            .Where(s => s.RenewalPeriodDays >= 300)
            .Sum(s => ToPreferred(s.Price, s.Currency) * 30m / s.RenewalPeriodDays);

        decimal monthlyPlanTotal = subs
            .Where(s => s.RenewalPeriodDays > 0 && s.RenewalPeriodDays < 300)
            .Sum(s => ToPreferred(s.Price, s.Currency) * 30m / s.RenewalPeriodDays);

        var topService = grouped.FirstOrDefault();

        return new FinanceDashboardDto
        {
            TotalMonthlyCost = monthlyEquivalent,
            AllTimeSpending = totalSnapshot,
            Currency = preferredCurrency,
            GroupedByService = grouped,
            GroupedByCategory = groupedByCategory,
            TwelveMonthTrend = trend,
            TopSpendingService = topService,
            SubscriptionCount = subs.Count,
            YearlyPlanTotal = yearlyPlanTotal,
            MonthlyPlanTotal = monthlyPlanTotal
        };
    }

    private static decimal GetAmountInMonth(
        Subscription s, DateTime monthStart, DateTime monthEnd,
        Func<decimal, string, decimal> toPreferred)
    {
        if (s.RenewalPeriodDays <= 0) return 0;

        var d = s.RenewalDate;

        while (d >= monthEnd)
            d = d.AddDays(-s.RenewalPeriodDays);

        while (d < monthStart)
            d = d.AddDays(s.RenewalPeriodDays);

        return (d >= monthStart && d < monthEnd) ? toPreferred(s.Price, s.Currency) : 0m;
    }
}
