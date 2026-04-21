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
    public DbSet<AuditLog> AuditLogs { get; set; } = default!;

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

        builder.Entity<Subscription>(cfg =>
        {
            cfg.Property(x => x.UserId)
                .IsRequired();

            cfg.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            cfg.Property(x => x.Price)
                .HasPrecision(18, 2);

            cfg.Property(x => x.RenewalPeriodDays)
                .IsRequired();

            cfg.HasIndex(x => new { x.UserId, x.IsArchived, x.RenewalDate });
        });

        builder.Entity<AuditLog>(cfg =>
        {
            cfg.Property(x => x.UserId).IsRequired();
            cfg.Property(x => x.Action).IsRequired().HasMaxLength(50);
            cfg.Property(x => x.EntityName).IsRequired().HasMaxLength(50);
            cfg.Property(x => x.Details).HasMaxLength(500);
            cfg.HasIndex(x => new { x.UserId, x.CreatedAt });
        });
    }
}
