using MediatR;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Queries.Common;

namespace Subsy.Application.Subscriptions.Queries.GetDueSubscriptions;

public sealed class GetDueSubscriptionsHandler : IRequestHandler<GetDueSubscriptionsQuery, List<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _repo;
    private readonly IDateTimeProvider _dateTime;
    public GetDueSubscriptionsHandler(ISubscriptionRepository repo, IDateTimeProvider dateTime)
    {
        _repo = repo;
        _dateTime = dateTime;
    }

    public async Task<List<SubscriptionDto>> Handle(GetDueSubscriptionsQuery q, CancellationToken ct = default)
    {
        var subs = await _repo.GetAllByUserIdAsync(q.UserId, ct);
        return subs
            .Where(s => !s.IsArchived && s.RenewalDate.Date <= _dateTime.Today.Date)
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
