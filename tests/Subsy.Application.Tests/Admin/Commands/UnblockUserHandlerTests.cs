using FluentAssertions;
using Moq;
using Subsy.Application.Admin.Commands.UnblockUser;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Tests.Admin.Commands;

public class UnblockUserHandlerTests
{
    private const string TargetUserId = "user-target";
    private const string RequestingUserId = "user-admin";

    [Fact]
    public async Task Handle_ShouldCallUnblockUserAsyncWithCorrectIds()
    {
        var adminServiceMock = new Mock<IAdminService>();
        var handler = new UnblockUserHandler(adminServiceMock.Object);

        await handler.Handle(new UnblockUserCommand(TargetUserId, RequestingUserId), CancellationToken.None);

        adminServiceMock.Verify(
            s => s.UnblockUserAsync(TargetUserId, RequestingUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldPropagateInvalidOperationException()
    {
        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock
            .Setup(s => s.UnblockUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User not found."));

        var handler = new UnblockUserHandler(adminServiceMock.Object);

        Func<Task> act = () => handler.Handle(new UnblockUserCommand(TargetUserId, RequestingUserId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
