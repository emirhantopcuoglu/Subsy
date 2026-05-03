using FluentAssertions;
using Moq;
using Subsy.Application.Admin.Queries.GetAdminStats;
using Subsy.Application.Admin.Queries.GetAllSubscriptionsAdmin;
using Subsy.Application.Admin.Queries.GetAllUsers;
using Subsy.Application.Admin.Queries.GetAuditLogs;
using Subsy.Application.Admin.Queries.GetUserDetail;
using Subsy.Application.Common;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Tests.Admin.Queries;

public class AdminQueryHandlerTests
{
    // ── GetAdminStats ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAdminStats_ShouldDelegateToAdminServiceAndReturnResult()
    {
        var expected = new AdminStatsDto(10, 2, 50, 45, 3);

        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock.Setup(s => s.GetStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new GetAdminStatsHandler(adminServiceMock.Object);

        var result = await handler.Handle(new GetAdminStatsQuery(), CancellationToken.None);

        result.Should().Be(expected);
        adminServiceMock.Verify(s => s.GetStatsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetAllUsers ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllUsers_ShouldDelegateToAdminServiceAndReturnUserList()
    {
        var users = new List<AdminUserDto>
        {
            new("u1", "alice", "alice@example.com", true, false, 3),
            new("u2", "bob", "bob@example.com", false, true, 0)
        };

        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock.Setup(s => s.GetAllUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var handler = new GetAllUsersHandler(adminServiceMock.Object);

        var result = await handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].UserName.Should().Be("alice");
        result[1].IsBlocked.Should().BeTrue();
        adminServiceMock.Verify(s => s.GetAllUsersAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllUsers_WhenNoUsers_ShouldReturnEmptyList()
    {
        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock.Setup(s => s.GetAllUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AdminUserDto>());

        var handler = new GetAllUsersHandler(adminServiceMock.Object);

        var result = await handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    // ── GetUserDetail ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserDetail_ShouldDelegateToAdminServiceAndReturnDetail()
    {
        var detail = new AdminUserDetailDto(
            "u1", "alice", "alice@example.com",
            IsAdmin: true, IsBlocked: false, EmailConfirmed: true, TwoFactorEnabled: false,
            RegisteredAt: new DateTime(2025, 1, 1),
            Subscriptions: [],
            RecentAuditLogs: []);

        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock.Setup(s => s.GetUserDetailAsync("u1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        var handler = new GetUserDetailHandler(adminServiceMock.Object);

        var result = await handler.Handle(new GetUserDetailQuery("u1"), CancellationToken.None);

        result.Should().Be(detail);
        adminServiceMock.Verify(s => s.GetUserDetailAsync("u1", It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetAllSubscriptionsAdmin ──────────────────────────────────────────────

    [Fact]
    public async Task GetAllSubscriptionsAdmin_ShouldDelegateToAdminServiceAndReturnList()
    {
        var subs = new List<AdminSubscriptionDto>
        {
            new(1, "u1", "alice", "alice@example.com", "Netflix", 99m, "TRY", 30, new DateTime(2026, 8, 1), false, DateTime.UtcNow)
        };

        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock.Setup(s => s.GetAllSubscriptionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(subs);

        var handler = new GetAllSubscriptionsAdminHandler(adminServiceMock.Object);

        var result = await handler.Handle(new GetAllSubscriptionsAdminQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Netflix");
        adminServiceMock.Verify(s => s.GetAllSubscriptionsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetAuditLogs ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAuditLogs_ShouldPassSearchAndPaginationToService()
    {
        var paged = new PagedResult<AuditLogDto>(
            Items: [new(1, "u1", "alice", "UserBlocked", "User", "Target: bob", DateTime.UtcNow)],
            TotalCount: 1, Page: 2, PageSize: 25);

        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock.Setup(s => s.GetAuditLogsAsync("alice", 2, 25, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var handler = new GetAuditLogsHandler(adminServiceMock.Object);

        var result = await handler.Handle(new GetAuditLogsQuery("alice", 2, 25), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(2);
        result.Items.Should().HaveCount(1);
        adminServiceMock.Verify(s => s.GetAuditLogsAsync("alice", 2, 25, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAuditLogs_WhenNoResults_ShouldReturnEmptyPagedResult()
    {
        var empty = new PagedResult<AuditLogDto>([], 0, 1, 50);

        var adminServiceMock = new Mock<IAdminService>();
        adminServiceMock.Setup(s => s.GetAuditLogsAsync(null, 1, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(empty);

        var handler = new GetAuditLogsHandler(adminServiceMock.Object);

        var result = await handler.Handle(new GetAuditLogsQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }
}
