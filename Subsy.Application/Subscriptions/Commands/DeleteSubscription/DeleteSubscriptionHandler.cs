using MediatR;
using Microsoft.Extensions.Logging;
using Subsy.Application.Common.Events;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Commands.DeleteSubscription;

public sealed class DeleteSubscriptionHandler
    : IRequestHandler<DeleteSubscriptionCommand, Unit>
{
    private readonly ISubscriptionRepository _repo;
    private readonly IPublisher _publisher;
    private readonly ILogger<DeleteSubscriptionHandler> _logger;

    public DeleteSubscriptionHandler(
        ISubscriptionRepository repo,
        IPublisher publisher,
        ILogger<DeleteSubscriptionHandler> logger)
    {
        _repo = repo;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteSubscriptionCommand cmd, CancellationToken ct)
    {
        var subscription = await _repo.GetByIdAsync(cmd.Id, cmd.UserId, ct);
        if (subscription is null)
            throw new KeyNotFoundException();

        if (subscription.UserId != cmd.UserId)
            throw new UnauthorizedAccessException();

        await _repo.DeleteAsync(subscription, ct);

        _logger.LogInformation("Subscription deleted: {SubscriptionId} '{Name}' by user {UserId}",
            subscription.Id, subscription.Name, cmd.UserId);

        await _publisher.Publish(new SubscriptionDeletedEvent(cmd.UserId, subscription.Id, subscription.Name), ct);
        return Unit.Value;
    }
}
