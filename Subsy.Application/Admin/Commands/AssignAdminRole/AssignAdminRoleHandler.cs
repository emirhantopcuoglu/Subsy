using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Admin.Commands.AssignAdminRole;

public sealed class AssignAdminRoleHandler : IRequestHandler<AssignAdminRoleCommand>
{
    private readonly IAdminService _adminService;

    public AssignAdminRoleHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public Task Handle(AssignAdminRoleCommand request, CancellationToken cancellationToken)
        => _adminService.AssignAdminRoleAsync(request.UserId, request.RequestingUserId, cancellationToken);
}
