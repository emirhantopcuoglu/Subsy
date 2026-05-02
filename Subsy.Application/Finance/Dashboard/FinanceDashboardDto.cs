namespace Subsy.Application.Finance.Dashboard;

public sealed class FinanceDashboardDto
{
    public decimal TotalMonthlyCost { get; init; }
    public decimal AllTimeSpending { get; init; }
    public int SubscriptionCount { get; init; }
    public string Currency { get; init; } = "TRY";
    public ServiceSummaryDto? TopSpendingService { get; init; }
    public List<ServiceSummaryDto> GroupedByService { get; init; } = new();
    public List<CategorySummaryDto> GroupedByCategory { get; init; } = new();
    public List<MonthlyTrendPoint> TwelveMonthTrend { get; init; } = new();
    public decimal YearlyPlanTotal { get; init; }
    public decimal MonthlyPlanTotal { get; init; }
}

public sealed class ServiceSummaryDto
{
    public string SubscriptionName { get; init; } = default!;
    public decimal TotalCost { get; init; }
}

public sealed class CategorySummaryDto
{
    public string CategoryName { get; init; } = default!;
    public decimal TotalCost { get; init; }
    public int Count { get; init; }
}

public sealed record MonthlyTrendPoint(string Label, decimal Amount);
