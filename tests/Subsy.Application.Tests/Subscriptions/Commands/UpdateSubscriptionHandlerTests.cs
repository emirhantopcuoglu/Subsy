using FluentAssertions;
using MediatR;
using Moq;
using Subsy.Application.Common.Events;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Commands.UpdateSubscription;
using Subsy.Domain.Entities;
using Subsy.Domain.Enums;

namespace Subsy.Application.Tests.Subscriptions.Commands;

public class UpdateSubscriptionHandlerTests
{
    private const string TestUserId = "user-123";
    private static readonly DateTime FixedToday = new(2026, 6, 15);

    private static Mock<IDateTimeProvider> CreateClockMock()
    {
        var mock = new Mock<IDateTimeProvider>();
        mock.Setup(c => c.Today).Returns(FixedToday);
        return mock;
    }

    private static Subscription CreateSubscription()
        => Subscription.Create(TestUserId, "Netflix", 99m, "TRY", 30, new DateTime(2026, 8, 1));

    [Fact]
    public async Task Handle_WhenSelectedDateIsInFuture_ShouldUpdateWithThatDate()
    {
        var subscription = CreateSubscription();

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var handler = new UpdateSubscriptionHandler(repoMock.Object, CreateClockMock().Object, new Mock<IPublisher>().Object);
        var command = new UpdateSubscriptionCommand(1, TestUserId, "Spotify", 49m, "EUR", 90, SelectedMonth: 9, SelectedDay: 10);

        await handler.Handle(command, CancellationToken.None);

        subscription.Name.Should().Be("Spotify");
        subscription.Price.Should().Be(49m);
        subscription.Currency.Should().Be("EUR");
        subscription.RenewalPeriodDays.Should().Be(90);
        subscription.RenewalDate.Should().Be(new DateTime(2026, 9, 10));
    }

    [Fact]
    public async Task Handle_WhenSelectedDateIsInPast_ShouldSetNextYearDate()
    {
        var subscription = CreateSubscription();

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var handler = new UpdateSubscriptionHandler(repoMock.Object, CreateClockMock().Object, new Mock<IPublisher>().Object);
        var command = new UpdateSubscriptionCommand(1, TestUserId, "Netflix", 99m, "TRY", 30, SelectedMonth: 3, SelectedDay: 1);

        await handler.Handle(command, CancellationToken.None);

        subscription.RenewalDate.Should().Be(new DateTime(2027, 3, 1));
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryUpdateAsync()
    {
        var subscription = CreateSubscription();

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var handler = new UpdateSubscriptionHandler(repoMock.Object, CreateClockMock().Object, new Mock<IPublisher>().Object);
        var command = new UpdateSubscriptionCommand(1, TestUserId, "Netflix", 99m, "TRY", 30, SelectedMonth: 9, SelectedDay: 1);

        await handler.Handle(command, CancellationToken.None);

        repoMock.Verify(r => r.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPublishUpdatedEvent()
    {
        var subscription = CreateSubscription();

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        INotification? published = null;
        var publisherMock = new Mock<IPublisher>();
        publisherMock
            .Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Callback<INotification, CancellationToken>((n, _) => published = n)
            .Returns(Task.CompletedTask);

        var handler = new UpdateSubscriptionHandler(repoMock.Object, CreateClockMock().Object, publisherMock.Object);
        var command = new UpdateSubscriptionCommand(1, TestUserId, "Netflix", 99m, "TRY", 30, SelectedMonth: 9, SelectedDay: 1);

        await handler.Handle(command, CancellationToken.None);

        published.Should().BeOfType<SubscriptionUpdatedEvent>();
        ((SubscriptionUpdatedEvent)published!).UserId.Should().Be(TestUserId);
    }

    [Fact]
    public async Task Handle_WhenSubscriptionNotFound_ShouldThrowKeyNotFoundException()
    {
        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(99, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var handler = new UpdateSubscriptionHandler(repoMock.Object, CreateClockMock().Object, new Mock<IPublisher>().Object);
        var command = new UpdateSubscriptionCommand(99, TestUserId, "Netflix", 99m, "TRY", 30, SelectedMonth: 9, SelectedDay: 1);

        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Subscription not found*");
    }

    [Fact]
    public async Task Handle_ShouldUpdateCategory()
    {
        var subscription = CreateSubscription();

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var handler = new UpdateSubscriptionHandler(repoMock.Object, CreateClockMock().Object, new Mock<IPublisher>().Object);
        var command = new UpdateSubscriptionCommand(1, TestUserId, "Netflix", 99m, "TRY", 30, 9, 1, SubscriptionCategory.Entertainment);

        await handler.Handle(command, CancellationToken.None);

        subscription.Category.Should().Be(SubscriptionCategory.Entertainment);
    }
}
