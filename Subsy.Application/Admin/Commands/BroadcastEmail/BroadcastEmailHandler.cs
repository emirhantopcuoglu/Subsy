using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Admin.Commands.BroadcastEmail;

public sealed class BroadcastEmailHandler : IRequestHandler<BroadcastEmailCommand, int>
{
    private readonly IAdminService _adminService;

    public BroadcastEmailHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public Task<int> Handle(BroadcastEmailCommand request, CancellationToken cancellationToken)
        => _adminService.BroadcastEmailAsync(request.Subject, request.Body, cancellationToken);
}
