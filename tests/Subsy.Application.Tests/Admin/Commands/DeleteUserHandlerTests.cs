using FluentAssertions;
using Moq;
using Subsy.Application.Admin.Commands.DeleteUser;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Tests.Admin.Commands;

public class DeleteUserHandlerTests
{
    private const string TargetUserId = "user-target";
    private const string RequestingUserId = "user-admin";

    [Fact]
    public async Task Handle_ShouldCallDeleteUserAsyncWithCorrectIds()
    {
        var adminServiceMock = new Mock<IAdminService>();
        var handler = new DeleteUserHandler(adminServiceMock.Object);

        await handler.Handle(new DeleteUserCommand(TargetUserId, RequestingUserId), CancellationToken.None);

        adminServiceMock.Verify(
            s => s.DeleteUserAsync(TargetUserId, RequestingUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSelfDelete_ShouldPropagateInvalidOperationException()
    {
        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock
            .Setup(s => s.DeleteUserAsync(RequestingUserId, RequestingUserId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Kendi hesabınızı silemezsiniz."));

        var handler = new DeleteUserHandler(adminServiceMock.Object);

        Func<Task> act = () => handler.Handle(new DeleteUserCommand(RequestingUserId, RequestingUserId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*silemezsiniz*");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldPropagateInvalidOperationException()
    {
        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock
            .Setup(s => s.DeleteUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User not found."));

        var handler = new DeleteUserHandler(adminServiceMock.Object);

        Func<Task> act = () => handler.Handle(new DeleteUserCommand(TargetUserId, RequestingUserId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
