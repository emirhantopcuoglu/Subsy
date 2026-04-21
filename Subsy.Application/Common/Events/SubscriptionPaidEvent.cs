using MediatR;

namespace Subsy.Application.Common.Events;

public sealed record SubscriptionPaidEvent(
    string UserId, int SubscriptionId, string SubscriptionName,
    DateTime OldRenewalDate, DateTime NewRenewalDate) : INotification;