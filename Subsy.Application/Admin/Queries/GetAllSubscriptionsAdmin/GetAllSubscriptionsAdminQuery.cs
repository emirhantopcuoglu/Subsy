using MediatR;

namespace Subsy.Application.Admin.Queries.GetAllSubscriptionsAdmin;

public record GetAllSubscriptionsAdminQuery : IRequest<IReadOnlyList<AdminSubscriptionDto>>;
