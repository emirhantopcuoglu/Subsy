using MediatR;

namespace Subsy.Application.Admin.Commands.BroadcastEmail;

public record BroadcastEmailCommand(string Subject, string Body) : IRequest<int>;
