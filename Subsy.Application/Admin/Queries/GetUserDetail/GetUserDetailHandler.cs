using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Admin.Queries.GetUserDetail;

public sealed class GetUserDetailHandler : IRequestHandler<GetUserDetailQuery, AdminUserDetailDto>
{
    private readonly IAdminService _adminService;

    public GetUserDetailHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public Task<AdminUserDetailDto> Handle(GetUserDetailQuery request, CancellationToken cancellationToken)
        => _adminService.GetUserDetailAsync(request.UserId, cancellationToken);
}
