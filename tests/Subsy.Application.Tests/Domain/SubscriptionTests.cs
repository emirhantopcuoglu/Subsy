using FluentAssertions;
using Subsy.Domain.Entities;
using Subsy.Domain.Enums;

namespace Subsy.Application.Tests.Domain;

public class SubscriptionTests
{
    private static readonly DateTime SomeDate = new(2026, 6, 15);

    // ── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var sub = Subscription.Create("u1", "Netflix", 99.99m, "usd", 30, SomeDate, SubscriptionCategory.Entertainment);

        sub.UserId.Should().Be("u1");
        sub.Name.Should().Be("Netflix");
        sub.Price.Should().Be(99.99m);
        sub.Currency.Should().Be("USD");
        sub.RenewalPeriodDays.Should().Be(30);
        sub.RenewalDate.Should().Be(SomeDate);
        sub.Category.Should().Be(SubscriptionCategory.Entertainment);
        sub.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldNormalizeCurrencyToUpperCase()
    {
        var sub = Subscription.Create("u1", "Test", 10m, "eur", 30, SomeDate);
        sub.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Create_ShouldDefaultCategoryToOther()
    {
        var sub = Subscription.Create("u1", "Test", 10m, "TRY", 30, SomeDate);
        sub.Category.Should().Be(SubscriptionCategory.Other);
    }

    // ── MarkAsPaid ───────────────────────────────────────────────────────────

    [Fact]
    public void MarkAsPaid_WhenDueToday_ShouldAdvanceRenewalDate()
    {
        var sub = Subscription.Create("u1", "Netflix", 99m, "TRY", 30, SomeDate);

        sub.MarkAsPaid(SomeDate);

        sub.RenewalDate.Should().Be(SomeDate.AddDays(30));
    }

    [Fact]
    public void MarkAsPaid_WhenOverdue_ShouldAdvanceFromOriginalDate()
    {
        var overdueDate = SomeDate.AddDays(-5);
        var sub = Subscription.Create("u1", "Netflix", 99m, "TRY", 30, overdueDate);

        sub.MarkAsPaid(SomeDate);

        sub.RenewalDate.Should().Be(overdueDate.AddDays(30));
    }

    [Fact]
    public void MarkAsPaid_WhenArchived_ShouldThrowInvalidOperationException()
    {
        var sub = Subscription.Create("u1", "Netflix", 99m, "TRY", 30, SomeDate);
        sub.Archive();

        Action act = () => sub.MarkAsPaid(SomeDate);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*arşivlenmiş*");
    }

    [Fact]
    public void MarkAsPaid_WhenRenewalDateIsInFuture_ShouldThrowInvalidOperationException()
    {
        var sub = Subscription.Create("u1", "Netflix", 99m, "TRY", 30, SomeDate.AddDays(1));

        Action act = () => sub.MarkAsPaid(SomeDate);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*henüz gelmedi*");
    }

    [Theory]
    [InlineData(7)]
    [InlineData(15)]
    [InlineData(90)]
    [InlineData(365)]
    public void MarkAsPaid_ShouldAdvanceByCorrectPeriod(int periodDays)
    {
        var sub = Subscription.Create("u1", "Test", 10m, "TRY", periodDays, SomeDate);

        sub.MarkAsPaid(SomeDate);

        sub.RenewalDate.Should().Be(SomeDate.AddDays(periodDays));
    }

    // ── Archive ──────────────────────────────────────────────────────────────

    [Fact]
    public void Archive_ShouldSetIsArchivedTrue()
    {
        var sub = Subscription.Create("u1", "Test", 10m, "TRY", 30, SomeDate);

        sub.Archive();

        sub.IsArchived.Should().BeTrue();
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ShouldThrowInvalidOperationException()
    {
        var sub = Subscription.Create("u1", "Test", 10m, "TRY", 30, SomeDate);
        sub.Archive();

        Action act = () => sub.Archive();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*zaten arşivlenmiş*");
    }

    // ── Unarchive ────────────────────────────────────────────────────────────

    [Fact]
    public void Unarchive_ShouldSetIsArchivedFalse()
    {
        var sub = Subscription.Create("u1", "Test", 10m, "TRY", 30, SomeDate);
        sub.Archive();

        sub.Unarchive();

        sub.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void Unarchive_WhenNotArchived_ShouldThrowInvalidOperationException()
    {
        var sub = Subscription.Create("u1", "Test", 10m, "TRY", 30, SomeDate);

        Action act = () => sub.Unarchive();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*zaten aktif*");
    }

    // ── UpdateDetails ────────────────────────────────────────────────────────

    [Fact]
    public void UpdateDetails_ShouldUpdateAllFields()
    {
        var sub = Subscription.Create("u1", "Old", 10m, "TRY", 30, SomeDate);
        var newDate = SomeDate.AddMonths(1);

        sub.UpdateDetails("New Name", 199m, "eur", 90, newDate, SubscriptionCategory.Health);

        sub.Name.Should().Be("New Name");
        sub.Price.Should().Be(199m);
        sub.Currency.Should().Be("EUR");
        sub.RenewalPeriodDays.Should().Be(90);
        sub.RenewalDate.Should().Be(newDate);
        sub.Category.Should().Be(SubscriptionCategory.Health);
    }

    [Fact]
    public void UpdateDetails_ShouldNormalizeCurrencyToUpperCase()
    {
        var sub = Subscription.Create("u1", "Test", 10m, "TRY", 30, SomeDate);

        sub.UpdateDetails("Test", 10m, "usd", 30, SomeDate);

        sub.Currency.Should().Be("USD");
    }
}
