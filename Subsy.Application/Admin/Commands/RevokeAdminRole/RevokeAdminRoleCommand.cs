using MediatR;

namespace Subsy.Application.Admin.Commands.RevokeAdminRole;

public record RevokeAdminRoleCommand(string UserId, string RequestingUserId) : IRequest;
