using MediatR;

namespace Subsy.Application.Admin.Queries.GetUserDetail;

public record GetUserDetailQuery(string UserId) : IRequest<AdminUserDetailDto>;
