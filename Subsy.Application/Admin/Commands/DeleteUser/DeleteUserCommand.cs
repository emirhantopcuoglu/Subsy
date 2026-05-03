using MediatR;

namespace Subsy.Application.Admin.Commands.DeleteUser;

public record DeleteUserCommand(string UserId, string RequestingUserId) : IRequest;
