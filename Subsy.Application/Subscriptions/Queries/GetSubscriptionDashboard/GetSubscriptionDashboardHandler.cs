using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Queries.GetSubscriptionDashboard;

public sealed class GetSubscriptionDashboardHandler
{
    private readonly ISubscriptionRepository _repo;

    public GetSubscriptionDashboardHandler(ISubscriptionRepository repo)
    {
        _repo = repo;
    }

    public async Task<SubscriptionDashboardDto> HandleAsync(
        GetSubscriptionDashboardQuery query,
        CancellationToken ct = default)
    {
        var subs = await _repo.GetAllByUserIdAsync(query.UserId, ct);

        var now = DateTime.Now;

        var totalCount = subs.Count;
        var archivedCount = subs.Count(s => s.IsArchived);
        var activeSubs = subs.Where(s => !s.IsArchived).ToList();
        var activeCount = activeSubs.Count;

        // Basit toplam (aktif)
        var totalActivePrice = activeSubs.Sum(s => s.Price);

        // Yaklaşan yenilemeler (aktif + tarihi gelecekte), en yakın 5
        var upcoming = activeSubs
            .Where(s => s.RenewalDate > now)
            .OrderBy(s => s.RenewalDate)
            .Take(5)
            .Select(s => new UpcomingRenewalDto
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                RenewalDate = s.RenewalDate
            })
            .ToList();

        return new SubscriptionDashboardDto
        {
            TotalCount = totalCount,
            ActiveCount = activeCount,
            ArchivedCount = archivedCount,
            TotalActivePrice = totalActivePrice,
            UpcomingRenewals = upcoming
        };
    }
}
