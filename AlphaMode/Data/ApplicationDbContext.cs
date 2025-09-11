using AlphaMode.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<PromoCode> PromoCodes => Set<PromoCode>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Order>().HasIndex(x => x.CreatedUtc);
            builder.Entity<Order>().HasIndex(x => new { x.Status, x.CreatedUtc });

            builder.Entity<PromoCode>()
           .HasIndex(p => p.Code)
           .IsUnique(); 
        }
    }
}
