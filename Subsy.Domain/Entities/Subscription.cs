namespace Subsy.Domain.Entities;

public class Subscription
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;

    public decimal Price { get; set; }

    public string RenewalPeriod { get; set; } = default!;

    public DateTime RenewalDate { get; set; }

    public string UserId { get; set; } = default!;

    public bool IsArchived { get; set; } = false;
}
