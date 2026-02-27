using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ecom.ProductService.Core.Entities;

public partial class ProductImage
{
    [Key]
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int? VariantId { get; set; }

    [StringLength(500)]
    [Unicode(false)]
    public string ImagePath { get; set; } = null!;

    [StringLength(255)]
    public string? AltText { get; set; }

    public int? SortOrder { get; set; }

    public byte? Status { get; set; }

    public bool? IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("ProductImages")]
    public virtual Product Product { get; set; } = null!;
}
