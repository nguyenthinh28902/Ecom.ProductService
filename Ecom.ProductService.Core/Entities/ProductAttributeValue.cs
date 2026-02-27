using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ecom.ProductService.Core.Entities;

[PrimaryKey("ProductId", "AttributeId")]
public partial class ProductAttributeValue
{
    [Key]
    public int ProductId { get; set; }

    [Key]
    public int AttributeId { get; set; }

    public string Value { get; set; } = null!;

    public bool? IsDeleted { get; set; }

    [ForeignKey("AttributeId")]
    [InverseProperty("ProductAttributeValues")]
    public virtual Attribute Attribute { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("ProductAttributeValues")]
    public virtual Product Product { get; set; } = null!;
}
