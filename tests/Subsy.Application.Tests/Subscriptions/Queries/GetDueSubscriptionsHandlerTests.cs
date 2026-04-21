using FluentAssertions;
using Moq;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Queries.GetDueSubscriptions;
using Subsy.Domain.Entities;

namespace Subsy.Application.Tests.Subscriptions.Queries;

public class GetDueSubscriptionsHandlerTests
{
    private const string TestUserId = "user-123";
    private static readonly DateTime FixedToday = new(2026, 6, 15);

    private static Mock<IDateTimeProvider> CreateClockMock()
    {
        var mock = new Mock<IDateTimeProvider>();
        mock.Setup(c => c.Today).Returns(FixedToday);
        mock.Setup(c => c.UtcNow).Returns(FixedToday);
        return mock;
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyNonArchivedDueSubscriptions()
    {
        // Arrange
        var netflix = Subscription.Create(TestUserId, "Netflix", 99, 30, FixedToday);
        var spotify = Subscription.Create(TestUserId, "Spotify", 49, 30, FixedToday.AddDays(-3));
        var disney = Subscription.Create(TestUserId, "Disney", 79, 30, FixedToday.AddDays(5));
        var gym = Subscription.Create(TestUserId, "Gym", 200, 30, FixedToday.AddDays(-1));
        gym.Archive();

        var subscriptions = new List<Subscription> { netflix, spotify, disney, gym };

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock
            .Setup(r => r.GetAllByUserIdAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        var clockMock = CreateClockMock();
        var handler = new GetDueSubscriptionsHandler(repoMock.Object, clockMock.Object);
        var query = new GetDueSubscriptionsQuery(TestUserId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.Name == "Netflix");
        result.Should().Contain(d => d.Name == "Spotify");
        result.Should().NotContain(d => d.Name == "Disney");
        result.Should().NotContain(d => d.Name == "Gym");
    }

    [Fact]
    public async Task Handle_WhenNoDueSubscriptions_ShouldReturnEmptyList()
    {
        // Arrange
        var subscriptions = new List<Subscription>
        {
            Subscription.Create(TestUserId, "Netflix", 99, 30, FixedToday.AddDays(1))
        };

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock
            .Setup(r => r.GetAllByUserIdAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        var clockMock = CreateClockMock();
        var handler = new GetDueSubscriptionsHandler(repoMock.Object, clockMock.Object);
        var query = new GetDueSubscriptionsQuery(TestUserId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}