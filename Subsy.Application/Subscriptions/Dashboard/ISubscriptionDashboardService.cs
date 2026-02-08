namespace Subsy.Application.Subscriptions.Dashboard;

public interface ISubscriptionDashboardService
{
    Task<SubscriptionDashboardDto> GetDashboardAsync(string userId, CancellationToken cancellationToken = default);
}
