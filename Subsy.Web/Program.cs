using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subsy.Application.DependencyInjection;
using Subsy.Infrastructure.BackgroundJobs;
using Subsy.Infrastructure.DependencyInjection;
using Subsy.Web.Filters;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add<FluentValidationModelStateFilter>();
    o.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Cookie ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

// Günlük sabah 8'de çalışacak recurring job
RecurringJob.AddOrUpdate<PaymentReminderJob>(
    "payment-reminder",
    job => job.ExecuteAsync(),
    "0 8 * * *"); // Cron: her gün saat 08:00

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();