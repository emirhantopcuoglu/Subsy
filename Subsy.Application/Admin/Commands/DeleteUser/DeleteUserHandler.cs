using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Admin.Commands.DeleteUser;

public sealed class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IAdminService _adminService;

    public DeleteUserHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        => _adminService.DeleteUserAsync(request.UserId, request.RequestingUserId, cancellationToken);
}
