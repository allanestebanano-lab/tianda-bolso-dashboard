using Microsoft.EntityFrameworkCore;
using TiendaBolsos.Infrastructure.Data.Analytics;

namespace TiendaBolsos.Infrastructure.Data;

public sealed class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options)
    : DbContext(options)
{
    public DbSet<DimDate> Dates => Set<DimDate>();

    public DbSet<DimProduct> Products => Set<DimProduct>();

    public DbSet<FactSale> Sales => Set<FactSale>();

    public DbSet<FactInventorySnapshot> InventorySnapshots => Set<FactInventorySnapshot>();

    public DbSet<FactApiRequest> ApiRequests => Set<FactApiRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DimDate>(entity =>
        {
            entity.ToTable("DimDate", "Analytics");
            entity.HasKey(row => row.DateKey);
            entity.Property(row => row.Date).HasColumnType("date");
        });

        modelBuilder.Entity<DimProduct>(entity =>
        {
            entity.ToTable("DimProduct", "Analytics");
            entity.HasKey(row => row.ProductKey);
            entity.Property(row => row.ProductName).HasMaxLength(150);
            entity.Property(row => row.CategoryName).HasMaxLength(100);
            entity.Property(row => row.BrandName).HasMaxLength(100);
            entity.Property(row => row.UnitPrice).HasPrecision(10, 2);
        });

        modelBuilder.Entity<FactSale>(entity =>
        {
            entity.ToTable("FactSale", "Analytics");
            entity.HasKey(row => row.SourceDetailId);
            entity.Property(row => row.PaymentMethod).HasMaxLength(50);
            entity.Property(row => row.UnitPrice).HasPrecision(10, 2);
            entity.Property(row => row.Amount).HasPrecision(10, 2);
        });

        modelBuilder.Entity<FactInventorySnapshot>(entity =>
        {
            entity.ToTable("FactInventorySnapshot", "Analytics");
            entity.HasKey(row => new { row.DateKey, row.ProductKey });
        });

        modelBuilder.Entity<FactApiRequest>(entity =>
        {
            entity.ToTable("FactApiRequest", "Analytics");
            entity.HasKey(row => row.RequestId);
            entity.Property(row => row.TimestampUtc).HasColumnType("datetime2(3)");
            entity.Property(row => row.HttpMethod).HasMaxLength(10);
            entity.Property(row => row.Route).HasMaxLength(300);
            entity.Property(row => row.DurationMilliseconds).HasPrecision(12, 3);
            entity.Property(row => row.Environment).HasMaxLength(100);
        });
    }
}
