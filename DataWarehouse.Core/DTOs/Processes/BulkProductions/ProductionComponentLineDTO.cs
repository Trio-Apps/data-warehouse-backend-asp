using System;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes.BulkProductions;

public class ProductionComponentLineDTO
{
    public int ProductionComponentLineId { get; set; }
    public int ProductionOrderId { get; set; }
    public int ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public int WarehouseId { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal? IssuedQuantity { get; set; }
    public decimal? InWhsQuantity { get; set; }
    public string IssueType { get; set; } = "Backflush";
}

public class AddProductionComponentLineDTO
{
    [Required(ErrorMessage = "Production Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Production Order ID must be greater than 0")]
    public int ProductionOrderId { get; set; }

    [Required(ErrorMessage = "Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Item ID must be greater than 0")]
    public int ItemId { get; set; }

    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }

    [Required(ErrorMessage = "Required quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Required quantity must be greater than 0")]
    public decimal RequiredQuantity { get; set; }

    [StringLength(20, ErrorMessage = "IssueType cannot exceed 20 characters")]
    public string? IssueType { get; set; }
}

public class UpdateProductionComponentLineDTO
{
    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }

    [Required(ErrorMessage = "Required quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Required quantity must be greater than 0")]
    public decimal RequiredQuantity { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Issued quantity cannot be negative")]
    public decimal? IssuedQuantity { get; set; }

    [StringLength(20, ErrorMessage = "IssueType cannot exceed 20 characters")]
    public string? IssueType { get; set; }
}
