using MediatR;

namespace Subsy.Application.Common.Events;

public sealed record SubscriptionCreatedEvent(
    string UserId, int SubscriptionId, string SubscriptionName) : INotification;