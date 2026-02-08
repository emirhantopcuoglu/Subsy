namespace Subsy.Web.Models;

public sealed class SubscriptionsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public string RenewalPeriod { get; set; } = default!;
    public DateTime RenewalDate { get; set; }
    public bool IsArchived { get; set; }
}
