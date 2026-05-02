namespace Subsy.Web.Models;

public class FinanceViewModel
{
    public decimal TotalMonthlyCost { get; set; }
    public decimal AllTimeSpending { get; set; }
    public string Currency { get; set; } = "TRY";
    public List<ServiceSummary> GroupedByService { get; set; } = new();
    public ServiceSummary? TopSpendingService { get; set; }
    public int SubscriptionCount { get; set; }
    public List<CategorySummary> GroupedByCategory { get; set; } = new();
    public List<(string Label, decimal Amount)> TwelveMonthTrend { get; set; } = new();
    public decimal YearlyPlanTotal { get; set; }
    public decimal MonthlyPlanTotal { get; set; }
}

public class ServiceSummary
{
    public string SubscriptionName { get; set; } = default!;
    public decimal TotalCost { get; set; }
}

public class CategorySummary
{
    public string CategoryName { get; set; } = default!;
    public decimal TotalCost { get; set; }
    public int Count { get; set; }
}
