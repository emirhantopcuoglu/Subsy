using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Subsy.Application.Admin.Queries.GetAdminStats;
using Subsy.Application.Admin.Queries.GetAllSubscriptionsAdmin;
using Subsy.Application.Admin.Queries.GetAllUsers;
using Subsy.Application.Admin.Queries.GetAuditLogs;
using Subsy.Application.Admin.Queries.GetUserDetail;
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
    private readonly IEmailService _emailService;

    public AdminService(
        UserManager<IdentityUser> userManager,
        SubsyContext db,
        IDateTimeProvider dateTime,
        IAuditLogRepository auditLog,
        IEmailService emailService)
    {
        _userManager = userManager;
        _db = db;
        _dateTime = dateTime;
        _auditLog = auditLog;
        _emailService = emailService;
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

    public async Task<AdminUserDetailDto> GetUserDetailAsync(string userId, CancellationToken ct = default)
    {
        var user = await FindOrThrowAsync(userId);
        var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Admin);
        var isBlocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;

        var profile = await _db.UserProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        var subscriptions = await _db.Subscriptions
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.RenewalDate)
            .AsNoTracking()
            .ToListAsync(ct);

        var auditLogs = await _db.AuditLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .Take(20)
            .AsNoTracking()
            .ToListAsync(ct);

        return new AdminUserDetailDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            isAdmin,
            isBlocked,
            user.EmailConfirmed,
            user.TwoFactorEnabled,
            profile?.RegisteredAt,
            subscriptions.Select(s => new AdminSubscriptionItem(
                s.Id, s.Name, s.Price, s.Currency, s.RenewalDate, s.IsArchived
            )).ToList(),
            auditLogs.Select(l => new AdminAuditLogItem(
                l.Action, l.EntityName, l.Details, l.CreatedAt
            )).ToList());
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var skip = (page - 1) * pageSize;
        var (items, total) = await _auditLog.GetPagedAsync(search, skip, pageSize, ct);

        var userIds = items.Select(l => l.UserId).Distinct().ToList();
        var users = await _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Id, ct);

        var dtos = items.Select(l => new AuditLogDto(
            l.Id,
            l.UserId,
            users.GetValueOrDefault(l.UserId, l.UserId),
            l.Action,
            l.EntityName,
            l.Details,
            l.CreatedAt)).ToList();

        return new PagedResult<AuditLogDto>(dtos, total, page, pageSize);
    }

    public async Task<IReadOnlyList<AdminSubscriptionDto>> GetAllSubscriptionsAsync(CancellationToken ct = default)
    {
        var subscriptions = await _db.Subscriptions
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);

        var userIds = subscriptions.Select(s => s.UserId).Distinct().ToList();
        var users = await _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync(ct);

        var userMap = users.ToDictionary(u => u.Id, u => (UserName: u.UserName ?? u.Id, Email: u.Email ?? string.Empty));

        return subscriptions.Select(s =>
        {
            var (userName, email) = userMap.GetValueOrDefault(s.UserId, (s.UserId, string.Empty));
            return new AdminSubscriptionDto(
                s.Id, s.UserId, userName, email,
                s.Name, s.Price, s.Currency, s.RenewalPeriodDays, s.RenewalDate, s.IsArchived, s.CreatedAt);
        }).ToList();
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

    public async Task ForceLogoutAsync(string targetUserId, string requestingUserId, CancellationToken ct = default)
    {
        if (targetUserId == requestingUserId)
            throw new InvalidOperationException("Kendi oturumunuzu bu şekilde sonlandıramazsınız.");

        var user = await FindOrThrowAsync(targetUserId);
        await _userManager.UpdateSecurityStampAsync(user);

        await WriteAuditAsync(requestingUserId, "ForceLogout", user, ct);
    }

    public async Task SendPasswordResetEmailAsync(string targetUserId, string callbackUrl, CancellationToken ct = default)
    {
        var user = await FindOrThrowAsync(targetUserId);
        if (user.Email is null) return;

        var body = $"""
            <h2>Merhaba {user.UserName},</h2>
            <p>Yönetici tarafından parola sıfırlama isteği oluşturuldu.</p>
            <p>Aşağıdaki bağlantıya tıklayarak parolanızı sıfırlayabilirsiniz:</p>
            <p><a href="{callbackUrl}" style="background:#5865f2;color:#fff;padding:10px 20px;border-radius:6px;text-decoration:none;display:inline-block;">Parolayı Sıfırla</a></p>
            <p style="color:#888;font-size:12px;">Bu bağlantı 24 saat geçerlidir. Eğer bu isteği siz yapmadıysanız görmezden gelebilirsiniz.</p>
            """;

        await _emailService.SendAsync(user.Email, "Subsy — Parola Sıfırlama", body, ct);
    }

    public async Task<int> BroadcastEmailAsync(string subject, string body, CancellationToken ct = default)
    {
        var users = await _userManager.Users
            .Where(u => u.EmailConfirmed && u.Email != null)
            .ToListAsync(ct);

        var blockedIds = users
            .Where(u => u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow)
            .Select(u => u.Id)
            .ToHashSet();

        var targets = users.Where(u => !blockedIds.Contains(u.Id)).ToList();
        var sent = 0;

        foreach (var user in targets)
        {
            try
            {
                await _emailService.SendAsync(user.Email!, subject, body, ct);
                sent++;
            }
            catch { /* per-user send failure should not abort the batch */ }
        }

        return sent;
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
