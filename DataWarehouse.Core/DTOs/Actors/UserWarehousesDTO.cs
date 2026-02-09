using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Actors;

public class UserWarehousesDTO
{
    public int? UserWarehousesId { get; set; }

    [Required(ErrorMessage = "User ID is required")]
    public string UserId { get; set; }

    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }
}

public class UserWarehousesResponseDTO
{
    public int UserWarehousesId { get; set; }
    public string UserId { get; set; }
    public string? UserFullName { get; set; }
    public string? UserEmail { get; set; }
    public int WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
}
