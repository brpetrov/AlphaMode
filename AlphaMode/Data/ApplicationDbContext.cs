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
        public DbSet<Bundle> Bundles => Set<Bundle>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Order>().HasIndex(x => x.CreatedUtc);
            builder.Entity<Order>().HasIndex(x => new { x.Status, x.CreatedUtc });

            builder.Entity<PromoCode>()
           .HasIndex(p => p.Code)
           .IsUnique();

            builder.Entity<Bundle>().HasData(
                new Bundle { Id = 1, Name = "1 опаковка", Size = 1, Price = 59m, IsActive = true },
                new Bundle { Id = 2, Name = "2 опаковки", Size = 2, Price = 100m, IsActive = true },
                new Bundle { Id = 3, Name = "3 опаковки", Size = 3, Price = 140m, IsActive = true }
            );
        }
    }
}
