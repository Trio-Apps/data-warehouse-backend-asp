using System;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes.BulkProductions;

public class ProductionReceiptDTO
{
    public int ProductionReceiptId { get; set; }

    [Required(ErrorMessage = "Production Order Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Production Order Item ID must be greater than 0")]
    public int ProductionOrderItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal ProducedQuantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }

    [StringLength(100, ErrorMessage = "Batch Number cannot exceed 100 characters")]
    public string? BatchNumber { get; set; } // SAP Goods Receipt Document (DocEntry)

    public DateTime? ExpiryDate { get; set; }
}

public class AddProductionReceiptDTO
{
    

    [Required(ErrorMessage = "Production Order Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Production Order Item ID must be greater than 0")]
    public int ProductionOrderItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal ProducedQuantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class UpdateProductionReceiptDTO
{
    [Required(ErrorMessage = "ProductionOrderItemId is required")]
    public int ProductionReceiptId { get; set; }


    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal ProducedQuantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }
    public DateTime? ExpiryDate { get; set; }
}


