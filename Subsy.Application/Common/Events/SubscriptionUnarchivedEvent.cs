using MediatR;

namespace Subsy.Application.Common.Events;

public sealed record SubscriptionUnarchivedEvent(
    string UserId, int SubscriptionId, string SubscriptionName) : INotification;
