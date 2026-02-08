using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Subsy.Domain.Entities;

namespace Subsy.Infrastructure.Persistence;

public class SubsyContext : IdentityDbContext<IdentityUser>
{
    public SubsyContext(DbContextOptions<SubsyContext> options) : base(options)
    {
    }

    public DbSet<Subscription> Subscriptions { get; set; } = default!;
}
