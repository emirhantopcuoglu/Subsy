using FluentAssertions;
using Moq;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Commands.MarkSubscriptionAsPaid;
using Subsy.Domain.Entities;

namespace Subsy.Application.Tests.Subscriptions.Commands;

public class MarkSubscriptionAsPaidHandlerTests
{
    private const string TestUserId = "user-123";

    private static Subscription CreateSubscription(DateTime renewalDate, int renewalPeriodDays = 30, bool isArchived = false)
    {
        return new Subscription
        {
            Id = 1,
            UserId = TestUserId,
            Name = "Netflix",
            Price = 99.99m,
            RenewalPeriodDays = renewalPeriodDays,
            RenewalDate = renewalDate,
            IsArchived = isArchived
        };
    }

    [Fact]
    public async Task Handle_WhenSubscriptionIsDueToday_ShouldAdvanceRenewalDateByPeriod()
    {
        // Arrange
        var today = DateTime.Today;
        var subscription = CreateSubscription(renewalDate: today, renewalPeriodDays: 30);

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var handler = new MarkSubscriptionAsPaidHandler(repoMock.Object);
        var command = new MarkSubscriptionAsPaidCommand(1, TestUserId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        subscription.RenewalDate.Should().Be(today.AddDays(30));
        repoMock.Verify(
            r => r.UpdateAsync(
                It.Is<Subscription>(s => s.Id == 1 && s.RenewalDate == today.AddDays(30)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSubscriptionIsNotDueYet_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var today = DateTime.Today;
        var subscription = CreateSubscription(renewalDate: today.AddDays(5), renewalPeriodDays: 30);

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var handler = new MarkSubscriptionAsPaidHandler(repoMock.Object);
        var command = new MarkSubscriptionAsPaidCommand(1, TestUserId);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert 
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*henüz*"); 

        subscription.RenewalDate.Should().Be(today.AddDays(5)); 

        repoMock.Verify(
            r => r.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}