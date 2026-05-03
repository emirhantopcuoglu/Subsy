using MediatR;

namespace Subsy.Application.Admin.Commands.SendPasswordResetEmail;

public record SendPasswordResetEmailCommand(string UserId, string CallbackUrl) : IRequest;
