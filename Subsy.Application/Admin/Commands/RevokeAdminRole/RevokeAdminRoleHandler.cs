using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Admin.Commands.RevokeAdminRole;

public sealed class RevokeAdminRoleHandler : IRequestHandler<RevokeAdminRoleCommand>
{
    private readonly IAdminService _adminService;

    public RevokeAdminRoleHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public Task Handle(RevokeAdminRoleCommand request, CancellationToken cancellationToken)
        => _adminService.RevokeAdminRoleAsync(request.UserId, cancellationToken);
}
