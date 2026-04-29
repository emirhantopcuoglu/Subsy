using MediatR;
using Subsy.Application.Common.Events;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Commands.UnarchiveSubscription;

public sealed class UnarchiveSubscriptionHandler
    : IRequestHandler<UnarchiveSubscriptionCommand, Unit>
{
    private readonly ISubscriptionRepository _repo;
    private readonly IPublisher _publisher;

    public UnarchiveSubscriptionHandler(ISubscriptionRepository repo, IPublisher publisher)
    {
        _repo = repo;
        _publisher = publisher;
    }

    public async Task<Unit> Handle(UnarchiveSubscriptionCommand cmd, CancellationToken ct)
    {
        var subscription = await _repo.GetByIdAsync(cmd.Id, cmd.UserId, ct);
        if (subscription is null)
            throw new KeyNotFoundException();

        subscription.Unarchive();

        await _repo.UpdateAsync(subscription, ct);

        await _publisher.Publish(new SubscriptionUnarchivedEvent(cmd.UserId, subscription.Id, subscription.Name), ct);
        return Unit.Value;
    }
}
