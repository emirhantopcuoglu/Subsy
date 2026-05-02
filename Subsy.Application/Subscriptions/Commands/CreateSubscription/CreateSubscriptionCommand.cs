using MediatR;
using Subsy.Domain.Enums;

namespace Subsy.Application.Subscriptions.Commands.CreateSubscription;

public sealed record CreateSubscriptionCommand(
    string UserId,
    string Name,
    decimal Price,
    string Currency,
    int RenewalPeriodDays,
    int SelectedMonth,
    int SelectedDay,
    SubscriptionCategory Category = SubscriptionCategory.Other,
    string? WebsiteUrl = null
) : IRequest<Unit>;