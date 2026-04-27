using Microsoft.AspNetCore.Identity;
using Subsy.Application.Common;

namespace Subsy.Infrastructure.Identity;

public static class RoleSeeder
{
    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
    {
        if (!await roleManager.RoleExistsAsync(Roles.Admin))
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
    }
}
