using Subsy.Application.Admin.Queries.GetAdminStats;
using Subsy.Application.Admin.Queries.GetAllUsers;

namespace Subsy.Application.Common.Interfaces;

public interface IAdminService
{
    Task<AdminStatsDto> GetStatsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AdminUserDto>> GetAllUsersAsync(CancellationToken ct = default);
    Task AssignAdminRoleAsync(string targetUserId, CancellationToken ct = default);
    Task RevokeAdminRoleAsync(string targetUserId, string requestingUserId, CancellationToken ct = default);
    Task BlockUserAsync(string targetUserId, string requestingUserId, CancellationToken ct = default);
    Task UnblockUserAsync(string targetUserId, CancellationToken ct = default);
    Task DeleteUserAsync(string targetUserId, string requestingUserId, CancellationToken ct = default);
}
