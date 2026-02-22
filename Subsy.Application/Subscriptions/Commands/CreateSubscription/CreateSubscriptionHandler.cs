using MediatR;
using Subsy.Application.Common.Interfaces;
using Subsy.Domain.Entities;

namespace Subsy.Application.Subscriptions.Commands.CreateSubscription;

public sealed class CreateSubscriptionHandler
    : IRequestHandler<CreateSubscriptionCommand, Unit>
{
    private readonly ISubscriptionRepository _repo;

    public CreateSubscriptionHandler(ISubscriptionRepository repo)
    {
        _repo = repo;
    }

    public async Task<Unit> Handle(CreateSubscriptionCommand cmd, CancellationToken ct)
    {
        var year = DateTime.Today.Year;
        var candidate = new DateTime(year, cmd.SelectedMonth, cmd.SelectedDay);

        if (candidate.Date < DateTime.Today)
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
