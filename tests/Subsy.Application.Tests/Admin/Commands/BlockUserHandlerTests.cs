using FluentAssertions;
using Moq;
using Subsy.Application.Admin.Commands.BlockUser;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Tests.Admin.Commands;

public class BlockUserHandlerTests
{
    private const string TargetUserId = "user-target";
    private const string RequestingUserId = "user-admin";

    [Fact]
    public async Task Handle_ShouldCallBlockUserAsyncWithCorrectIds()
    {
        var adminServiceMock = new Mock<IAdminService>();
        var handler = new BlockUserHandler(adminServiceMock.Object);

        await handler.Handle(new BlockUserCommand(TargetUserId, RequestingUserId), CancellationToken.None);

        adminServiceMock.Verify(
            s => s.BlockUserAsync(TargetUserId, RequestingUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSelfBlock_ShouldPropagateInvalidOperationException()
    {
        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock
            .Setup(s => s.BlockUserAsync(RequestingUserId, RequestingUserId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Kendinizi bloklayamazsınız."));

        var handler = new BlockUserHandler(adminServiceMock.Object);

        Func<Task> act = () => handler.Handle(new BlockUserCommand(RequestingUserId, RequestingUserId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*bloklayamazsınız*");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldPropagateInvalidOperationException()
    {
        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock
            .Setup(s => s.BlockUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User not found."));

        var handler = new BlockUserHandler(adminServiceMock.Object);

        Func<Task> act = () => handler.Handle(new BlockUserCommand(TargetUserId, RequestingUserId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
