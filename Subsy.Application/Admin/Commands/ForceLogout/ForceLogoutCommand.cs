using MediatR;

namespace Subsy.Application.Admin.Commands.ForceLogout;

public record ForceLogoutCommand(string UserId, string RequestingUserId) : IRequest;
