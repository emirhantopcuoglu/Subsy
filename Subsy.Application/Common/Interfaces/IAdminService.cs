using Subsy.Application.Admin.Queries.GetAdminStats;
using Subsy.Application.Admin.Queries.GetAllUsers;

namespace Subsy.Application.Common.Interfaces;

public interface IAdminService
{
    Task<AdminStatsDto> GetStatsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AdminUserDto>> GetAllUsersAsync(CancellationToken ct = default);
    Task AssignAdminRoleAsync(string userId, CancellationToken ct = default);
    Task RevokeAdminRoleAsync(string userId, CancellationToken ct = default);
}
