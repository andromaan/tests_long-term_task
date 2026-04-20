using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Agent> Agents { get; set; }
    public DbSet<Property> Properties { get; set; }
    public DbSet<Inquiry> Inquiries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Property configuration
        modelBuilder
            .Entity<Property>()
            .HasOne(p => p.Agent)
            .WithMany(a => a.Properties)
            .HasForeignKey(p => p.AgentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder
            .Entity<Property>()
            .Property(p => p.Type)
            .HasConversion(
                v => v.ToString(),
                v => (PropertyType)Enum.Parse(typeof(PropertyType), v)
            )
            .IsRequired();

        modelBuilder
            .Entity<Property>()
            .Property(p => p.Status)
            .HasConversion(
                v => v.ToString(),
                v => (PropertyStatus)Enum.Parse(typeof(PropertyStatus), v)
            )
            .IsRequired();

        modelBuilder
            .Entity<Property>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // Inquiry configuration
        modelBuilder
            .Entity<Inquiry>()
            .HasOne(i => i.Property)
            .WithMany(p => p.Inquiries)
            .HasForeignKey(i => i.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Agent configuration
        modelBuilder.Entity<Agent>().HasIndex(a => a.LicenseNumber).IsUnique();
    }
}
