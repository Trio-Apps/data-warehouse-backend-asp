using System;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes.BulkProductions;

public class ProductionComponentBatchDTO
{
    public int ProductionComponentBatchId { get; set; }
    public int ProductionComponentLineId { get; set; }
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class AvailableComponentBatchDTO
{
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal QuantityAllocatedInOrder { get; set; }
    public decimal QuantityAvailable { get; set; }
}

public class AddProductionComponentBatchDTO
{
    [Required(ErrorMessage = "Production Component Line ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Production Component Line ID must be greater than 0")]
    public int ProductionComponentLineId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Batch Number is required")]
    [StringLength(100, ErrorMessage = "Batch Number cannot exceed 100 characters")]
    public string BatchNumber { get; set; } = string.Empty;

    public DateTime? ExpiryDate { get; set; }
}

public class UpdateProductionComponentBatchDTO
{
    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Batch Number is required")]
    [StringLength(100, ErrorMessage = "Batch Number cannot exceed 100 characters")]
    public string BatchNumber { get; set; } = string.Empty;

    public DateTime? ExpiryDate { get; set; }
}
