using DataWarehouse.Domain.Entities.Actors;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Actors;

public class WarehouseDTO
{
    public int WarehouseId { get; set; }

    [Required(ErrorMessage = "Warehouse Name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Warehouse Name must be between 2 and 200 characters")]
    public string WarehouseName { get; set; }

    [Required(ErrorMessage = "Sap ID is required")]
    public int SapId { get; set; }
}

public class AddWarehouseDTO
{
    [Required(ErrorMessage = "Warehouse Code is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Warehouse Code must be between 1 and 50 characters")]
    public string WarehouseCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Warehouse Name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Warehouse Name must be between 2 and 200 characters")]
    public string WarehouseName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sap ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Sap ID must be greater than 0")]
    public int SapId { get; set; }
}

public class WarehouseResponseDTO
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; }
    public string UserId { get; set; }
    public string? UserFullName { get; set; }
    public int ItemsCount { get; set; }
}

public class WarehouseItemDto
{
    public int WarehouseItemId { get; set; }
    public int ItemId { get; set; }
    public int WarehouseId { get; set; }
    public string ItemName { get; set; }
    public string ItemCode { get; set; }
    public string WarehouseCode { get; set; }
    public double? InStock { get; set; }
    public double? MinStock { get; set; }
}
