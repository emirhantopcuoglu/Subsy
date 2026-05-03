using MediatR;

namespace Subsy.Application.Admin.Commands.UnblockUser;

public record UnblockUserCommand(string UserId) : IRequest;
