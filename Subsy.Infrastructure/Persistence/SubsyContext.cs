using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Subsy.Domain.Entities;

namespace Subsy.Infrastructure.Persistence;

public class SubsyContext : IdentityDbContext
{
    public SubsyContext(DbContextOptions<SubsyContext> options) : base(options)
    {
    }

    public DbSet<Subscription> Subscriptions { get; set; } = default!;
    public DbSet<UserProfile> UserProfiles { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserProfile>(cfg =>
        {
            cfg.ToTable("UserProfiles");
            cfg.HasKey(x => x.UserId);

            cfg.Property(x => x.UserId)
                .IsRequired();

            cfg.Property(x => x.RegisteredAt)
                .IsRequired();

            cfg.Property(x => x.ProfilePhotoPath)
                .HasMaxLength(260);
        });
    }
}
