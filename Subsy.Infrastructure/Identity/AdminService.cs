using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Subsy.Application.Admin.Queries.GetAdminStats;
using Subsy.Application.Admin.Queries.GetAllUsers;
using Subsy.Application.Common;
using Subsy.Application.Common.Interfaces;
using Subsy.Domain.Entities;
using Subsy.Infrastructure.Persistence;

namespace Subsy.Infrastructure.Identity;

public sealed class AdminService : IAdminService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SubsyContext _db;
    private readonly IDateTimeProvider _dateTime;
    private readonly IAuditLogRepository _auditLog;

    public AdminService(
        UserManager<IdentityUser> userManager,
        SubsyContext db,
        IDateTimeProvider dateTime,
        IAuditLogRepository auditLog)
    {
        _userManager = userManager;
        _db = db;
        _dateTime = dateTime;
        _auditLog = auditLog;
    }

    public async Task<AdminStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var totalUsers = await _userManager.Users.CountAsync(ct);
        var adminUsers = (await _userManager.GetUsersInRoleAsync(Roles.Admin)).Count;
        var totalSubscriptions = await _db.Subscriptions.CountAsync(ct);
        var activeSubscriptions = await _db.Subscriptions.CountAsync(s => !s.IsArchived, ct);
        var cutoff = _dateTime.UtcNow.AddDays(-30);
        var newUsersLast30Days = await _db.UserProfiles.CountAsync(p => p.RegisteredAt >= cutoff, ct);

        return new AdminStatsDto(totalUsers, adminUsers, totalSubscriptions, activeSubscriptions, newUsersLast30Days);
    }

    public async Task<IReadOnlyList<AdminUserDto>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var users = await _userManager.Users
            .OrderBy(u => u.UserName)
            .ToListAsync(ct);

        var subscriptionCounts = await _db.Subscriptions
            .GroupBy(s => s.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count, ct);

        var adminRoleUsers = (await _userManager.GetUsersInRoleAsync(Roles.Admin))
            .Select(u => u.Id)
            .ToHashSet();

        return users.Select(u => new AdminUserDto(
            u.Id,
            u.UserName ?? string.Empty,
            u.Email ?? string.Empty,
            adminRoleUsers.Contains(u.Id),
            u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow,
            subscriptionCounts.GetValueOrDefault(u.Id, 0)
        )).ToList();
    }

    public async Task AssignAdminRoleAsync(string targetUserId, CancellationToken ct = default)
    {
        var user = await FindOrThrowAsync(targetUserId);

        if (!await _userManager.IsInRoleAsync(user, Roles.Admin))
            await _userManager.AddToRoleAsync(user, Roles.Admin);
    }

    public async Task RevokeAdminRoleAsync(string targetUserId, string requestingUserId, CancellationToken ct = default)
    {
        if (targetUserId == requestingUserId)
            throw new InvalidOperationException("Kendi admin rolünüzü kaldıramazsınız.");

        var adminCount = (await _userManager.GetUsersInRoleAsync(Roles.Admin)).Count;
        if (adminCount <= 1)
            throw new InvalidOperationException("Sistemde en az bir admin kalmalıdır.");

        var user = await FindOrThrowAsync(targetUserId);

        if (await _userManager.IsInRoleAsync(user, Roles.Admin))
            await _userManager.RemoveFromRoleAsync(user, Roles.Admin);

        await WriteAuditAsync(requestingUserId, "AdminRoleRevoked", user, ct);
    }

    public async Task BlockUserAsync(string targetUserId, string requestingUserId, CancellationToken ct = default)
    {
        if (targetUserId == requestingUserId)
            throw new InvalidOperationException("Kendinizi bloklayamazsınız.");

        var user = await FindOrThrowAsync(targetUserId);

        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

        await WriteAuditAsync(requestingUserId, "UserBlocked", user, ct);
    }

    public async Task UnblockUserAsync(string targetUserId, CancellationToken ct = default)
    {
        var user = await FindOrThrowAsync(targetUserId);

        await _userManager.SetLockoutEndDateAsync(user, null);

        await WriteAuditAsync(targetUserId, "UserUnblocked", user, ct);
    }

    public async Task DeleteUserAsync(string targetUserId, string requestingUserId, CancellationToken ct = default)
    {
        if (targetUserId == requestingUserId)
            throw new InvalidOperationException("Kendi hesabınızı silemezsiniz.");

        var user = await FindOrThrowAsync(targetUserId);

        // Cascade: EF Core relationships without configured cascade delete require manual cleanup.
        await _db.Subscriptions.Where(s => s.UserId == targetUserId).ExecuteDeleteAsync(ct);
        await _db.UserProfiles.Where(p => p.UserId == targetUserId).ExecuteDeleteAsync(ct);
        await _db.AuditLogs.Where(a => a.UserId == targetUserId).ExecuteDeleteAsync(ct);

        await _userManager.DeleteAsync(user);

        await WriteAuditAsync(requestingUserId, "UserDeleted",
            $"Deleted user: {user.UserName} ({user.Email})", ct);
    }

    private async Task<IdentityUser> FindOrThrowAsync(string userId)
        => await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");

    private Task WriteAuditAsync(string actorUserId, string action, IdentityUser target, CancellationToken ct)
        => WriteAuditAsync(actorUserId, action, $"Target: {target.UserName} ({target.Id})", ct);

    private Task WriteAuditAsync(string actorUserId, string action, string details, CancellationToken ct)
        => _auditLog.AddAsync(
            AuditLog.Create(actorUserId, action, "User", 0, details, _dateTime.UtcNow),
            ct);
}
