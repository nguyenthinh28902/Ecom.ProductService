using System;
using System.Collections.Generic;
using Ecom.ProductService.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecom.ProductService.Infrastructure.DbContexts;

public partial class EcomProductDbContext : DbContext
{
    // Constructor cho Master (Port 5000)
    public EcomProductDbContext(DbContextOptions<EcomProductDbContext> options) : base(options)
    {
    }

    // Constructor dùng cho các lớp con kế thừa (như ReadOnlyDbContext)
    protected EcomProductDbContext(DbContextOptions options) : base(options)
    {
    }

    public virtual DbSet<Core.Entities.Attribute> Attributes { get; set; }

    public virtual DbSet<AttributeGroup> AttributeGroups { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<PriceHistory> PriceHistories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductAttributeValue> ProductAttributeValues { get; set; }

    public virtual DbSet<ProductCategoryMapping> ProductCategoryMappings { get; set; }

    public virtual DbSet<ProductGroup> ProductGroups { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ProductVariant> ProductVariants { get; set; }

    public virtual DbSet<SystemLog> SystemLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Core.Entities.Attribute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attribut__3214EC07AD3D0B44");

            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);

            entity.HasOne(d => d.Group).WithMany(p => p.Attributes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attribute_Group");
        });

        modelBuilder.Entity<AttributeGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attribut__3214EC076F5935D6");

            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);

            entity.HasOne(d => d.Category).WithMany(p => p.AttributeGroups)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttributeGroup_Category");
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Brands__3214EC07C70E4BE5");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Status).HasDefaultValue((byte)1);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC0779CF3757");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Status).HasDefaultValue((byte)1);

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent).HasConstraintName("FK_Category_Parent");
        });

        modelBuilder.Entity<PriceHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PriceHis__3214EC07B4346E26");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CurrencyUnit).HasDefaultValue("VNĐ");
            entity.Property(e => e.DiscountAmount).HasDefaultValue(0m);
            entity.Property(e => e.FinalPrice).HasComputedColumnSql("([NewBasePrice]-[DiscountAmount])", false);
            entity.Property(e => e.IsPromotion).HasDefaultValue(false);

            entity.HasOne(d => d.Product).WithMany(p => p.PriceHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PriceHistory_Product");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Products__3214EC0741F35A17");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CurrencyUnit).HasDefaultValue("VNĐ");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Status).HasDefaultValue((byte)1);

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Brand");

            entity.HasOne(d => d.ProductGroup).WithMany(p => p.Products)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Group");
        });

        modelBuilder.Entity<ProductAttributeValue>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.AttributeId }).HasName("PK__ProductA__08145453A5E500A2");

            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Attribute).WithMany(p => p.ProductAttributeValues)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductAttributeValues_Attribute");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductAttributeValues)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductAttributeValues_Product");
        });

        modelBuilder.Entity<ProductCategoryMapping>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.CategoryId }).HasName("PK__ProductC__159C556DDF41C562");

            entity.Property(e => e.IsMain).HasDefaultValue(false);

            entity.HasOne(d => d.Category).WithMany(p => p.ProductCategoryMappings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mapping_Category");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductCategoryMappings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mapping_Product");
        });

        modelBuilder.Entity<ProductGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductG__3214EC0728A886EB");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Status).HasDefaultValue((byte)1);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductI__3214EC07F43C637B");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Status).HasDefaultValue((byte)1);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Image_Product");
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductV__3214EC078D84D2ED");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.NameAscii).HasDefaultValue("");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Variant_Product");
        });

        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.Property(e => e.FunctionName).HasComment("Tên hàm service thực hiện ghi log");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
