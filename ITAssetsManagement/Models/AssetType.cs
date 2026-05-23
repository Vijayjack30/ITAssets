using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace ITAssetsManagement.Models;
[Keyless]
[Table("AssetType")]
public partial class AssetType
{
    public int Id { get; set; }
    [StringLength(50)]
    [Unicode(false)]
    public string Name { get; set; } = null!;
    public ICollection<Asset> Assets { get; set; }
}
