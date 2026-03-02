using System;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes.BulkProductions;

public class ProductionHeaderBatchDTO
{
    public int ProductionHeaderBatchId { get; set; }
    public int ProductionOrderId { get; set; }
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class AddProductionHeaderBatchDTO
{
    [Required(ErrorMessage = "Production Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Production Order ID must be greater than 0")]
    public int ProductionOrderId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Batch Number is required")]
    [StringLength(100, ErrorMessage = "Batch Number cannot exceed 100 characters")]
    public string BatchNumber { get; set; } = string.Empty;

    public DateTime? ExpiryDate { get; set; }
}

public class UpdateProductionHeaderBatchDTO
{
    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Batch Number is required")]
    [StringLength(100, ErrorMessage = "Batch Number cannot exceed 100 characters")]
    public string BatchNumber { get; set; } = string.Empty;

    public DateTime? ExpiryDate { get; set; }
}
