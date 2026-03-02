using MediatR;

namespace Subsy.Application.UserProfile.Commands.UpdateUserProfile;

public sealed record UpdateUserProfileCommand(
    string UserId,
    string UserName,
    string Email
) : IRequest<Unit>;
