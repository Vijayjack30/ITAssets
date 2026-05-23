using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
namespace ITAssetsManagement.Models;

public partial class ITAssetsContext : DbContext
{
    public ITAssetsContext()
    {
    }
    public ITAssetsContext(DbContextOptions<ITAssetsContext> options)
        : base(options)
    {
    }
    public virtual DbSet<Asset> Assets { get; set; }
    public virtual DbSet<AssetAssignment> AssetAssignments { get; set; }
    public virtual DbSet<AssetCondition> AssetConditions { get; set; }
    public virtual DbSet<AssetModel> AssetModels { get; set; }
    public virtual DbSet<AssetStatus> AssetStatuses { get; set; }
    public virtual DbSet<AssetType> AssetTypes { get; set; }
    public virtual DbSet<AssetUser> AssetUsers { get; set; }
    public DbSet<AssetLocation> AssetLocations { get; set; }
    public virtual DbSet<AuditLog> AuditLogs { get; set; }
    public virtual DbSet<PurchasedA> PurchasedAs { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

        => optionsBuilder.UseSqlServer("Server=192.168.0.3;Database=ITAssets;User Id=sa;Password=Quick123;TrustServerCertificate=True");
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.AssetId).HasName("PK__Asset__434923726EB2B15D");
        });
        modelBuilder.Entity<PurchasedA>(entity =>
        {
            entity.Property(e => e.PurchaseId).ValueGeneratedNever();
        });
        modelBuilder.Entity<AssetModel>()
        .HasKey(x => new { x.AssetType, x.Asset_Model });
        modelBuilder.Entity<AssetAssignment>(entity =>
        {
            entity.ToTable("AssetAssignment");
            entity.HasKey(e => new { e.AssetId, e.AssignedDate });
            entity.Property(e => e.AssignedDate)
                  .ValueGeneratedNever();
            entity.Property(e => e.AssetId)
                  .HasMaxLength(50)
                  .IsRequired();
            entity.Property(e => e.AssignedTo)
                  .HasMaxLength(100)
                  .IsRequired();
        });
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
        modelBuilder.Entity<AssetLocation>().ToTable("AssetLocation");
        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
