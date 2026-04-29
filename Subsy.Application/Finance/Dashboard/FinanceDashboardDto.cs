namespace Subsy.Application.Finance.Dashboard;

public sealed class FinanceDashboardDto
{
    public decimal TotalMonthlyCost { get; init; }
    public decimal AllTimeSpending { get; init; }
    public int SubscriptionCount { get; init; }
    public string Currency { get; init; } = "TRY";
    public ServiceSummaryDto? TopSpendingService { get; init; }
    public List<ServiceSummaryDto> GroupedByService { get; init; } = new();
}

public sealed class ServiceSummaryDto
{
    public string SubscriptionName { get; init; } = default!;
    public decimal TotalCost { get; init; }
}
