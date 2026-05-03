using FluentAssertions;
using Moq;
using Subsy.Application.Admin.Commands.RevokeAdminRole;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Tests.Admin.Commands;

public class RevokeAdminRoleHandlerTests
{
    private const string TargetUserId = "user-target";
    private const string RequestingUserId = "user-admin";

    [Fact]
    public async Task Handle_ShouldCallRevokeAdminRoleAsyncWithCorrectIds()
    {
        var adminServiceMock = new Mock<IAdminService>();
        var handler = new RevokeAdminRoleHandler(adminServiceMock.Object);

        await handler.Handle(new RevokeAdminRoleCommand(TargetUserId, RequestingUserId), CancellationToken.None);

        adminServiceMock.Verify(
            s => s.RevokeAdminRoleAsync(TargetUserId, RequestingUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSelfRevoke_ShouldPropagateInvalidOperationException()
    {
        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock
            .Setup(s => s.RevokeAdminRoleAsync(RequestingUserId, RequestingUserId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Kendi admin rolünüzü kaldıramazsınız."));

        var handler = new RevokeAdminRoleHandler(adminServiceMock.Object);

        Func<Task> act = () => handler.Handle(new RevokeAdminRoleCommand(RequestingUserId, RequestingUserId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*kaldıramazsınız*");
    }

    [Fact]
    public async Task Handle_WhenLastAdmin_ShouldPropagateInvalidOperationException()
    {
        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock
            .Setup(s => s.RevokeAdminRoleAsync(TargetUserId, RequestingUserId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Sistemde en az bir admin kalmalıdır."));

        var handler = new RevokeAdminRoleHandler(adminServiceMock.Object);

        Func<Task> act = () => handler.Handle(new RevokeAdminRoleCommand(TargetUserId, RequestingUserId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*en az bir admin*");
    }
}
