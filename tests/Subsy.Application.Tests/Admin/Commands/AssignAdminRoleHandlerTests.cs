using FluentAssertions;
using Moq;
using Subsy.Application.Admin.Commands.AssignAdminRole;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Tests.Admin.Commands;

public class AssignAdminRoleHandlerTests
{
    private const string TargetUserId = "user-target";
    private const string RequestingUserId = "user-admin";

    [Fact]
    public async Task Handle_ShouldCallAssignAdminRoleAsyncWithCorrectIds()
    {
        var adminServiceMock = new Mock<IAdminService>();
        var handler = new AssignAdminRoleHandler(adminServiceMock.Object);
        var command = new AssignAdminRoleCommand(TargetUserId, RequestingUserId);

        await handler.Handle(command, CancellationToken.None);

        adminServiceMock.Verify(
            s => s.AssignAdminRoleAsync(TargetUserId, RequestingUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenServiceThrows_ShouldPropagateException()
    {
        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock
            .Setup(s => s.AssignAdminRoleAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User not found."));

        var handler = new AssignAdminRoleHandler(adminServiceMock.Object);

        Func<Task> act = () => handler.Handle(new AssignAdminRoleCommand(TargetUserId, RequestingUserId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }
}
