using MediatR;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Queries.Common;

namespace Subsy.Application.Subscriptions.Queries.GetDueSubscriptions;

public sealed class GetDueSubscriptionsHandler : IRequestHandler<GetDueSubscriptionsQuery, List<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _repo;
    public GetDueSubscriptionsHandler(ISubscriptionRepository repo) => _repo = repo;

    public async Task<List<SubscriptionDto>> Handle(GetDueSubscriptionsQuery q, CancellationToken ct = default)
    {
        var subs = await _repo.GetAllByUserIdAsync(q.UserId, ct);
        return subs
            .Where(s => !s.IsArchived && ((s.RenewalDate.Date == DateTime.Today.Date) || s.RenewalDate.Date <= DateTime.Today.Date))
            .Select(s => new SubscriptionDto
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                RenewalPeriodDays = s.RenewalPeriodDays,
                RenewalDate = s.RenewalDate,
                IsArchived = s.IsArchived
            })
            .ToList();
    }
}
