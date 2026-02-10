using MediatR;

namespace Subsy.Application.Subscriptions.Commands.CreateSubscription;

public sealed record CreateSubscriptionCommand(
    string UserId,
    string Name,
    decimal Price,
    string RenewalPeriod,
    DateTime RenewalDate
) : IRequest<Unit>;
