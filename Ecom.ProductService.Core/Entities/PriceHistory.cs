using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ecom.ProductService.Core.Entities;

public partial class PriceHistory
{
    [Key]
    public long Id { get; set; }

    public int ProductId { get; set; }

    public int? VariantId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? OldBasePrice { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal NewBasePrice { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? DiscountAmount { get; set; }

    public bool? IsPromotion { get; set; }

    [Column(TypeName = "decimal(19, 2)")]
    public decimal? FinalPrice { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime StartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? EndDate { get; set; }

    [StringLength(20)]
    public string? CurrencyUnit { get; set; }

    [StringLength(500)]
    public string? ChangeNote { get; set; }

    public int? PromotionId { get; set; }

    public int UserId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("PriceHistories")]
    public virtual Product Product { get; set; } = null!;
}
