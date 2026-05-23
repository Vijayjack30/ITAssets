namespace ITAssetsManagement.Models
{
    public class AssetHistoryVM
    {
        public Asset Asset { get; set; } = null!;
        public string AssetTypeName { get; set; } = "";
        public string StatusName { get; set; } = "";
        public List<AuditLog> AuditLogs { get; set; } = new();
        public List<AssetAssignment> Assignments { get; set; } = new();
    }
}
