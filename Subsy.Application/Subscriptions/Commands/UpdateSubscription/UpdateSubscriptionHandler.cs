using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Commands.UpdateSubscription;

public sealed class UpdateSubscriptionHandler
    : IRequestHandler<UpdateSubscriptionCommand, Unit>
{
    private readonly ISubscriptionRepository _repo;
    private readonly IDateTimeProvider _dateTime;

    public UpdateSubscriptionHandler(ISubscriptionRepository repo, IDateTimeProvider dateTime)
    {
        _repo = repo;
        _dateTime = dateTime;
    }

    public async Task<Unit> Handle(UpdateSubscriptionCommand cmd, CancellationToken ct)
    {
        var subscription = await _repo.GetByIdAsync(cmd.Id, cmd.UserId, ct);
        if (subscription is null)
            throw new KeyNotFoundException("Subscription not found.");

        var year = _dateTime.Today.Year;
        var candidate = new DateTime(year, cmd.SelectedMonth, cmd.SelectedDay);
        if (candidate.Date < _dateTime.Today)
            candidate = candidate.AddYears(1);

        subscription.UpdateDetails(cmd.Name, cmd.Price, cmd.Currency, cmd.RenewalPeriodDays, candidate);

        await _repo.UpdateAsync(subscription, ct);
        return Unit.Value;
    }
}