using MediatR;

namespace Subsy.Application.Admin.Commands.BlockUser;

public record BlockUserCommand(string UserId, string RequestingUserId) : IRequest;
