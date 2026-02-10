using MediatR;

namespace Subsy.Application.Subscriptions.Commands.UnarchiveSubscription;

public sealed record UnarchiveSubscriptionCommand(
    int Id,
    string UserId
) : IRequest<Unit>;
