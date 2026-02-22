using MediatR;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Queries.Common;

namespace Subsy.Application.Subscriptions.Queries.GetUserSubscriptions;

public sealed class GetUserSubscriptionsHandler : IRequestHandler<GetUserSubscriptionsQuery, List<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _repo;

    public GetUserSubscriptionsHandler(ISubscriptionRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<SubscriptionDto>> Handle(GetUserSubscriptionsQuery request, CancellationToken ct)
    {
        var subs = await _repo.GetAllByUserIdAsync(request.UserId, ct);

        return subs.Select(s => new SubscriptionDto
        {
            Id = s.Id,
            Name = s.Name,
            Price = s.Price,
            RenewalPeriodDays = s.RenewalPeriodDays,
            RenewalDate = s.RenewalDate,
            IsArchived = s.IsArchived
        }).ToList();
    }
}
