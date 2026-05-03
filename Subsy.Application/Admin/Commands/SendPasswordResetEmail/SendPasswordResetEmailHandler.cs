using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Admin.Commands.SendPasswordResetEmail;

public sealed class SendPasswordResetEmailHandler : IRequestHandler<SendPasswordResetEmailCommand>
{
    private readonly IAdminService _adminService;

    public SendPasswordResetEmailHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public Task Handle(SendPasswordResetEmailCommand request, CancellationToken cancellationToken)
        => _adminService.SendPasswordResetEmailAsync(request.UserId, request.UserName, request.Email, request.CallbackUrl, cancellationToken);
}
