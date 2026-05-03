using FluentAssertions;
using Moq;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Queries.GetActiveSubscriptions;
using Subsy.Domain.Entities;
using Subsy.Domain.Enums;

namespace Subsy.Application.Tests.Subscriptions.Queries;

public class GetActiveSubscriptionsHandlerTests
{
    private const string TestUserId = "user-123";
    private static readonly DateTime RenewalDate = new(2026, 9, 1);

    [Fact]
    public async Task Handle_ShouldReturnDtosForAllSubscriptions()
    {
        var subs = new List<Subscription>
        {
            Subscription.Create(TestUserId, "Netflix", 99m, "TRY", 30, RenewalDate, SubscriptionCategory.Entertainment),
            Subscription.Create(TestUserId, "Spotify", 49m, "USD", 90, RenewalDate.AddMonths(1))
        };

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetActiveByUserIdAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subs);

        var handler = new GetActiveSubscriptionsHandler(repoMock.Object);

        var result = await handler.Handle(new GetActiveSubscriptionsQuery(TestUserId), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldMapAllDtoFieldsCorrectly()
    {
        var sub = Subscription.Create(TestUserId, "Netflix", 99.99m, "usd", 30, RenewalDate, SubscriptionCategory.Entertainment);

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetActiveByUserIdAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Subscription> { sub });

        var handler = new GetActiveSubscriptionsHandler(repoMock.Object);

        var result = await handler.Handle(new GetActiveSubscriptionsQuery(TestUserId), CancellationToken.None);

        var dto = result.Single();
        dto.Name.Should().Be("Netflix");
        dto.Price.Should().Be(99.99m);
        dto.Currency.Should().Be("USD");
        dto.RenewalPeriodDays.Should().Be(30);
        dto.RenewalDate.Should().Be(RenewalDate);
        dto.IsArchived.Should().BeFalse();
        dto.Category.Should().Be(SubscriptionCategory.Entertainment);
    }

    [Fact]
    public async Task Handle_WhenNoActiveSubscriptions_ShouldReturnEmptyList()
    {
        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetActiveByUserIdAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Subscription>());

        var handler = new GetActiveSubscriptionsHandler(repoMock.Object);

        var result = await handler.Handle(new GetActiveSubscriptionsQuery(TestUserId), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
