using MediatR;
using Subsy.Application.Common;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Admin.Queries.GetAuditLogs;

public sealed class GetAuditLogsHandler : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    private readonly IAdminService _adminService;

    public GetAuditLogsHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
        => _adminService.GetAuditLogsAsync(request.Search, request.Page, request.PageSize, cancellationToken);
}
