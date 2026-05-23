using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace ITAssetsManagement.Models;
public partial class PurchasedA
{
    [Key]
    [Column("PurchaseID")]
    public int PurchaseId { get; set; }
    [StringLength(20)]
    [Unicode(false)]
    public string PurchasedAs { get; set; } = null!;
}
