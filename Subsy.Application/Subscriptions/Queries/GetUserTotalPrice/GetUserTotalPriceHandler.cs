using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Queries.GetUserTotalPrice;

public sealed class GetUserTotalPriceHandler
{
    private readonly ISubscriptionRepository _repo;
    public GetUserTotalPriceHandler(ISubscriptionRepository repo) => _repo = repo;

    public Task<decimal> HandleAsync(GetUserTotalPriceQuery query, CancellationToken ct = default)
        => _repo.GetTotalPriceByUserIdAsync(query.UserId, ct);
}
