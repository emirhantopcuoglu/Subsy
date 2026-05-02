namespace Subsy.Web.Models;

public sealed class DashboardViewModel
{
    public int ActiveCount { get; set; }
    public int TodayDueCount { get; set; }
    public decimal TotalThisMonth { get; set; }
    public decimal YearlyCost { get; set; }
    public decimal PaidThisMonth { get; set; }
    public decimal RemainingThisMonth { get; set; }
    public List<(string Label, decimal Amount)> SixMonthTrend { get; set; } = new();
    public List<SubscriptionsViewModel> Upcoming { get; set; } = new();
}
