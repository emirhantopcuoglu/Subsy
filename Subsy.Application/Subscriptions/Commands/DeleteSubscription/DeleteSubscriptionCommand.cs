using MediatR;

namespace Subsy.Application.Subscriptions.Commands.DeleteSubscription;

public sealed record DeleteSubscriptionCommand(
    int Id,
    string UserId
) : IRequest<Unit>;
