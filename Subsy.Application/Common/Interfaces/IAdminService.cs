using Subsy.Application.Admin.Queries.GetAuditLogs;
using Subsy.Application.Admin.Queries.GetAllSubscriptionsAdmin;
using Subsy.Application.Admin.Queries.GetAdminStats;
using Subsy.Application.Admin.Queries.GetAllUsers;
using Subsy.Application.Admin.Queries.GetUserDetail;
using Subsy.Application.Common;

namespace Subsy.Application.Common.Interfaces;

public interface IAdminService
{
    Task<AdminStatsDto> GetStatsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AdminUserDto>> GetAllUsersAsync(CancellationToken ct = default);
    Task<AdminUserDetailDto> GetUserDetailAsync(string userId, CancellationToken ct = default);
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<AdminSubscriptionDto>> GetAllSubscriptionsAsync(CancellationToken ct = default);

    Task AssignAdminRoleAsync(string targetUserId, string requestingUserId, CancellationToken ct = default);
    Task RevokeAdminRoleAsync(string targetUserId, string requestingUserId, CancellationToken ct = default);
    Task BlockUserAsync(string targetUserId, string requestingUserId, CancellationToken ct = default);
    Task UnblockUserAsync(string targetUserId, string requestingUserId, CancellationToken ct = default);
    Task DeleteUserAsync(string targetUserId, string requestingUserId, CancellationToken ct = default);
    Task ForceLogoutAsync(string targetUserId, string requestingUserId, CancellationToken ct = default);
    Task SendPasswordResetEmailAsync(string targetUserId, string userName, string email, string callbackUrl, CancellationToken ct = default);
    Task<int> BroadcastEmailAsync(string subject, string body, CancellationToken ct = default);
}
