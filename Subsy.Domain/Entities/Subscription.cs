using Subsy.Domain.Enums;

namespace Subsy.Domain.Entities;

public class Subscription
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!;
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "TRY";
    public int RenewalPeriodDays { get; private set; }
    public DateTime RenewalDate { get; private set; }
    public string UserId { get; private set; } = default!;
    public bool IsArchived { get; private set; }
    public SubscriptionCategory Category { get; private set; }
    public string? WebsiteUrl { get; private set; }

    private Subscription() { }

    public static Subscription Create(
        string userId,
        string name,
        decimal price,
        string currency,
        int renewalPeriodDays,
        DateTime firstRenewalDate,
        SubscriptionCategory category = SubscriptionCategory.Other,
        string? websiteUrl = null)
    {
        return new Subscription
        {
            UserId = userId,
            Name = name,
            Price = price,
            Currency = currency.ToUpperInvariant(),
            RenewalPeriodDays = renewalPeriodDays,
            RenewalDate = firstRenewalDate,
            IsArchived = false,
            Category = category,
            WebsiteUrl = websiteUrl?.Trim()
        };
    }

    public void MarkAsPaid(DateTime today)
    {
        if (IsArchived)
            throw new InvalidOperationException("Arşivlenmiş abonelik ödenemez.");

        if (RenewalDate.Date > today.Date)
            throw new InvalidOperationException("Ödeme günü henüz gelmedi.");

        RenewalDate = RenewalDate.AddDays(RenewalPeriodDays);
    }

    public void Archive()
    {
        if (IsArchived)
            throw new InvalidOperationException("Abonelik zaten arşivlenmiş.");

        IsArchived = true;
    }

    public void Unarchive()
    {
        if (!IsArchived)
            throw new InvalidOperationException("Abonelik zaten aktif.");

        IsArchived = false;
    }

    public void UpdateDetails(
        string name,
        decimal price,
        string currency,
        int renewalPeriodDays,
        DateTime newRenewalDate,
        SubscriptionCategory category = SubscriptionCategory.Other,
        string? websiteUrl = null)
    {
        Name = name;
        Price = price;
        Currency = currency.ToUpperInvariant();
        RenewalPeriodDays = renewalPeriodDays;
        RenewalDate = newRenewalDate;
        Category = category;
        WebsiteUrl = websiteUrl?.Trim();
    }
}