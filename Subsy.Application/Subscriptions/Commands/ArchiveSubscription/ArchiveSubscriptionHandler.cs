using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Commands.ArchiveSubscription;

public sealed class ArchiveSubscriptionHandler
    : IRequestHandler<ArchiveSubscriptionCommand, Unit>
{
    private readonly ISubscriptionRepository _repo;

    public ArchiveSubscriptionHandler(ISubscriptionRepository repo)
    {
        _repo = repo;
    }

    public async Task<Unit> Handle(ArchiveSubscriptionCommand cmd, CancellationToken ct)
    {
        var subscription = await _repo.GetByIdAsync(cmd.Id, cmd.UserId, ct);
        if (subscription is null)
            throw new KeyNotFoundException();

        subscription.Archive();

        await _repo.UpdateAsync(subscription, ct);
        return Unit.Value;
    }
}
