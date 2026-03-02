using MediatR;

namespace Subsy.Application.UserProfile.Commands.UpdateUserProfilePhoto;

public sealed record UpdateUserProfilePhotoCommand(
    string UserId,
    string OriginalFileName,
    string ContentType,
    byte[] FileBytes
) : IRequest<Unit>;
