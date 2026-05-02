using MediatR;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Queries.Common;
using Subsy.Application.Subscriptions.Queries.GetActiveSubscriptions;

public sealed class GetActiveSubscriptionsHandler
    : IRequestHandler<GetActiveSubscriptionsQuery, List<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _repo;

    public GetActiveSubscriptionsHandler(ISubscriptionRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<SubscriptionDto>> Handle(
        GetActiveSubscriptionsQuery request,
        CancellationToken ct)
    {
        var subs = await _repo.GetActiveByUserIdAsync(request.UserId, ct);

        return subs
            .Select(s => new SubscriptionDto
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                Currency = s.Currency,
                RenewalPeriodDays = s.RenewalPeriodDays,
                RenewalDate = s.RenewalDate,
                IsArchived = s.IsArchived,
                Category = s.Category,
                WebsiteUrl = s.WebsiteUrl
            })
            .ToList();
    }
}
