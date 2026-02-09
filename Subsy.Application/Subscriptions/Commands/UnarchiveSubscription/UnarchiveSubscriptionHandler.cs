using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Commands.UnarchiveSubscription;

public sealed class UnarchiveSubscriptionHandler
{
    private readonly ISubscriptionRepository _repo;
    public UnarchiveSubscriptionHandler(ISubscriptionRepository repo) => _repo = repo;

    public async Task HandleAsync(UnarchiveSubscriptionCommand cmd, CancellationToken ct = default)
    {
        var sub = await _repo.GetByIdAsync(cmd.Id, ct);
        if (sub is null) throw new KeyNotFoundException("Subscription not found.");
        if (sub.UserId != cmd.UserId) throw new UnauthorizedAccessException();

        sub.IsArchived = false;
        await _repo.UpdateAsync(sub, ct);
    }
}
