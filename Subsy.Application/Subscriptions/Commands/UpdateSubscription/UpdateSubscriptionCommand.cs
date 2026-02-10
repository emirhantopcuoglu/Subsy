using MediatR;

namespace Subsy.Application.Subscriptions.Commands.UpdateSubscription;

public sealed record UpdateSubscriptionCommand(
    int Id,
    string UserId,
    string Name,
    decimal Price,
    string RenewalPeriod,
    DateTime RenewalDate
) : IRequest<Unit>;
