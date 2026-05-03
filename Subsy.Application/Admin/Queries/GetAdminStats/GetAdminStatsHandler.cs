using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Admin.Queries.GetAdminStats;

public sealed class GetAdminStatsHandler : IRequestHandler<GetAdminStatsQuery, AdminStatsDto>
{
    private readonly IAdminService _adminService;

    public GetAdminStatsHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public Task<AdminStatsDto> Handle(GetAdminStatsQuery request, CancellationToken cancellationToken)
        => _adminService.GetStatsAsync(cancellationToken);
}
