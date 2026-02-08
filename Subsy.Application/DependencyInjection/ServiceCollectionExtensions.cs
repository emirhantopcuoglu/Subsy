using Microsoft.Extensions.DependencyInjection;
using Subsy.Application.Finance.Dashboard;
using Subsy.Application.Subscriptions.Queries.GetActiveSubscriptions;
using Subsy.Application.Subscriptions.Queries.GetArchivedSubscriptions;
using Subsy.Application.Subscriptions.Queries.GetDueSubscriptions;
using Subsy.Application.Subscriptions.Queries.GetSubscriptionDashboard;
using Subsy.Application.Subscriptions.Queries.GetUserSubscriptions;
using Subsy.Application.Subscriptions.Queries.GetUserTotalPrice;

namespace Subsy.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Subscriptions
        services.AddScoped<GetUserSubscriptionsHandler>();
        services.AddScoped<GetUserTotalPriceHandler>();
        services.AddScoped<GetSubscriptionDashboardHandler>();

        services.AddScoped<GetActiveSubscriptionsHandler>();
        services.AddScoped<GetArchivedSubscriptionsHandler>();
        services.AddScoped<GetDueSubscriptionsHandler>();
        // Finance
        services.AddScoped<GetFinanceDashboardHandler>();

        return services;
    }
}
