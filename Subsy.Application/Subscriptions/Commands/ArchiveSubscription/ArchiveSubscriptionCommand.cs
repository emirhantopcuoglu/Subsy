using MediatR;

namespace Subsy.Application.Subscriptions.Commands.ArchiveSubscription;

public sealed record ArchiveSubscriptionCommand(
    int Id,
    string UserId
) : IRequest<Unit>;
