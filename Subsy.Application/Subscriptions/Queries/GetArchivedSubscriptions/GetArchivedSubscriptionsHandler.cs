using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Queries.Common;

namespace Subsy.Application.Subscriptions.Queries.GetArchivedSubscriptions;

public sealed class GetArchivedSubscriptionsHandler
{
    private readonly ISubscriptionRepository _repo;
    public GetArchivedSubscriptionsHandler(ISubscriptionRepository repo) => _repo = repo;

    public async Task<List<SubscriptionDto>> HandleAsync(GetArchivedSubscriptionsQuery q, CancellationToken ct = default)
    {
        var subs = await _repo.GetAllByUserIdAsync(q.UserId, ct);
        return subs
            .Where(s => s.IsArchived)
            .Select(s => new SubscriptionDto
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                RenewalPeriod = s.RenewalPeriod,
                RenewalDate = s.RenewalDate,
                IsArchived = s.IsArchived
            })
            .ToList();
    }
}
