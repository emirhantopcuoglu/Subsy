using Subsy.Domain.Entities;

namespace Subsy.Application.Common.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct = default);
    Task<List<AuditLog>> GetRecentByUserIdAsync(string userId, int count = 10, CancellationToken ct = default);
    Task<(List<AuditLog> Items, int TotalCount)> GetPagedAsync(string? search, int skip, int take, CancellationToken ct = default);
}
