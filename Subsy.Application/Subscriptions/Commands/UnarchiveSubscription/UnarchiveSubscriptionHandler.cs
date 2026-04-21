using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Commands.UnarchiveSubscription;

public sealed class UnarchiveSubscriptionHandler
    : IRequestHandler<UnarchiveSubscriptionCommand, Unit>
{
    private readonly ISubscriptionRepository _repo;

    public UnarchiveSubscriptionHandler(ISubscriptionRepository repo)
    {
        _repo = repo;
    }

    public async Task<Unit> Handle(UnarchiveSubscriptionCommand cmd, CancellationToken ct)
    {
        var subscription = await _repo.GetByIdAsync(cmd.Id, cmd.UserId, ct);
        if (subscription is null)
            throw new KeyNotFoundException();

        subscription.Unarchive();

        await _repo.UpdateAsync(subscription, ct);
        return Unit.Value;
    }
}
