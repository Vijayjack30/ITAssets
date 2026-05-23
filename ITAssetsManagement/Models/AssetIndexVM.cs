using ITAssetsManagement.Models;
namespace ITAssetsManagement.Models
{
    public class AssetIndexVM
    {
        public Asset Asset { get; set; }
        public bool IsAssigned { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string Department { get; set; } = "-";
        public string AssetLocation { get; set; } = "In Stock";
    }
}
