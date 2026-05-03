using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Admin.Queries.GetAllSubscriptionsAdmin;

public sealed class GetAllSubscriptionsAdminHandler
    : IRequestHandler<GetAllSubscriptionsAdminQuery, IReadOnlyList<AdminSubscriptionDto>>
{
    private readonly IAdminService _adminService;

    public GetAllSubscriptionsAdminHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public Task<IReadOnlyList<AdminSubscriptionDto>> Handle(
        GetAllSubscriptionsAdminQuery request,
        CancellationToken cancellationToken)
        => _adminService.GetAllSubscriptionsAsync(cancellationToken);
}
