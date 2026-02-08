using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Subsy.Models;

namespace Subsy.Data
{
    public class SubsyContext : IdentityDbContext
    {
        public SubsyContext(DbContextOptions<SubsyContext> options): base(options)
        {
            
        }

        public DbSet<SubscriptionViewModel> Subscriptions { get; set; }
    }
}
