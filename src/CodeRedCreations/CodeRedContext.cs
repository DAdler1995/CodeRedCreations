using CodeRedCreations.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeRedCreations
{
    public class CodeRedContext : DbContext
    {
        public DbSet<BrandModel> Brands { get; set; }
        public DbSet<PartModel> Parts { get; set; }
        public DbSet<CarModel> Cars { get; set; }
    }
}
