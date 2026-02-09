using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Commands.ArchiveSubscription;

public sealed class ArchiveSubscriptionHandler
{
    private readonly ISubscriptionRepository _repo;
    public ArchiveSubscriptionHandler(ISubscriptionRepository repo) => _repo = repo;

    public async Task HandleAsync(ArchiveSubscriptionCommand cmd, CancellationToken ct = default)
    {
        var sub = await _repo.GetByIdAsync(cmd.Id, ct);
        if (sub is null) throw new KeyNotFoundException("Subscription not found.");
        if (sub.UserId != cmd.UserId) throw new UnauthorizedAccessException();

        sub.IsArchived = true;
        await _repo.UpdateAsync(sub, ct);
    }
}
