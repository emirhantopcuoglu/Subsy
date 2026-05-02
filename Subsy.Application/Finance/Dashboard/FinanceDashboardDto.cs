namespace Subsy.Application.Finance.Dashboard;

public sealed class FinanceDashboardDto
{
    public decimal TotalMonthlyCost { get; init; }
    public decimal TotalYearlyCost { get; init; }
    public decimal DailyAverage { get; init; }
    public int SubscriptionCount { get; init; }
    public string Currency { get; init; } = "TRY";
    public ServiceSummaryDto? TopSpendingService { get; init; }
    public List<ServiceSummaryDto> GroupedByService { get; init; } = new();
    public List<CategorySummaryDto> GroupedByCategory { get; init; } = new();
    public List<InsightDto> Insights { get; init; } = new();
    public List<PaymentCalendarWeek> PaymentCalendar { get; init; } = new();
    public List<SubscriptionCostRow> CostTable { get; init; } = new();
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

public sealed record InsightDto(string Icon, string Message, string Type);

public sealed class PaymentCalendarWeek
{
    public string Label { get; init; } = default!;
    public decimal Total { get; init; }
    public List<PaymentCalendarItem> Items { get; init; } = new();
}

public sealed class PaymentCalendarItem
{
    public string Name { get; init; } = default!;
    public DateTime Date { get; init; }
    public decimal Amount { get; init; }
}

public sealed class SubscriptionCostRow
{
    public string Name { get; init; } = default!;
    public string CategoryName { get; init; } = default!;
    public string Period { get; init; } = default!;
    public decimal OriginalPrice { get; init; }
    public string OriginalCurrency { get; init; } = default!;
    public decimal MonthlyCost { get; init; }
    public decimal DailyCost { get; init; }
}
