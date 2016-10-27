using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CodeRedCreations.Models;

namespace CodeRedCreations.Data
{
    public class CodeRedContext : IdentityDbContext<ApplicationUser>
    {
        public CodeRedContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<BrandModel> Brand { get; set; }
        public DbSet<CarModel> Car { get; set; }
        public DbSet<PartModel> Part { get; set; }
        public DbSet<ImageModel> Images { get; set; }
        public DbSet<PromoModel> Promos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CarModel>();
            builder.Entity<BrandModel>();
            builder.Entity<PartModel>();
            builder.Entity<ImageModel>();
            builder.Entity<PromoModel>();
        }
    }
}
