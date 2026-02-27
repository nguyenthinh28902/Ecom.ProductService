using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ecom.ProductService.Core.Entities;

[Index("NameAscii", Name = "UQ__Categori__44835350E115998C", IsUnique = true)]
public partial class Category
{
    [Key]
    public int Id { get; set; }

    public int? ParentId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string NameAscii { get; set; } = null!;

    [StringLength(500)]
    [Unicode(false)]
    public string? IconPath { get; set; }

    public int? SortOrder { get; set; }

    public byte? Status { get; set; }

    public bool? IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<AttributeGroup> AttributeGroups { get; set; } = new List<AttributeGroup>();

    [InverseProperty("Parent")]
    public virtual ICollection<Category> InverseParent { get; set; } = new List<Category>();

    [ForeignKey("ParentId")]
    [InverseProperty("InverseParent")]
    public virtual Category? Parent { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<ProductCategoryMapping> ProductCategoryMappings { get; set; } = new List<ProductCategoryMapping>();
}
