using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Subsy.Application.Common.Interfaces;
using Subsy.Infrastructure.Persistence;
using Subsy.Infrastructure.Repositories;

namespace Subsy.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<SubsyContext>(options =>
            options.UseSqlite(connectionString));

        // Identity
        services
            .AddDefaultIdentity<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<SubsyContext>();

        // Repositories
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

        return services;
    }
}
