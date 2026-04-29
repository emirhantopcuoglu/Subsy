using MediatR;

namespace Subsy.Application.Common.Events;

public sealed record SubscriptionUpdatedEvent(
    string UserId, int SubscriptionId, string SubscriptionName) : INotification;
