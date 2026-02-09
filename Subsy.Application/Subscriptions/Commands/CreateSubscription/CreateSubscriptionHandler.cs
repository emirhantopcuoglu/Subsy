using Subsy.Application.Common.Interfaces;
using Subsy.Domain.Entities;

namespace Subsy.Application.Subscriptions.Commands.CreateSubscription;

public sealed class CreateSubscriptionHandler
{
    private readonly ISubscriptionRepository _repo;

    public CreateSubscriptionHandler(ISubscriptionRepository repo)
    {
        _repo = repo;
    }

    public async Task HandleAsync(CreateSubscriptionCommand cmd, CancellationToken ct = default)
    {
        if (cmd.RenewalDate.Date < DateTime.Today)
            throw new ArgumentException("Yenileme tarihi geçmiş olamaz.");

        var subscription = new Subscription
        {
            UserId = cmd.UserId,
            Name = cmd.Name,
            Price = cmd.Price,
            RenewalPeriod = cmd.RenewalPeriod,
            RenewalDate = cmd.RenewalDate,
            IsArchived = false
        };

        await _repo.AddAsync(subscription, ct);
    }
}
