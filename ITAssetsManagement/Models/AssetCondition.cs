using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace ITAssetsManagement.Models;
[Keyless]
[Table("AssetCondition")]
public partial class AssetCondition
{
    [Column("Cond_ID")]
    public int CondId { get; set; }
    [Column("Cond_name")]
    [StringLength(50)]
    [Unicode(false)]
    public string CondName { get; set; } = null!;
    public ICollection<Asset> Assets { get; set; }
}
