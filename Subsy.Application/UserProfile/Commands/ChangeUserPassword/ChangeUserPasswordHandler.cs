using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.UserProfile.Commands.ChangeUserPassword;

public sealed class ChangeUserPasswordHandler : IRequestHandler<ChangeUserPasswordCommand, Unit>
{
    private readonly IUserProfileService _userProfileService;

    public ChangeUserPasswordHandler(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    public async Task<Unit> Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
    {
        await _userProfileService.ChangePasswordAsync(
            request.UserId,
            request.CurrentPassword,
            request.NewPassword,
            cancellationToken);

        return Unit.Value;
    }
}
