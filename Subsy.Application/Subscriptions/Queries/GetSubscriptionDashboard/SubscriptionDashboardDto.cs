namespace Subsy.Application.Subscriptions.Queries.GetSubscriptionDashboard;

public sealed class SubscriptionDashboardDto
{
    public int TotalCount { get; init; }
    public int ActiveCount { get; init; }
    public int ArchivedCount { get; init; }
    public decimal TotalActivePrice { get; init; }
    public List<UpcomingRenewalDto> UpcomingRenewals { get; init; } = new();
}

public sealed class UpcomingRenewalDto
{
    public int Id { get; init; }
    public string Name { get; init; } = default!;
    public decimal Price { get; init; }
    public DateTime RenewalDate { get; init; }
}
