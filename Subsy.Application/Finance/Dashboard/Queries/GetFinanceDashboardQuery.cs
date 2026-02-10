using MediatR;

namespace Subsy.Application.Finance.Dashboard.Queries;

public sealed record GetFinanceDashboardQuery(string UserId) : IRequest<FinanceDashboardDto>;
