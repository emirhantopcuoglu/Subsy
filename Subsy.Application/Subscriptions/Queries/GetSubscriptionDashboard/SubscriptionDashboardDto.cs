namespace Subsy.Application.Subscriptions.Queries.GetSubscriptionDashboard;

public sealed class SubscriptionDashboardDto
{
    public int ActiveCount { get; set; }
    public int TodayDueCount { get; set; }
    public decimal TotalThisMonth { get; set; }

    public List<UpcomingSubscriptionDto> Upcoming { get; set; } = new();
}

public sealed class UpcomingSubscriptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public DateTime RenewalDate { get; set; }
    public int RenewalPeriodDays { get; set; }
    public bool IsArchived { get; set; }
}
