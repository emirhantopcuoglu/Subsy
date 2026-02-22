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
        var subscription = await _repo.GetByIdAsync(cmd.Id, cmd.UserId, ct);
        if (subscription is null)
            throw new KeyNotFoundException("Subscription not found.");

        var year = DateTime.Today.Year;
        var candidate = new DateTime(year, cmd.SelectedMonth, cmd.SelectedDay);
        if (candidate.Date < DateTime.Today)
            candidate = candidate.AddYears(1);

        subscription.Name = cmd.Name;
        subscription.Price = cmd.Price;
        subscription.RenewalPeriodDays = cmd.RenewalPeriodDays;
        subscription.RenewalDate = candidate;

        await _repo.UpdateAsync(subscription, ct);
        return Unit.Value;
    }
}