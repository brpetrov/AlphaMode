using AlphaMode.Models;
using Microsoft.AspNetCore.Identity;
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

    public static class DataSeeder
    {
        public static async Task SeedAsync(
            IServiceProvider services,
            IConfiguration config,
            CancellationToken ct = default)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            var userMgr = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();

            // 1) Apply migrations
            await db.Database.MigrateAsync(ct);

            // 2) Ensure Admin role exists
            const string adminRole = "Admin";
            if (!await roleMgr.RoleExistsAsync(adminRole))
            {
                await roleMgr.CreateAsync(new IdentityRole(adminRole));
            }

            // 3) Seed Admin users from configuration (see appsettings below)
            var admins = config.GetSection("Seed:Admins").Get<List<SeedAdmin>>() ?? new();
            foreach (var a in admins)
            {
                if (string.IsNullOrWhiteSpace(a.Email) || string.IsNullOrWhiteSpace(a.Password))
                    continue;

                var user = await userMgr.FindByEmailAsync(a.Email);
                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = a.Email,
                        Email = a.Email,
                        EmailConfirmed = true
                    };
                    var create = await userMgr.CreateAsync(user, a.Password);
                    if (!create.Succeeded) continue;
                }

                // ensure in role
                if (!await userMgr.IsInRoleAsync(user, adminRole))
                {
                    await userMgr.AddToRoleAsync(user, adminRole);
                }
            }
        }

        private class SeedAdmin
        {
            public string Email { get; set; } = "";
            public string Password { get; set; } = "";
        }
    }
}
