using Subsy.Domain.Enums;

namespace Subsy.Application.Subscriptions.Queries.Common;

public sealed class SubscriptionDto
{
    public int Id { get; init; }
    public string Name { get; init; } = default!;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "TRY";
    public int RenewalPeriodDays { get; init; }
    public DateTime RenewalDate { get; init; }
    public bool IsArchived { get; init; }
    public SubscriptionCategory Category { get; init; }
}
