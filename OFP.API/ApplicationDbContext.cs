using Microsoft.EntityFrameworkCore;
using OFP.API.Configuration;
using OFP.API.DTO;
using OFP.API.Models;

namespace OFP.API
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Product> Product { get; set; }
        public DbSet<AppUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
        }
    }
}
