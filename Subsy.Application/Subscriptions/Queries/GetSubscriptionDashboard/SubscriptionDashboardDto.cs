namespace Subsy.Application.Subscriptions.Queries.GetSubscriptionDashboard;

public sealed class SubscriptionDashboardDto
{
    public int ActiveCount { get; set; }
    public int TodayDueCount { get; set; }
    public decimal YearlyCost { get; set; }
    public decimal MonthlyCost { get; set; }
    public decimal DailyAverage { get; set; }
    public NextPaymentDto? NextPayment { get; set; }
    public List<UpcomingSubscriptionDto> Upcoming { get; set; } = new();
}

public sealed class NextPaymentDto
{
    public string Name { get; set; } = default!;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int DaysLeft { get; set; }
}

public sealed class UpcomingSubscriptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "TRY";
    public DateTime RenewalDate { get; set; }
    public int RenewalPeriodDays { get; set; }
    public bool IsArchived { get; set; }
}
