using MediatR;

namespace Subsy.Application.Subscriptions.Commands.UpdateSubscription;

public sealed record UpdateSubscriptionCommand(
    int Id,
    string UserId,
    string Name,
    decimal Price,
    int RenewalPeriodDays,
    int SelectedMonth,
    int SelectedDay
) : IRequest<Unit>;
