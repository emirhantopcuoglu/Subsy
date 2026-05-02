namespace Subsy.Application.Subscriptions.Queries.GetSubscriptionDashboard;

public sealed class SubscriptionDashboardDto
{
    public int ActiveCount { get; set; }
    public int TodayDueCount { get; set; }
    public decimal TotalThisMonth { get; set; }
    public decimal YearlyCost { get; set; }
    public decimal PaidThisMonth { get; set; }
    public decimal RemainingThisMonth { get; set; }
    public List<MonthlySpendPoint> SixMonthTrend { get; set; } = new();
    public List<UpcomingSubscriptionDto> Upcoming { get; set; } = new();
}

public sealed record MonthlySpendPoint(string Label, decimal Amount);

public sealed class UpcomingSubscriptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public DateTime RenewalDate { get; set; }
    public int RenewalPeriodDays { get; set; }
    public bool IsArchived { get; set; }
}
