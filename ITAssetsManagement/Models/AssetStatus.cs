using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace ITAssetsManagement.Models;
[Keyless]
[Table("AssetStatus")]
public partial class AssetStatus
{
    public int Id { get; set; }
    [StringLength(20)]
    [Unicode(false)]
    public string? Name { get; set; }
    public ICollection<Asset> Assets { get; set; }
}
