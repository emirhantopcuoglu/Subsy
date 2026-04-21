using FluentAssertions;
using MediatR;
using Moq;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Commands.CreateSubscription;
using Subsy.Application.Subscriptions.Commands.MarkSubscriptionAsPaid;
using Subsy.Domain.Entities;

namespace Subsy.Application.Tests.Subscriptions.Commands;

public class CreateSubscriptionHandlerTests
{
    private const string TestUserId = "user-123";
    private static readonly DateTime FixedToday = new(2026, 6, 15);

    private static Mock<IDateTimeProvider> CreateClockMock()
    {
        var mock = new Mock<IDateTimeProvider>();
        mock.Setup(c => c.Today).Returns(FixedToday);
        return mock;
    }

    [Fact]
    public async Task Handle_WhenSelectedDateIsInFuture_ShouldCreateWithThatDate()
    {
        // Arrange
        Subscription? captured = null;

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock
            .Setup(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .Callback<Subscription, CancellationToken>((sub, ct) => captured = sub)
            .Returns(Task.CompletedTask);

        var clockMock = CreateClockMock();

        var publisherMock = new Mock<IPublisher>();
        var handler = new CreateSubscriptionHandler(repoMock.Object, clockMock.Object, publisherMock.Object);

        var command = new CreateSubscriptionCommand(
            TestUserId, "Netflix", 99.99m, 30, SelectedMonth: 8, SelectedDay: 10);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.RenewalDate.Should().Be(new DateTime(2026, 8, 10));
        captured.Name.Should().Be("Netflix");
        captured.Price.Should().Be(99.99m);
        captured.UserId.Should().Be(TestUserId);
        captured.IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenSelectedDateIsInPast_ShouldCreateWithNextYear()
    {
        // Arrange
        Subscription? captured = null;

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock
            .Setup(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .Callback<Subscription, CancellationToken>((sub, ct) => captured = sub)
            .Returns(Task.CompletedTask);

        var clockMock = CreateClockMock();

        var publisherMock = new Mock<IPublisher>();
        var handler = new CreateSubscriptionHandler(repoMock.Object, clockMock.Object, publisherMock.Object);

        var command = new CreateSubscriptionCommand(
            TestUserId, "Spotify", 49.99m, 30, SelectedMonth: 3, SelectedDay: 1);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.RenewalDate.Should().Be(new DateTime(2027, 3, 1));
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryAddAsyncExactlyOnce()
    {
        // Arrange
        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock
            .Setup(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var clockMock = CreateClockMock();

        var publisherMock = new Mock<IPublisher>();
        var handler = new CreateSubscriptionHandler(repoMock.Object, clockMock.Object, publisherMock.Object);

        var command = new CreateSubscriptionCommand(
            TestUserId, "Netflix", 99.99m, 30, SelectedMonth: 8, SelectedDay: 10);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repoMock.Verify(
            r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
