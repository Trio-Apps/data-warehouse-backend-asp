using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes.BulkProductions;

public class ProductionOrderItemDTO
{
    public int ProductionOrderItemId { get; set; }

    [Required(ErrorMessage = "Planned Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Planned Quantity must be greater than 0")]
    public decimal PlannedQuantity { get; set; }

    public decimal? ProducedQuantity { get; set; }


    public int? AbsoluteEntry { get; set; } // SAP Production Order AbsoluteEntry

    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Error Message cannot exceed 500 characters")]
    public string? ErrorMessage { get; set; }

    public DateTime? ProcessedAt { get; set; }

    [Required(ErrorMessage = "Production Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Production Order ID must be greater than 0")]
    public int ProductionOrderId { get; set; }

    [Required(ErrorMessage = "Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Item ID must be greater than 0")]
    public int ItemId { get; set; }

    public List<ProductionReceiptDTO>? ProductionReceipts { get; set; }
}


public class AddProductionOrderItemDTO
{
    [Required(ErrorMessage = "Production Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Production Order ID must be greater than 0")]
    public int ProductionOrderId { get; set; }

    [Required(ErrorMessage = "Planned Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Planned Quantity must be greater than 0")]
    public decimal PlannedQuantity { get; set; }

    [Required(ErrorMessage = "Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Item ID must be greater than 0")]
    public int ItemId { get; set; }

}

public class UpdateProductionOrderItemDTO
{
    [Required(ErrorMessage = "Planned Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Planned Quantity must be greater than 0")]
    public decimal PlannedQuantity { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Planned Quantity must be greater than 0")]
    public decimal? ProducedQuantity { get; set; }

}
