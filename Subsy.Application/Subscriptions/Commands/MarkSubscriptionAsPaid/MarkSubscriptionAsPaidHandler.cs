using MediatR;
using Subsy.Application.Common.Events;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Commands.MarkSubscriptionAsPaid;

public sealed class MarkSubscriptionAsPaidHandler
    : IRequestHandler<MarkSubscriptionAsPaidCommand, Unit>
{
    private readonly ISubscriptionRepository _repo;
    private readonly IDateTimeProvider _dateTime;
    private readonly IPublisher _publisher;

    public MarkSubscriptionAsPaidHandler(
        ISubscriptionRepository repo, IDateTimeProvider dateTime, IPublisher publisher)
    {
        _repo = repo;
        _dateTime = dateTime;
        _publisher = publisher;
    }

    public async Task<Unit> Handle(MarkSubscriptionAsPaidCommand cmd, CancellationToken ct)
    {
        var subscription = await _repo.GetByIdAsync(cmd.Id, cmd.UserId, ct);
        if (subscription is null) throw new KeyNotFoundException();

        var oldDate = subscription.RenewalDate;
        subscription.MarkAsPaid(_dateTime.Today);
        await _repo.UpdateAsync(subscription, ct);

        await _publisher.Publish(new SubscriptionPaidEvent(
            cmd.UserId, subscription.Id, subscription.Name,
            oldDate, subscription.RenewalDate), ct);

        return Unit.Value;
    }
}