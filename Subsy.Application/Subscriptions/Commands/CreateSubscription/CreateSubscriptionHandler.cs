using MediatR;
using Microsoft.Extensions.Logging;
using Subsy.Application.Common.Events;
using Subsy.Application.Common.Interfaces;
using Subsy.Domain.Entities;

namespace Subsy.Application.Subscriptions.Commands.CreateSubscription;

public sealed class CreateSubscriptionHandler
    : IRequestHandler<CreateSubscriptionCommand, Unit>
{
    private readonly ISubscriptionRepository _repo;
    private readonly IDateTimeProvider _dateTime;
    private readonly IPublisher _publisher;
    private readonly ILogger<CreateSubscriptionHandler> _logger;

    public CreateSubscriptionHandler(
        ISubscriptionRepository repo,
        IDateTimeProvider dateTime,
        IPublisher publisher,
        ILogger<CreateSubscriptionHandler> logger)
    {
        _repo = repo;
        _dateTime = dateTime;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Unit> Handle(CreateSubscriptionCommand cmd, CancellationToken ct)
    {
        var year = _dateTime.Today.Year;
        var candidate = new DateTime(year, cmd.SelectedMonth, cmd.SelectedDay);

        if (candidate.Date < _dateTime.Today)
            candidate = candidate.AddYears(1);

        var subscription = Subscription.Create(
             cmd.UserId,
             cmd.Name,
             cmd.Price,
             cmd.Currency,
             cmd.RenewalPeriodDays,
             candidate);

        await _repo.AddAsync(subscription, ct);

        _logger.LogInformation("Subscription created: {SubscriptionId} '{Name}' for user {UserId}",
            subscription.Id, subscription.Name, cmd.UserId);

        await _publisher.Publish(new SubscriptionCreatedEvent(
            cmd.UserId, subscription.Id, subscription.Name), ct);
        return Unit.Value;
    }
}
