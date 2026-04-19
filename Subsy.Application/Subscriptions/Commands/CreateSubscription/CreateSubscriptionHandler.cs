using MediatR;
using Subsy.Application.Common.Interfaces;
using Subsy.Domain.Entities;

namespace Subsy.Application.Subscriptions.Commands.CreateSubscription;

public sealed class CreateSubscriptionHandler
    : IRequestHandler<CreateSubscriptionCommand, Unit>
{
    private readonly ISubscriptionRepository _repo;
    private readonly IDateTimeProvider _dateTime;
    public CreateSubscriptionHandler(ISubscriptionRepository repo, IDateTimeProvider dateTime)
    {
        _repo = repo;
        _dateTime = dateTime;
    }

    public async Task<Unit> Handle(CreateSubscriptionCommand cmd, CancellationToken ct)
    {
        var year = _dateTime.Today.Year;
        var candidate = new DateTime(year, cmd.SelectedMonth, cmd.SelectedDay);

        if (candidate.Date < _dateTime.Today)
            candidate = candidate.AddYears(1);

        var subscription = new Subscription
        {
            UserId = cmd.UserId,
            Name = cmd.Name,
            Price = cmd.Price,
            RenewalPeriodDays = cmd.RenewalPeriodDays,
            RenewalDate = candidate,
            IsArchived = false
        };

        await _repo.AddAsync(subscription, ct);
        return Unit.Value;
    }
}
