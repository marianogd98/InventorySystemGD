// Inventory.Infrastructure/Persistence/ApplicationDbContext.cs
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Price)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.Stock)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime2(7)")
                .IsRequired();
        });
    }
}

