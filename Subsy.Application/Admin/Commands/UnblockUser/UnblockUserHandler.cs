using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Admin.Commands.UnblockUser;

public sealed class UnblockUserHandler : IRequestHandler<UnblockUserCommand>
{
    private readonly IAdminService _adminService;

    public UnblockUserHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public Task Handle(UnblockUserCommand request, CancellationToken cancellationToken)
        => _adminService.UnblockUserAsync(request.UserId, cancellationToken);
}
