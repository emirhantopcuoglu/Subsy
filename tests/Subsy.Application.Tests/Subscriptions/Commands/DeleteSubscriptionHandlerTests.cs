using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Subsy.Application.Common.Events;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.Subscriptions.Commands.DeleteSubscription;
using Subsy.Domain.Entities;

namespace Subsy.Application.Tests.Subscriptions.Commands;

public class DeleteSubscriptionHandlerTests
{
    private const string TestUserId = "user-123";
    private const string OtherUserId = "user-999";

    private static Subscription CreateSubscription(string userId = TestUserId)
        => Subscription.Create(userId, "Netflix", 99m, "TRY", 30, new DateTime(2026, 8, 1));

    private static DeleteSubscriptionHandler BuildHandler(ISubscriptionRepository repo, IPublisher publisher)
        => new(repo, publisher, NullLogger<DeleteSubscriptionHandler>.Instance);

    [Fact]
    public async Task Handle_WhenSubscriptionExists_ShouldDeleteFromRepository()
    {
        var subscription = CreateSubscription();

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var handler = BuildHandler(repoMock.Object, new Mock<IPublisher>().Object);

        await handler.Handle(new DeleteSubscriptionCommand(1, TestUserId), CancellationToken.None);

        repoMock.Verify(r => r.DeleteAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSubscriptionExists_ShouldPublishDeletedEvent()
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

        var handler = BuildHandler(repoMock.Object, publisherMock.Object);

        await handler.Handle(new DeleteSubscriptionCommand(1, TestUserId), CancellationToken.None);

        published.Should().BeOfType<SubscriptionDeletedEvent>();
        var evt = (SubscriptionDeletedEvent)published!;
        evt.UserId.Should().Be(TestUserId);
        evt.SubscriptionName.Should().Be("Netflix");
    }

    [Fact]
    public async Task Handle_WhenSubscriptionNotFound_ShouldThrowKeyNotFoundException()
    {
        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(99, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var handler = BuildHandler(repoMock.Object, new Mock<IPublisher>().Object);

        Func<Task> act = () => handler.Handle(new DeleteSubscriptionCommand(99, TestUserId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        repoMock.Verify(r => r.DeleteAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CanDeleteArchivedSubscription()
    {
        var subscription = CreateSubscription();
        subscription.Archive();

        var repoMock = new Mock<ISubscriptionRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var handler = BuildHandler(repoMock.Object, new Mock<IPublisher>().Object);

        await handler.Handle(new DeleteSubscriptionCommand(1, TestUserId), CancellationToken.None);

        repoMock.Verify(r => r.DeleteAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
    }
}
