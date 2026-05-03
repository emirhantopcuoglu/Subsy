using FluentAssertions;
using Moq;
using Subsy.Application.Admin.Commands.ForceLogout;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Tests.Admin.Commands;

public class ForceLogoutHandlerTests
{
    private const string TargetUserId = "user-target";
    private const string RequestingUserId = "user-admin";

    [Fact]
    public async Task Handle_ShouldCallForceLogoutAsyncWithCorrectIds()
    {
        var adminServiceMock = new Mock<IAdminService>();
        var handler = new ForceLogoutHandler(adminServiceMock.Object);

        await handler.Handle(new ForceLogoutCommand(TargetUserId, RequestingUserId), CancellationToken.None);

        adminServiceMock.Verify(
            s => s.ForceLogoutAsync(TargetUserId, RequestingUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSelfForceLogout_ShouldPropagateInvalidOperationException()
    {
        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock
            .Setup(s => s.ForceLogoutAsync(RequestingUserId, RequestingUserId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Kendi oturumunuzu bu şekilde sonlandıramazsınız."));

        var handler = new ForceLogoutHandler(adminServiceMock.Object);

        Func<Task> act = () => handler.Handle(new ForceLogoutCommand(RequestingUserId, RequestingUserId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*sonlandıramazsınız*");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldPropagateInvalidOperationException()
    {
        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock
            .Setup(s => s.ForceLogoutAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User not found."));

        var handler = new ForceLogoutHandler(adminServiceMock.Object);

        Func<Task> act = () => handler.Handle(new ForceLogoutCommand(TargetUserId, RequestingUserId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
