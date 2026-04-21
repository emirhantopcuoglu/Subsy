using MediatR;

namespace Subsy.Application.Common.Events;

public sealed record SubscriptionArchivedEvent(
    string UserId, int SubscriptionId, string SubscriptionName) : INotification;