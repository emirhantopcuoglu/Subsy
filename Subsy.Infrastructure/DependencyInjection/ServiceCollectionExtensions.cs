using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Subsy.Application.Common.Interfaces;
using Subsy.Infrastructure.BackgroundJobs;
using Subsy.Infrastructure.Identity;
using Subsy.Infrastructure.Persistence;
using Subsy.Infrastructure.Repositories;
using Subsy.Infrastructure.Services;
using Subsy.Infrastructure.Settings;
using Subsy.Infrastructure.Storage;

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

        // File storage: Supabase when configured, local disk otherwise
        var supabaseUrl = configuration["Supabase:Url"];
        if (!string.IsNullOrWhiteSpace(supabaseUrl))
        {
            services.Configure<SupabaseStorageSettings>(configuration.GetSection("Supabase"));
            services.AddHttpClient<IFileStorageService, SupabaseStorageService>((sp, client) =>
            {
                var key = configuration["Supabase:ServiceKey"] ?? string.Empty;
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");
            });
        }
        else
        {
            services.AddSingleton<IFileStorageService, LocalFileStorageService>();
        }

        // Repositories
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IUserProfileService, UserProfileService>();

        services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();

        // Email
        services.Configure<SmtpSettings>(configuration.GetSection("Email"));
        services.AddScoped<IEmailService, SmtpEmailService>();

        // Background Jobs
        services.AddScoped<PaymentReminderJob>();

        // Hangfire
        services.AddHangfire(config => config
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSQLiteStorage());

        services.AddHangfireServer();

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Exchange rate service with HttpClient + resilience (retry, circuit breaker, timeout via Polly)
        services.AddHttpClient<IExchangeRateService, ExchangeRateService>()
            .AddStandardResilienceHandler();

        // In-memory cache
        services.AddMemoryCache();

        return services;
    }
}