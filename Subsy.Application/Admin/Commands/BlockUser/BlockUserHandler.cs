using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Admin.Commands.BlockUser;

public sealed class BlockUserHandler : IRequestHandler<BlockUserCommand>
{
    private readonly IAdminService _adminService;

    public BlockUserHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public Task Handle(BlockUserCommand request, CancellationToken cancellationToken)
        => _adminService.BlockUserAsync(request.UserId, request.RequestingUserId, cancellationToken);
}
