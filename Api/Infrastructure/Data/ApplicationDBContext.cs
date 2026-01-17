using Api.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions)
    : DbContext(dbContextOptions)
{
    public DbSet<Settings> Settings { get; set; }
    public DbSet<Picture> Pictures { get; set; }
    public DbSet<Hierarchy> Hierarchies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hierarchy>()
            .Property(h => h.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Picture>()
            .Property(p => p.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Picture>()
            .Property(p => p.CurationStatus)
            .HasConversion<string>();

        // Seed default Settings
        modelBuilder.Entity<Settings>().HasData(
            new Settings { Id = 1, ThemeMode = "system", LibraryPath = "" }
        );
    }
}