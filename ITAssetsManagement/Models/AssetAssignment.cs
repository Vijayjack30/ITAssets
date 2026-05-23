using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace ITAssetsManagement.Models;
[Table("AssetAssignment")]
public class AssetAssignment
{
    
    public string AssetId { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public string? AssetLocation { get; set; }
    public string? Comments { get; set; }
    
    public DateTime? ReturnDate { get; set; }
    public DateTime? RepairCompletedDate { get; set; }
}
