using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ecom.ProductService.Core.Entities;

[Index("Status", "PublishDate", Name = "IX_Product_Status_PublishDate_Price")]
[Index("NameAscii", Name = "UQ__Products__448353502CE724BD", IsUnique = true)]
[Index("Sku", Name = "UQ__Products__CA1ECF0D1D1AD9E0", IsUnique = true)]
public partial class Product
{
    [Key]
    public int Id { get; set; }

    public int ProductGroupId { get; set; }

    public int BrandId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string NameAscii { get; set; } = null!;

    [Column("SKU")]
    [StringLength(50)]
    [Unicode(false)]
    public string Sku { get; set; } = null!;

    [StringLength(100)]
    public string? VersionName { get; set; }

    [StringLength(500)]
    [Unicode(false)]
    public string? MainImage { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal OriginalPrice { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    [StringLength(20)]
    public string? CurrencyUnit { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    [StringLength(100)]
    public string? MadeIn { get; set; }

    [StringLength(100)]
    public string? Version { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PublishDate { get; set; }

    public byte? Status { get; set; }

    public bool? IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("BrandId")]
    [InverseProperty("Products")]
    public virtual Brand Brand { get; set; } = null!;

    [InverseProperty("Product")]
    public virtual ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();

    [InverseProperty("Product")]
    public virtual ICollection<ProductAttributeValue> ProductAttributeValues { get; set; } = new List<ProductAttributeValue>();

    [InverseProperty("Product")]
    public virtual ICollection<ProductCategoryMapping> ProductCategoryMappings { get; set; } = new List<ProductCategoryMapping>();

    [ForeignKey("ProductGroupId")]
    [InverseProperty("Products")]
    public virtual ProductGroup ProductGroup { get; set; } = null!;

    [InverseProperty("Product")]
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    [InverseProperty("Product")]
    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
}
