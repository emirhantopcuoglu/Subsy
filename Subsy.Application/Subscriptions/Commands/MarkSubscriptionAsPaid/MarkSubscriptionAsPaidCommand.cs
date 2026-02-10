using MediatR;

namespace Subsy.Application.Subscriptions.Commands.MarkSubscriptionAsPaid;

public sealed record MarkSubscriptionAsPaidCommand(
    int Id,
    string UserId
) : IRequest<Unit>;
