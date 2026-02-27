using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ecom.ProductService.Core.Entities;

public partial class SystemLog
{
    [Key]
    public long Id { get; set; }

    public int UserId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string TableName { get; set; } = null!;

    public int RecordId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Action { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string? ColumnName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    [Column("IPAddress")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Ipaddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }
}
