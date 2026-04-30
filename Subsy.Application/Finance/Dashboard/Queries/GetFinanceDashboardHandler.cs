using MediatR;
using Subsy.Application.Common.Interfaces;

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

        // Fetch all rates once — base = preferredCurrency, so rates[X] = how many X per 1 preferred unit
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
        var monthEnd = monthStart.AddMonths(1);

        var active = subs;

        decimal monthlyEquivalent = active
            .Where(s => s.RenewalPeriodDays > 0)
            .Sum(s => ToPreferred(s.Price, s.Currency) * 30m / s.RenewalPeriodDays);

        decimal dueThisMonth = active
            .Where(s => s.RenewalDate >= monthStart && s.RenewalDate < monthEnd)
            .Sum(s => ToPreferred(s.Price, s.Currency));

        decimal totalSnapshot = active.Sum(s => ToPreferred(s.Price, s.Currency));

        var grouped = active
            .Where(s => s.RenewalPeriodDays > 0)
            .GroupBy(s => s.Name)
            .Select(g => new ServiceSummaryDto
            {
                SubscriptionName = g.Key,
                TotalCost = g.Sum(x => ToPreferred(x.Price, x.Currency) * 30m / x.RenewalPeriodDays)
            })
            .OrderByDescending(x => x.TotalCost)
            .ToList();

        var topService = grouped.FirstOrDefault();

        return new FinanceDashboardDto
        {
            TotalMonthlyCost = monthlyEquivalent,
            AllTimeSpending = totalSnapshot,
            Currency = preferredCurrency,
            GroupedByService = grouped,
            TopSpendingService = topService,
            SubscriptionCount = active.Count
        };
    }
}
