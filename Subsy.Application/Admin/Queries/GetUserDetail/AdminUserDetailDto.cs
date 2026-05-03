namespace Subsy.Application.Admin.Queries.GetUserDetail;

public record AdminUserDetailDto(
    string Id,
    string UserName,
    string Email,
    bool IsAdmin,
    bool IsBlocked,
    bool EmailConfirmed,
    bool TwoFactorEnabled,
    DateTime? RegisteredAt,
    IReadOnlyList<AdminSubscriptionItem> Subscriptions,
    IReadOnlyList<AdminAuditLogItem> RecentAuditLogs);

public record AdminSubscriptionItem(
    int Id,
    string Name,
    decimal Price,
    string Currency,
    DateTime RenewalDate,
    bool IsArchived);

public record AdminAuditLogItem(
    string Action,
    string EntityName,
    string? Details,
    DateTime CreatedAt);
