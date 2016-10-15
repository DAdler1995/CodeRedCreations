using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CarModel>();
            builder.Entity<BrandModel>();
            builder.Entity<PartModel>();
        }
    }
}
