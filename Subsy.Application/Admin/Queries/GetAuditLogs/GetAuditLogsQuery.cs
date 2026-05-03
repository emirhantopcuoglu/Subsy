using MediatR;
using Subsy.Application.Common;

namespace Subsy.Application.Admin.Queries.GetAuditLogs;

public record GetAuditLogsQuery(string? Search = null, int Page = 1, int PageSize = 50)
    : IRequest<PagedResult<AuditLogDto>>;
