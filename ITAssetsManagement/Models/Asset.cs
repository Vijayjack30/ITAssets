using System.ComponentModel.DataAnnotations;
namespace ITAssetsManagement.Models;
public partial class Asset
{
    [Required]
    public string AssetId { get; set; } = null!;
    [Required]
    public int AssetTypeId { get; set; }
    [Required]
    public string AssetModel { get; set; } = null!;
    [Required]
    public string SerialNumber { get; set; } = null!;
    public string? InvoiceNumber { get; set; }
    public DateTime? PurchasedDate { get; set; }
    public string? PurchasedFrom { get; set; }
    [Required]
    public string PurchasedAs { get; set; } = null!;
    [Required]
    public int Status { get; set; }
    [Required]
    public int Condition { get; set; }
    public DateTime? DisposedDate { get; set; }
    public string? Comments { get; set; }
    public DateTime LastModified { get; set; }
}
