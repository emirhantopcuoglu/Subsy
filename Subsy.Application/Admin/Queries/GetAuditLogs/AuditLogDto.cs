namespace Subsy.Application.Admin.Queries.GetAuditLogs;

public record AuditLogDto(
    int Id,
    string UserId,
    string UserName,
    string Action,
    string EntityName,
    string? Details,
    DateTime CreatedAt);
