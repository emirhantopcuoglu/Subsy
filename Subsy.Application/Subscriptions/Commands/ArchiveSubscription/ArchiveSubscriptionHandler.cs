using MediatR;
using Subsy.Application.Common.Events;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Commands.ArchiveSubscription;

public sealed class ArchiveSubscriptionHandler
    : IRequestHandler<ArchiveSubscriptionCommand, Unit>
{
    private readonly ISubscriptionRepository _repo;
    private readonly IPublisher _publisher;

    public ArchiveSubscriptionHandler(ISubscriptionRepository repo, IPublisher publisher)
    {
        _repo = repo;
        _publisher = publisher;
    }

    public async Task<Unit> Handle(ArchiveSubscriptionCommand cmd, CancellationToken ct)
    {
        var subscription = await _repo.GetByIdAsync(cmd.Id, cmd.UserId, ct);
        if (subscription is null)
            throw new KeyNotFoundException();

        subscription.Archive();

        await _repo.UpdateAsync(subscription, ct);

        await _publisher.Publish(new SubscriptionArchivedEvent(
            cmd.UserId, subscription.Id, subscription.Name), ct);
        return Unit.Value;
    }
}
