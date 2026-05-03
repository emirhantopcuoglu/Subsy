using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Subsy.Application.Admin.Queries.GetAdminStats;
using Subsy.Application.Admin.Queries.GetAllUsers;
using Subsy.Application.Common;
using Subsy.Application.Common.Interfaces;
using Subsy.Infrastructure.Persistence;

namespace Subsy.Infrastructure.Identity;

public sealed class AdminService : IAdminService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SubsyContext _db;
    private readonly IDateTimeProvider _dateTime;

    public AdminService(UserManager<IdentityUser> userManager, SubsyContext db, IDateTimeProvider dateTime)
    {
        _userManager = userManager;
        _db = db;
        _dateTime = dateTime;
    }

    public async Task<AdminStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var totalUsers = await _userManager.Users.CountAsync(ct);

        var adminUsers = (await _userManager.GetUsersInRoleAsync(Roles.Admin)).Count;

        var totalSubscriptions = await _db.Subscriptions.CountAsync(ct);

        var activeSubscriptions = await _db.Subscriptions
            .CountAsync(s => !s.IsArchived, ct);

        var cutoff = _dateTime.UtcNow.AddDays(-30);
        var newUsersLast30Days = await _db.UserProfiles
            .CountAsync(p => p.RegisteredAt >= cutoff, ct);

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
            subscriptionCounts.GetValueOrDefault(u.Id, 0)
        )).ToList();
    }

    public async Task AssignAdminRoleAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");

        if (!await _userManager.IsInRoleAsync(user, Roles.Admin))
            await _userManager.AddToRoleAsync(user, Roles.Admin);
    }

    public async Task RevokeAdminRoleAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");

        if (await _userManager.IsInRoleAsync(user, Roles.Admin))
            await _userManager.RemoveFromRoleAsync(user, Roles.Admin);
    }
}
