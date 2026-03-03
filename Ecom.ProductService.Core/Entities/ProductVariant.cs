using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ecom.ProductService.Core.Entities;

[Index("Sku", Name = "UQ__ProductV__CA1ECF0D85E9B0E5", IsUnique = true)]
public partial class ProductVariant
{
    [Key]
    public int Id { get; set; }

    public int ProductId { get; set; }

    [StringLength(100)]
    public string ColorName { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string? ColorCode { get; set; }

    [Column("SKU")]
    [StringLength(50)]
    [Unicode(false)]
    public string Sku { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    public bool IsDefault { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string NameAscii { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("ProductVariants")]
    public virtual Product Product { get; set; } = null!;
}
