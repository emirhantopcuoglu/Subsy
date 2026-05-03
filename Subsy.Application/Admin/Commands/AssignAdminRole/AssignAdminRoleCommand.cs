using MediatR;

namespace Subsy.Application.Admin.Commands.AssignAdminRole;

public record AssignAdminRoleCommand(string UserId) : IRequest;
