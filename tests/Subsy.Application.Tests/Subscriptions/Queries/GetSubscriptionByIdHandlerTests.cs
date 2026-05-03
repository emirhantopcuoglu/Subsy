using FluentAssertions;
using Moq;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Queries.GetSubscriptionById;
using Subsy.Domain.Entities;
using Subsy.Domain.Enums;

namespace Subsy.Application.Tests.Subscriptions.Queries;

public class GetSubscriptionByIdHandlerTests
{
    private const string TestUserId = "user-123";
    private static readonly DateTime RenewalDate = new(2026, 9, 1);

    [Fact]
    public async Task Handle_WhenSubscriptionExists_ShouldReturnDto()
    {
        var sub = Subscription.Create(TestUserId, "Netflix", 99m, "TRY", 30, RenewalDate, SubscriptionCategory.Entertainment);

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        var handler = new GetSubscriptionByIdHandler(repoMock.Object);

        var result = await handler.Handle(new GetSubscriptionByIdQuery(1, TestUserId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Netflix");
        result.Price.Should().Be(99m);
        result.Currency.Should().Be("TRY");
        result.RenewalPeriodDays.Should().Be(30);
        result.RenewalDate.Should().Be(RenewalDate);
        result.IsArchived.Should().BeFalse();
        result.Category.Should().Be(SubscriptionCategory.Entertainment);
    }

    [Fact]
    public async Task Handle_WhenSubscriptionNotFound_ShouldReturnNull()
    {
        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(99, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var handler = new GetSubscriptionByIdHandler(repoMock.Object);

        var result = await handler.Handle(new GetSubscriptionByIdQuery(99, TestUserId), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenSubscriptionIsArchived_ShouldStillReturnDto()
    {
        var sub = Subscription.Create(TestUserId, "Netflix", 99m, "TRY", 30, RenewalDate);
        sub.Archive();

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        var handler = new GetSubscriptionByIdHandler(repoMock.Object);

        var result = await handler.Handle(new GetSubscriptionByIdQuery(1, TestUserId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsArchived.Should().BeTrue();
    }
}
