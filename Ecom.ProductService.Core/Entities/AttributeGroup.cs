using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ecom.ProductService.Core.Entities;

public partial class AttributeGroup
{
    [Key]
    public int Id { get; set; }

    public int CategoryId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    public int? SortOrder { get; set; }

    public bool? IsDeleted { get; set; }

    [InverseProperty("Group")]
    public virtual ICollection<Attribute> Attributes { get; set; } = new List<Attribute>();

    [ForeignKey("CategoryId")]
    [InverseProperty("AttributeGroups")]
    public virtual Category Category { get; set; } = null!;
}
