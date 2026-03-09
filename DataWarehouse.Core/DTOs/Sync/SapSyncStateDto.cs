namespace DataWarehouse.Core.DTOs.Sync;

public class SapSyncStateDto
{
    public int SapId { get; set; }
    public string EntityName { get; set; }
    public DateTime? LastSyncDate { get; set; }
    public int? Skip { get; set; }
}
