using MediatR;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Queries.GetSubscriptionDashboard;

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

        decimal monthlyCost = active
            .Where(s => s.RenewalPeriodDays > 0)
            .Sum(s => ToPreferred(s.Price, s.Currency) * 30m / s.RenewalPeriodDays);

        decimal dailyAverage = active
            .Where(s => s.RenewalPeriodDays > 0)
            .Sum(s => ToPreferred(s.Price, s.Currency) / s.RenewalPeriodDays);

        decimal yearlyCost = monthlyCost * 12m;

        // Next upcoming payment (closest future renewal)
        var nextSub = active
            .Where(s => s.RenewalDate >= today)
            .OrderBy(s => s.RenewalDate)
            .FirstOrDefault();

        NextPaymentDto? nextPayment = nextSub is null ? null : new NextPaymentDto
        {
            Name = nextSub.Name,
            Amount = ToPreferred(nextSub.Price, nextSub.Currency),
            Date = nextSub.RenewalDate,
            DaysLeft = (nextSub.RenewalDate - today).Days
        };

        return new SubscriptionDashboardDto
        {
            ActiveCount = activeCountTask.Result,
            TodayDueCount = todayDueCountTask.Result,
            YearlyCost = yearlyCost,
            MonthlyCost = monthlyCost,
            DailyAverage = dailyAverage,
            NextPayment = nextPayment,
            Upcoming = upcomingTask.Result.Select(s => new UpcomingSubscriptionDto
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                Currency = s.Currency,
                RenewalDate = s.RenewalDate,
                RenewalPeriodDays = s.RenewalPeriodDays,
                IsArchived = s.IsArchived
            }).ToList()
        };
    }
}
