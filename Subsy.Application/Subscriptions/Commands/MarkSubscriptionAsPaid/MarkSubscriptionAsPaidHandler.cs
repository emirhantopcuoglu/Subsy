using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Commands.MarkSubscriptionAsPaid;

public sealed class MarkSubscriptionAsPaidHandler
    : IRequestHandler<MarkSubscriptionAsPaidCommand, Unit>
{
    private readonly ISubscriptionRepository _repo;
    private readonly IDateTimeProvider _dateTime;

    public MarkSubscriptionAsPaidHandler(ISubscriptionRepository repo, IDateTimeProvider dateTime)
    {
        _repo = repo;
        _dateTime = dateTime;
    }

    public async Task<Unit> Handle(MarkSubscriptionAsPaidCommand cmd, CancellationToken ct)
    {
        var subscription = await _repo.GetByIdAsync(cmd.Id, cmd.UserId, ct);

        if (subscription is null)
            throw new KeyNotFoundException();

        subscription.MarkAsPaid(_dateTime.Today);

        await _repo.UpdateAsync(subscription, ct);
        return Unit.Value;
    }
}