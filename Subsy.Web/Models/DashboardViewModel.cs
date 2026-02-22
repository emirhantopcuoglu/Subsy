namespace Subsy.Web.Models;

public sealed class DashboardViewModel
{
    public int ActiveCount { get; set; }
    public int TodayDueCount { get; set; }
    public decimal TotalThisMonth { get; set; }

    public List<SubscriptionsViewModel> Upcoming { get; set; } = new();
}