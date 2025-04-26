using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Subsy.Data
{
    public class SubsyContext : IdentityDbContext
    {
        public SubsyContext(DbContextOptions<SubsyContext> options): base(options)
        {
            
        }
    }
}
