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
        var subscriptions = new List<Subscription>
        {
            new() { Id = 1, UserId = TestUserId, Name = "Netflix",  Price = 99, RenewalPeriodDays = 30, RenewalDate = FixedToday, IsArchived = false },
            new() { Id = 2, UserId = TestUserId, Name = "Spotify",  Price = 49, RenewalPeriodDays = 30, RenewalDate = FixedToday.AddDays(-3), IsArchived = false },
            new() { Id = 3, UserId = TestUserId, Name = "Disney",   Price = 79, RenewalPeriodDays = 30, RenewalDate = FixedToday.AddDays(5), IsArchived = false },
            new() { Id = 4, UserId = TestUserId, Name = "Gym",      Price = 200, RenewalPeriodDays = 30, RenewalDate = FixedToday.AddDays(-1), IsArchived = true }
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
            new() { Id = 1, UserId = TestUserId, Name = "Netflix", Price = 99, RenewalPeriodDays = 30, RenewalDate = FixedToday.AddDays(1), IsArchived = false }
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