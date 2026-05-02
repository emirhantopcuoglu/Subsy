using Subsy.Application.Finance.Dashboard;

namespace Subsy.Web.Models;

public class FinanceViewModel
{
    public decimal TotalMonthlyCost { get; set; }
    public decimal TotalYearlyCost { get; set; }
    public decimal DailyAverage { get; set; }
    public string Currency { get; set; } = "TRY";
    public int SubscriptionCount { get; set; }
    public ServiceSummaryDto? TopSpendingService { get; set; }
    public List<CategorySummaryDto> GroupedByCategory { get; set; } = new();
    public List<InsightDto> Insights { get; set; } = new();
    public List<PaymentCalendarWeek> PaymentCalendar { get; set; } = new();
    public List<SubscriptionCostRow> CostTable { get; set; } = new();
}
