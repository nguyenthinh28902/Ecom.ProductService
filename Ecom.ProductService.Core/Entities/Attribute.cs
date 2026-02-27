using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ecom.ProductService.Core.Entities;

public partial class Attribute
{
    [Key]
    public int Id { get; set; }

    public int GroupId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    public int? SortOrder { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("Attributes")]
    public virtual AttributeGroup Group { get; set; } = null!;

    [InverseProperty("Attribute")]
    public virtual ICollection<ProductAttributeValue> ProductAttributeValues { get; set; } = new List<ProductAttributeValue>();
}
