using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace ITAssetsManagement.Models;
public class AssetAssignmentVM
{
    public string AssetId { get; set; }
    public string AssignedTo { get; set; }          
    public string AssignedToDisplay { get; set; }   
    public string AssetLocation { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public DateTime? RepairCompletedDate { get; set; }
    public string Comments { get; set; }
}
