using MediatR;

namespace Subsy.Application.Admin.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<IReadOnlyList<AdminUserDto>>;
