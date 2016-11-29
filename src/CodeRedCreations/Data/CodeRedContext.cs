using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CodeRedCreations.Models;
using CodeRedCreations.Models.Account;

namespace CodeRedCreations.Data
{
    public class CodeRedContext : IdentityDbContext<ApplicationUser>
    {
        public CodeRedContext(DbContextOptions<CodeRedContext> options)
            : base(options)
        {
        }

        public DbSet<BrandModel> Brand { get; set; }
        public DbSet<CarModel> Car { get; set; }
        public DbSet<ImageModel> Images { get; set; }
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<PromoModel> Promos { get; set; }
        public DbSet<CarProduct> CarProduct { get; set; }
        public DbSet<UserReferral> UserReferral { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CarProduct>()
                .HasKey(x => new { x.ProductId, x.CarId });

            builder.Entity<CarProduct>()
                .HasOne(x => x.Car)
                .WithMany(x => x.CarProducts)
                .HasForeignKey(x => x.CarId);

            builder.Entity<CarProduct>()
                .HasOne(x => x.Product)
                .WithMany(x => x.CarProducts)
                .HasForeignKey(x => x.ProductId);
        }
    }
}
