using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ecom.ProductService.Core.Entities;

[PrimaryKey("ProductId", "CategoryId")]
public partial class ProductCategoryMapping
{
    [Key]
    public int ProductId { get; set; }

    [Key]
    public int CategoryId { get; set; }

    public bool? IsMain { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("ProductCategoryMappings")]
    public virtual Category Category { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("ProductCategoryMappings")]
    public virtual Product Product { get; set; } = null!;
}
