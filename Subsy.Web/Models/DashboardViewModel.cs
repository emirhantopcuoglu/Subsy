namespace Subsy.Web.Models;

public sealed class DashboardViewModel
{
    public int ActiveCount { get; set; }
    public int TodayDueCount { get; set; }
    public decimal YearlyCost { get; set; }
    public decimal MonthlyCost { get; set; }
    public decimal DailyAverage { get; set; }
    public NextPaymentViewModel? NextPayment { get; set; }
    public List<SubscriptionsViewModel> Upcoming { get; set; } = new();
}

public sealed class NextPaymentViewModel
{
    public string Name { get; set; } = default!;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int DaysLeft { get; set; }
}
