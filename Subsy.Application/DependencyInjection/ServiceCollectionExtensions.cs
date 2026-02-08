using Microsoft.Extensions.DependencyInjection;
using Subsy.Application.Subscriptions.Dashboard;

namespace Subsy.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISubscriptionDashboardService, SubscriptionDashboardService>();
        return services;
    }
}
