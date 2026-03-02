using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.UserProfile.Commands.UpdateUserProfilePhoto;

public sealed class UpdateUserProfilePhotoHandler : IRequestHandler<UpdateUserProfilePhotoCommand, Unit>
{
    private readonly IUserProfileService _userProfileService;

    public UpdateUserProfilePhotoHandler(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    public async Task<Unit> Handle(UpdateUserProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        await _userProfileService.UpdateProfilePhotoAsync(
            request.UserId,
            request.OriginalFileName,
            request.ContentType,
            request.FileBytes,
            cancellationToken);

        return Unit.Value;
    }
}
