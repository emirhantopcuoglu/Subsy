using Microsoft.EntityFrameworkCore;
using Subsy.Application.Common.Interfaces;
using Subsy.Domain.Entities;
using Subsy.Infrastructure.Persistence;

namespace Subsy.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly SubsyContext _context;

    public AuditLogRepository(SubsyContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog log, CancellationToken ct = default)
    {
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<AuditLog>> GetRecentByUserIdAsync(
        string userId, int count = 10, CancellationToken ct = default)
    {
        return await _context.AuditLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<(List<AuditLog> Items, int TotalCount)> GetPagedAsync(
        string? search, int skip, int take, CancellationToken ct = default)
    {
        var query = _context.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(l =>
                l.Action.Contains(search) ||
                l.EntityName.Contains(search) ||
                (l.Details != null && l.Details.Contains(search)) ||
                l.UserId.Contains(search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }
}
