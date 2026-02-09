using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Actors;

public class SapSyncStatusFrontDTO
{
    public int? SapSyncStatusFrontId { get; set; }

    [Required(ErrorMessage = "Entity Name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Entity Name must be between 1 and 200 characters")]
    public string EntityName { get; set; }

    [Required(ErrorMessage = "Last Sync Date is required")]
    public DateTime LastSyncDate { get; set; }
}

