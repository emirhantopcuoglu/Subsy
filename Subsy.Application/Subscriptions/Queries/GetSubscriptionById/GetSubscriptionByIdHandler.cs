using MediatR;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Queries.Common;

namespace Subsy.Application.Subscriptions.Queries.GetSubscriptionById;

public sealed class GetSubscriptionByIdHandler
    : IRequestHandler<GetSubscriptionByIdQuery, SubscriptionDto?>
{
    private readonly ISubscriptionRepository _repo;

    public GetSubscriptionByIdHandler(ISubscriptionRepository repo)
    {
        _repo = repo;
    }

    public async Task<SubscriptionDto?> Handle(GetSubscriptionByIdQuery request, CancellationToken ct)
    {
        var sub = await _repo.GetByIdAsync(request.Id, request.UserId, ct);
        if (sub is null) return null;

        return new SubscriptionDto
        {
            Id = sub.Id,
            Name = sub.Name,
            Price = sub.Price,
            Currency = sub.Currency,
            RenewalPeriodDays = sub.RenewalPeriodDays,
            RenewalDate = sub.RenewalDate,
            IsArchived = sub.IsArchived,
            Category = sub.Category,
            WebsiteUrl = sub.WebsiteUrl
        };
    }
}