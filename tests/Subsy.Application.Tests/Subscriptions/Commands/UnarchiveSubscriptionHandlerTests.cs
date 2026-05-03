using FluentAssertions;
using MediatR;
using Moq;
using Subsy.Application.Common.Events;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Commands.UnarchiveSubscription;
using Subsy.Domain.Entities;

namespace Subsy.Application.Tests.Subscriptions.Commands;

public class UnarchiveSubscriptionHandlerTests
{
    private const string TestUserId = "user-123";

    private static Subscription CreateArchived()
    {
        var sub = Subscription.Create(TestUserId, "Spotify", 49m, "TRY", 30, new DateTime(2026, 8, 1));
        sub.Archive();
        return sub;
    }

    [Fact]
    public async Task Handle_WhenSubscriptionIsArchived_ShouldUnarchiveAndPersist()
    {
        var subscription = CreateArchived();

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var handler = new UnarchiveSubscriptionHandler(repoMock.Object, new Mock<IPublisher>().Object);

        await handler.Handle(new UnarchiveSubscriptionCommand(1, TestUserId), CancellationToken.None);

        subscription.IsArchived.Should().BeFalse();
        repoMock.Verify(r => r.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSubscriptionIsArchived_ShouldPublishUnarchivedEvent()
    {
        var subscription = CreateArchived();

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        INotification? published = null;
        var publisherMock = new Mock<IPublisher>();
        publisherMock
            .Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Callback<INotification, CancellationToken>((n, _) => published = n)
            .Returns(Task.CompletedTask);

        var handler = new UnarchiveSubscriptionHandler(repoMock.Object, publisherMock.Object);

        await handler.Handle(new UnarchiveSubscriptionCommand(1, TestUserId), CancellationToken.None);

        published.Should().BeOfType<SubscriptionUnarchivedEvent>();
        var evt = (SubscriptionUnarchivedEvent)published!;
        evt.UserId.Should().Be(TestUserId);
        evt.SubscriptionName.Should().Be("Spotify");
    }

    [Fact]
    public async Task Handle_WhenSubscriptionNotFound_ShouldThrowKeyNotFoundException()
    {
        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(99, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var handler = new UnarchiveSubscriptionHandler(repoMock.Object, new Mock<IPublisher>().Object);

        Func<Task> act = () => handler.Handle(new UnarchiveSubscriptionCommand(99, TestUserId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenSubscriptionIsNotArchived_ShouldThrowInvalidOperationException()
    {
        var active = Subscription.Create(TestUserId, "Spotify", 49m, "TRY", 30, new DateTime(2026, 8, 1));

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(active);

        var handler = new UnarchiveSubscriptionHandler(repoMock.Object, new Mock<IPublisher>().Object);

        Func<Task> act = () => handler.Handle(new UnarchiveSubscriptionCommand(1, TestUserId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
