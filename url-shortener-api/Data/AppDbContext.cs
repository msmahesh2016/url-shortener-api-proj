using Microsoft.EntityFrameworkCore;
using url_shortener_api.Models;

namespace url_shortener_api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
       : base(options) { }

        public DbSet<UrlMapping> Urls => Set<UrlMapping>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UrlMapping>()
                .HasIndex(x => x.ShortCode)
                .IsUnique();
        }
    }
}
