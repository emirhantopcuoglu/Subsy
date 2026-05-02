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

    private static readonly Dictionary<SubscriptionCategory, string> CategoryNames = new()
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
                return Math.Round(amount / rate, 2);
            return amount;
        }

        var today = _dateTime.Today;

        // Monthly equivalents per subscription
        var subsWithCost = subs
            .Where(s => s.RenewalPeriodDays > 0)
            .Select(s => new
            {
                Sub = s,
                Monthly = ToPreferred(s.Price, s.Currency) * 30m / s.RenewalPeriodDays,
                Daily = ToPreferred(s.Price, s.Currency) / s.RenewalPeriodDays
            })
            .ToList();

        decimal totalMonthly = subsWithCost.Sum(x => x.Monthly);
        decimal totalYearly = totalMonthly * 12m;
        decimal dailyAverage = subsWithCost.Sum(x => x.Daily);

        // Grouped by service
        var grouped = subsWithCost
            .GroupBy(x => x.Sub.Name)
            .Select(g => new ServiceSummaryDto
            {
                SubscriptionName = g.Key,
                TotalCost = g.Sum(x => x.Monthly)
            })
            .OrderByDescending(x => x.TotalCost)
            .ToList();

        // Grouped by category
        var groupedByCategory = subsWithCost
            .GroupBy(x => x.Sub.Category)
            .Select(g => new CategorySummaryDto
            {
                CategoryName = CategoryNames.GetValueOrDefault(g.Key, "Diğer"),
                TotalCost = g.Sum(x => x.Monthly),
                Count = g.Count()
            })
            .OrderByDescending(x => x.TotalCost)
            .ToList();

        // Cost table
        var costTable = subsWithCost
            .OrderByDescending(x => x.Monthly)
            .Select(x => new SubscriptionCostRow
            {
                Name = x.Sub.Name,
                CategoryName = CategoryNames.GetValueOrDefault(x.Sub.Category, "Diğer"),
                Period = GetPeriodLabel(x.Sub.RenewalPeriodDays),
                OriginalPrice = x.Sub.Price,
                OriginalCurrency = x.Sub.Currency,
                MonthlyCost = Math.Round(x.Monthly, 2),
                DailyCost = Math.Round(x.Daily, 2)
            })
            .ToList();

        // Payment calendar (next 30 days, grouped by week)
        var calendar = BuildPaymentCalendar(subs, today, ToPreferred);

        // Smart insights
        var insights = BuildInsights(subsWithCost.Select(x => (x.Sub, x.Monthly)).ToList(), totalMonthly, today, groupedByCategory, ToPreferred);

        var topService = grouped.FirstOrDefault();

        return new FinanceDashboardDto
        {
            TotalMonthlyCost = totalMonthly,
            TotalYearlyCost = totalYearly,
            DailyAverage = dailyAverage,
            Currency = preferredCurrency,
            SubscriptionCount = subs.Count,
            TopSpendingService = topService,
            GroupedByService = grouped,
            GroupedByCategory = groupedByCategory,
            Insights = insights,
            PaymentCalendar = calendar,
            CostTable = costTable
        };
    }

    private static string GetPeriodLabel(int days) => days switch
    {
        <= 7 => "Haftalık",
        <= 16 => "2 Haftalık",
        <= 35 => "Aylık",
        <= 95 => "3 Aylık",
        <= 190 => "6 Aylık",
        _ => "Yıllık"
    };

    private static List<PaymentCalendarWeek> BuildPaymentCalendar(
        List<Subscription> subs, DateTime today,
        Func<decimal, string, decimal> toPreferred)
    {
        var tr = new CultureInfo("tr-TR");
        var end = today.AddDays(30);

        var payments = subs
            .Where(s => s.RenewalDate >= today && s.RenewalDate <= end)
            .OrderBy(s => s.RenewalDate)
            .Select(s => new PaymentCalendarItem
            {
                Name = s.Name,
                Date = s.RenewalDate,
                Amount = toPreferred(s.Price, s.Currency)
            })
            .ToList();

        var weeks = new List<PaymentCalendarWeek>();
        var currentWeekStart = today;

        while (currentWeekStart < end)
        {
            var weekEnd = currentWeekStart.AddDays(7);
            var weekItems = payments.Where(p => p.Date >= currentWeekStart && p.Date < weekEnd).ToList();

            if (weekItems.Count > 0)
            {
                var label = $"{currentWeekStart:d MMM} – {weekEnd.AddDays(-1):d MMM}";
                weeks.Add(new PaymentCalendarWeek
                {
                    Label = label,
                    Total = weekItems.Sum(x => x.Amount),
                    Items = weekItems
                });
            }

            currentWeekStart = weekEnd;
        }

        return weeks;
    }

    private static List<InsightDto> BuildInsights(
        List<(Subscription Sub, decimal Monthly)> subs,
        decimal totalMonthly,
        DateTime today,
        List<CategorySummaryDto> categories,
        Func<decimal, string, decimal> toPreferred)
    {
        var insights = new List<InsightDto>();
        if (subs.Count == 0) return insights;

        // Top N subscriptions dominate spending
        if (subs.Count >= 3)
        {
            var top2 = subs.OrderByDescending(x => x.Monthly).Take(2).Sum(x => x.Monthly);
            var pct = totalMonthly > 0 ? top2 / totalMonthly * 100 : 0;
            if (pct >= 50)
            {
                var names = string.Join(" ve ", subs.OrderByDescending(x => x.Monthly).Take(2).Select(x => x.Sub.Name));
                insights.Add(new InsightDto("💰", $"{names}, toplam harcamanın %{pct:F0}'ini oluşturuyor.", "warning"));
            }
        }

        // Category with multiple subscriptions
        var multiCat = categories.Where(c => c.Count >= 2).OrderByDescending(c => c.Count).FirstOrDefault();
        if (multiCat is not null)
        {
            var catSubs = subs.Where(x => CategoryNames.GetValueOrDefault(x.Sub.Category, "Diğer") == multiCat.CategoryName).Select(x => x.Sub.Name);
            insights.Add(new InsightDto("🔁", $"{multiCat.CategoryName} kategorisinde {multiCat.Count} aboneliğin var: {string.Join(", ", catSubs)}.", "info"));
        }

        // Upcoming payments in next 7 days
        var next7 = subs.Where(x => x.Sub.RenewalDate >= today && x.Sub.RenewalDate <= today.AddDays(7)).ToList();
        if (next7.Count > 0)
        {
            var total7 = next7.Sum(x => toPreferred(x.Sub.Price, x.Sub.Currency));
            insights.Add(new InsightDto("📅", $"Önümüzdeki 7 günde {next7.Count} ödeme var, toplam {total7:C0}.", "neutral"));
        }

        // Most expensive single subscription vs daily perspective
        var most = subs.OrderByDescending(x => x.Monthly).FirstOrDefault();
        if (most.Sub is not null && most.Monthly > 0)
        {
            var daily = most.Monthly / 30m;
            insights.Add(new InsightDto("📊", $"En pahalı aboneliğin ({most.Sub.Name}) günde {daily:C2} maliyetinde.", "neutral"));
        }

        return insights;
    }
}
