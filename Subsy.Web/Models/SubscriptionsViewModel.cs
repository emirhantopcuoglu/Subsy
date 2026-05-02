using Subsy.Domain.Enums;

namespace Subsy.Web.Models;

public sealed class SubscriptionsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "TRY";
    public int RenewalPeriodDays { get; set; }
    public int SelectedMonth { get; set; }
    public int SelectedDay { get; set; }
    public DateTime RenewalDate { get; set; }
    public bool IsArchived { get; set; }
    public SubscriptionCategory Category { get; set; }
    public string? WebsiteUrl { get; set; }
}
