using MediatR;

namespace Subsy.Application.Subscriptions.Commands.CreateSubscription;

public sealed record CreateSubscriptionCommand(
    string UserId,
    string Name,
    decimal Price,
    int RenewalPeriodDays,
    int SelectedMonth,
    int SelectedDay
) : IRequest<Unit>;