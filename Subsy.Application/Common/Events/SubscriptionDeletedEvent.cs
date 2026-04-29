using MediatR;

namespace Subsy.Application.Common.Events;

public sealed record SubscriptionDeletedEvent(
    string UserId, int SubscriptionId, string SubscriptionName) : INotification;
