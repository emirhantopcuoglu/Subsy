using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Admin.Queries.GetAllUsers;

public sealed class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, IReadOnlyList<AdminUserDto>>
{
    private readonly IAdminService _adminService;

    public GetAllUsersHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public Task<IReadOnlyList<AdminUserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        => _adminService.GetAllUsersAsync(cancellationToken);
}
