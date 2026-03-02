using MediatR;

namespace Subsy.Application.UserProfile.Commands.ChangeUserPassword;

public sealed record ChangeUserPasswordCommand(
    string UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
) : IRequest<Unit>;
