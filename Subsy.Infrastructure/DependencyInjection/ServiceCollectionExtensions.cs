using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Subsy.Application.Common.Interfaces;
using Subsy.Infrastructure.Identity;
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
            .AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                // Password policy
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                // Lockout: 5 başarısız denemede 15 dakika kilit
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;

                // User
                options.User.RequireUniqueEmail = true;

                // SignIn
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<SubsyContext>()
            .AddDefaultTokenProviders();

        // Repositories
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IUserProfileService, UserProfileService>();

        return services;
    }
}