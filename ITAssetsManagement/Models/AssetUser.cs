using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace ITAssetsManagement.Models;
[Table("Asset_User")]
public class AssetUser
{
    [Key]
    [Column("User_ID")]
    public string UserId { get; set; }
    [Column("User_Name")]
    public string UserName { get; set; }
    [Column("User_Password")]
    public string UserPassword { get; set; }
    [Column("Dept_Name")]
    public string DeptName { get; set; }
    public string User_Status { get; set; }
}
