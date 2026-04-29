using MediatR;
using Subsy.Application.Common.Events;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Commands.DeleteSubscription;

public sealed class DeleteSubscriptionHandler
    : IRequestHandler<DeleteSubscriptionCommand, Unit>
{
    private readonly ISubscriptionRepository _repo;
    private readonly IPublisher _publisher;

    public DeleteSubscriptionHandler(ISubscriptionRepository repo, IPublisher publisher)
    {
        _repo = repo;
        _publisher = publisher;
    }

    public async Task<Unit> Handle(DeleteSubscriptionCommand cmd, CancellationToken ct)
    {
        var subscription = await _repo.GetByIdAsync(cmd.Id, cmd.UserId, ct);
        if (subscription is null)
            throw new KeyNotFoundException();

        if (subscription.UserId != cmd.UserId)
            throw new UnauthorizedAccessException();

        await _repo.DeleteAsync(subscription, ct);

        await _publisher.Publish(new SubscriptionDeletedEvent(cmd.UserId, subscription.Id, subscription.Name), ct);
        return Unit.Value;
    }
}
