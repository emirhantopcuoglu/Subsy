using Moq;
using Subsy.Application.Admin.Commands.SendPasswordResetEmail;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Tests.Admin.Commands;

public class SendPasswordResetEmailHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallSendPasswordResetEmailAsyncWithAllParams()
    {
        var adminServiceMock = new Mock<IAdminService>();
        var handler = new SendPasswordResetEmailHandler(adminServiceMock.Object);

        var command = new SendPasswordResetEmailCommand(
            UserId: "user-1",
            UserName: "johndoe",
            Email: "john@example.com",
            CallbackUrl: "https://example.com/reset?token=abc");

        await handler.Handle(command, CancellationToken.None);

        adminServiceMock.Verify(
            s => s.SendPasswordResetEmailAsync(
                "user-1",
                "johndoe",
                "john@example.com",
                "https://example.com/reset?token=abc",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
