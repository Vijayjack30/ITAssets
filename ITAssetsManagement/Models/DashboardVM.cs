using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ITAssetsManagement.Models
{
    public class DashboardVM
    {
        public int TotalAssets { get; set; }
        public int InStock { get; set; }
        public int Assigned { get; set; }
        public int InRepair { get; set; }
        public int Disposed { get; set; }
        public int Donated { get; set; }
        public int Lost { get; set; }
        public int CondId { get; set; }
        public string CondName { get; set; } = null!;
        public int Desktop { get; set; }
        public int Laptop { get; set; }
        public int Mac { get; set; }
        public int Tablet { get; set; }
        public int Server { get; set; }
        public int Firewall { get; set; }
        public int InternalHardDisk { get; set; }
        public int ExternalHardDisk { get; set; }
        public int Monitor { get; set; }
        public int Keyboard { get; set; }
        public int Mouse { get; set; }
        public int Printer { get; set; }
        public int Scanner { get; set; }
        public int Router { get; set; }
        public int Switch { get; set; }
        public int Storage { get; set; }
        public int Workstation { get; set; }
        public int OtherAccessories { get; set; }
        public int Working { get; set; }
        public int NotWorking { get; set; }
    }
}
