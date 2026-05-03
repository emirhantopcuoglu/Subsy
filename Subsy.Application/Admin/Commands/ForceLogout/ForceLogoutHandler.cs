using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Admin.Commands.ForceLogout;

public sealed class ForceLogoutHandler : IRequestHandler<ForceLogoutCommand>
{
    private readonly IAdminService _adminService;

    public ForceLogoutHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public Task Handle(ForceLogoutCommand request, CancellationToken cancellationToken)
        => _adminService.ForceLogoutAsync(request.UserId, request.RequestingUserId, cancellationToken);
}
