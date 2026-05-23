using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace ITAssetsManagement.Models;
[Keyless]
[Table("AssetModel")]
public partial class AssetModel
{
    [StringLength(50)]
    [Unicode(false)]
    public string AssetType { get; set; } = null!;
    [Column("AssetModel")]
    [StringLength(50)]
    [Unicode(false)]
    public string Asset_Model { get; set; } = null!;
}
