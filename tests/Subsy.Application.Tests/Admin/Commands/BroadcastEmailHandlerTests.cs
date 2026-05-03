using FluentAssertions;
using Moq;
using Subsy.Application.Admin.Commands.BroadcastEmail;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Tests.Admin.Commands;

public class BroadcastEmailHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallBroadcastEmailAsyncAndReturnSentCount()
    {
        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock
            .Setup(s => s.BroadcastEmailAsync("Subject", "Body", It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        var handler = new BroadcastEmailHandler(adminServiceMock.Object);

        var result = await handler.Handle(new BroadcastEmailCommand("Subject", "Body"), CancellationToken.None);

        result.Should().Be(42);
        adminServiceMock.Verify(
            s => s.BroadcastEmailAsync("Subject", "Body", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoUsersEligible_ShouldReturnZero()
    {
        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock
            .Setup(s => s.BroadcastEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var handler = new BroadcastEmailHandler(adminServiceMock.Object);

        var result = await handler.Handle(new BroadcastEmailCommand("Test", "Body"), CancellationToken.None);

        result.Should().Be(0);
    }
}
