using FluentAssertions;
using MediatR;
using Moq;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Commands.MarkSubscriptionAsPaid;
using Subsy.Domain.Entities;

namespace Subsy.Application.Tests.Subscriptions.Commands;

public class MarkSubscriptionAsPaidHandlerTests
{
    private const string TestUserId = "user-123";
    private static readonly DateTime FixedToday = new(2026, 6, 15);

    private static Subscription CreateSubscription(
        DateTime renewalDate,
        int renewalPeriodDays = 30,
        bool isArchived = false)
    {
        var sub = Subscription.Create(
            TestUserId, "Netflix", 99.99m, "TRY", renewalPeriodDays, renewalDate);

        if (isArchived)
            sub.Archive();

        return sub;
    }

    private static Mock<IDateTimeProvider> CreateClockMock()
    {
        var mock = new Mock<IDateTimeProvider>();
        mock.Setup(c => c.Today).Returns(FixedToday);
        mock.Setup(c => c.UtcNow).Returns(FixedToday);
        return mock;
    }

    [Fact]
    public async Task Handle_WhenSubscriptionIsDueToday_ShouldAdvanceRenewalDateByPeriod()
    {
        // Arrange
        var subscription = CreateSubscription(renewalDate: FixedToday, renewalPeriodDays: 30);

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var clockMock = CreateClockMock();
        var publisherMock = new Mock<IPublisher>();
        var handler = new MarkSubscriptionAsPaidHandler(repoMock.Object, clockMock.Object, publisherMock.Object);
        var command = new MarkSubscriptionAsPaidCommand(1, TestUserId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        subscription.RenewalDate.Should().Be(FixedToday.AddDays(30));
        repoMock.Verify(
            r => r.UpdateAsync(It.Is<Subscription>(s => s.Name == "Netflix"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSubscriptionIsNotDueYet_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var subscription = CreateSubscription(renewalDate: FixedToday.AddDays(5));

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var clockMock = CreateClockMock();
        var publisherMock = new Mock<IPublisher>();
        var handler = new MarkSubscriptionAsPaidHandler(repoMock.Object, clockMock.Object, publisherMock.Object);
        var command = new MarkSubscriptionAsPaidCommand(1, TestUserId);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        subscription.RenewalDate.Should().Be(FixedToday.AddDays(5));
        repoMock.Verify(
            r => r.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSubscriptionIsOverdue_ShouldStillAdvanceFromOriginalDate()
    {
        // Arrange
        var overdueDate = FixedToday.AddDays(-3);
        var subscription = CreateSubscription(renewalDate: overdueDate, renewalPeriodDays: 30);

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var clockMock = CreateClockMock();
        var publisherMock = new Mock<IPublisher>();
        var handler = new MarkSubscriptionAsPaidHandler(repoMock.Object, clockMock.Object, publisherMock.Object);
        var command = new MarkSubscriptionAsPaidCommand(1, TestUserId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        subscription.RenewalDate.Should().Be(overdueDate.AddDays(30));
    }
}