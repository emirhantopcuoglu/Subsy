using Subsy.Domain.Entities;

namespace Subsy.Application.Subscriptions.Dashboard;

public sealed class SubscriptionDashboardDto
{
    public int ActiveCount { get; init; }
    public int TodayDueCount { get; init; }
    public decimal TotalThisMonth { get; init; }
    public List<Subscription> Upcoming { get; init; } = new();
}
