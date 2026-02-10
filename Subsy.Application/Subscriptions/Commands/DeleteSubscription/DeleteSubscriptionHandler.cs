using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Commands.DeleteSubscription;

public sealed class DeleteSubscriptionHandler
    : IRequestHandler<DeleteSubscriptionCommand, Unit>
{
    private readonly ISubscriptionRepository _repo;

    public DeleteSubscriptionHandler(ISubscriptionRepository repo)
    {
        _repo = repo;
    }

    public async Task<Unit> Handle(DeleteSubscriptionCommand cmd, CancellationToken ct)
    {
        var subscription = await _repo.GetByIdAsync(cmd.Id, ct);
        if (subscription is null)
            throw new KeyNotFoundException();

        if (subscription.UserId != cmd.UserId)
            throw new UnauthorizedAccessException();

        await _repo.DeleteAsync(subscription, ct);
        return Unit.Value;
    }
}
