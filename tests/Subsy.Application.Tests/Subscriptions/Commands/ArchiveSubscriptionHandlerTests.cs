using FluentAssertions;
using MediatR;
using Moq;
using Subsy.Application.Common.Events;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Commands.ArchiveSubscription;
using Subsy.Domain.Entities;

namespace Subsy.Application.Tests.Subscriptions.Commands;

public class ArchiveSubscriptionHandlerTests
{
    private const string TestUserId = "user-123";

    private static Subscription CreateActive()
        => Subscription.Create(TestUserId, "Netflix", 99m, "TRY", 30, new DateTime(2026, 8, 1));

    private static Subscription CreateAlreadyArchived()
    {
        var sub = CreateActive();
        sub.Archive();
        return sub;
    }

    [Fact]
    public async Task Handle_WhenSubscriptionExists_ShouldArchiveAndPersist()
    {
        var subscription = CreateActive();

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var publisherMock = new Mock<IPublisher>();
        var handler = new ArchiveSubscriptionHandler(repoMock.Object, publisherMock.Object);

        await handler.Handle(new ArchiveSubscriptionCommand(1, TestUserId), CancellationToken.None);

        subscription.IsArchived.Should().BeTrue();
        repoMock.Verify(r => r.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSubscriptionExists_ShouldPublishArchivedEvent()
    {
        var subscription = CreateActive();

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        INotification? published = null;
        var publisherMock = new Mock<IPublisher>();
        publisherMock
            .Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Callback<INotification, CancellationToken>((n, _) => published = n)
            .Returns(Task.CompletedTask);

        var handler = new ArchiveSubscriptionHandler(repoMock.Object, publisherMock.Object);

        await handler.Handle(new ArchiveSubscriptionCommand(1, TestUserId), CancellationToken.None);

        published.Should().BeOfType<SubscriptionArchivedEvent>();
        var evt = (SubscriptionArchivedEvent)published!;
        evt.UserId.Should().Be(TestUserId);
        evt.SubscriptionName.Should().Be("Netflix");
    }

    [Fact]
    public async Task Handle_WhenSubscriptionNotFound_ShouldThrowKeyNotFoundException()
    {
        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(99, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var handler = new ArchiveSubscriptionHandler(repoMock.Object, new Mock<IPublisher>().Object);

        Func<Task> act = () => handler.Handle(new ArchiveSubscriptionCommand(99, TestUserId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenAlreadyArchived_ShouldThrowInvalidOperationException()
    {
        var subscription = CreateAlreadyArchived();

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var handler = new ArchiveSubscriptionHandler(repoMock.Object, new Mock<IPublisher>().Object);

        Func<Task> act = () => handler.Handle(new ArchiveSubscriptionCommand(1, TestUserId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
