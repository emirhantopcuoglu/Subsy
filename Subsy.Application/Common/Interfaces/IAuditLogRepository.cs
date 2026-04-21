using Subsy.Domain.Entities;

namespace Subsy.Application.Common.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct = default);
    Task<List<AuditLog>> GetRecentByUserIdAsync(string userId, int count = 10, CancellationToken ct = default);
}