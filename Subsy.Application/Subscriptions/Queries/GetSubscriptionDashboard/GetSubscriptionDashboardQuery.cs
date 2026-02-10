using MediatR;

namespace Subsy.Application.Subscriptions.Queries.GetSubscriptionDashboard;

public sealed record GetSubscriptionDashboardQuery(string UserId)
    : IRequest<SubscriptionDashboardDto>;
