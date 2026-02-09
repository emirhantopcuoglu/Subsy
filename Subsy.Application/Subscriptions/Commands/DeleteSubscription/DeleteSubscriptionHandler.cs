using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Commands.DeleteSubscription;

public sealed class DeleteSubscriptionHandler
{
    private readonly ISubscriptionRepository _repo;

    public DeleteSubscriptionHandler(ISubscriptionRepository repo)
    {
        _repo = repo;
    }

    public async Task HandleAsync(DeleteSubscriptionCommand cmd, CancellationToken ct = default)
    {
        var subscription = await _repo.GetByIdAsync(cmd.Id, ct);
        if (subscription is null)
            throw new KeyNotFoundException("Subscription not found.");

        if (subscription.UserId != cmd.UserId)
            throw new UnauthorizedAccessException();

        await _repo.DeleteAsync(subscription, ct);
    }
}
