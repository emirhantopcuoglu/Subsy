using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Commands.UpdateSubscription;

public sealed class UpdateSubscriptionHandler
    : IRequestHandler<UpdateSubscriptionCommand, Unit>
{
    private readonly ISubscriptionRepository _repo;

    public UpdateSubscriptionHandler(ISubscriptionRepository repo)
    {
        _repo = repo;
    }

    public async Task<Unit> Handle(UpdateSubscriptionCommand cmd, CancellationToken ct)
    {
        var subscription = await _repo.GetByIdAsync(cmd.Id, ct);
        if (subscription is null)
            throw new KeyNotFoundException("Subscription not found.");

        if (subscription.UserId != cmd.UserId)
            throw new UnauthorizedAccessException();

        if (cmd.RenewalDate.Date < DateTime.Today)
            throw new ArgumentException("Yenileme tarihi geçmiş olamaz.");

        subscription.Name = cmd.Name;
        subscription.Price = cmd.Price;
        subscription.RenewalPeriod = cmd.RenewalPeriod;
        subscription.RenewalDate = cmd.RenewalDate;

        await _repo.UpdateAsync(subscription, ct);
        return Unit.Value;
    }
}
