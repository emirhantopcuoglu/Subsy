using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.UserProfile.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileCommand, Unit>
{
    private readonly IUserProfileService _userProfileService;

    public UpdateUserProfileHandler(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    public async Task<Unit> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        await _userProfileService.UpdateAsync(request.UserId, request.UserName, request.Email, cancellationToken);
        return Unit.Value;
    }
}
