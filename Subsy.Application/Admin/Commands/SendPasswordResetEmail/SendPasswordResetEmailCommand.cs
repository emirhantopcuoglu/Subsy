using MediatR;

namespace Subsy.Application.Admin.Commands.SendPasswordResetEmail;

public record SendPasswordResetEmailCommand(string UserId, string UserName, string Email, string CallbackUrl) : IRequest;
