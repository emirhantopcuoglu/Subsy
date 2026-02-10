using MediatR;
using Subsy.Application.Common.Interfaces;
using Subsy.Domain.Entities;

namespace Subsy.Application.Subscriptions.Commands.CreateSubscription;

public sealed class CreateSubscriptionHandler
    : IRequestHandler<CreateSubscriptionCommand, Unit>
{
    private readonly ISubscriptionRepository _repo;

    public CreateSubscriptionHandler(ISubscriptionRepository repo)
    {
        _repo = repo;
    }

    public async Task<Unit> Handle(CreateSubscriptionCommand cmd, CancellationToken ct)
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
        return Unit.Value;
    }
}
