using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SparklingHome.Areas.Identity.Data;

namespace SparklingHome.Data
{
    public class SparklingHomeContext : IdentityDbContext<SparklingHomeUser>
    {
        public SparklingHomeContext(DbContextOptions<SparklingHomeContext> options)
            : base(options)
        {
        }

        public DbSet<SparklingHome.Models.Maid> Maid { get; set; }

        public DbSet<SparklingHome.Models.Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
