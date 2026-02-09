using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes;

public class ReceivedStockDTO
{
    public int ReceivedStockId { get; set; }

    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } // GeneralStatus enum

    [Required(ErrorMessage = "Due Date is required")]
    public DateTime DueDate { get; set; }

    [Required(ErrorMessage = "User ID is required")]
    public string UserId { get; set; }

    public string? Comment { get; set; }

    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }

    [Required(ErrorMessage = "Source Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Source Warehouse ID must be greater than 0")]
    public int SourceWarehouseId { get; set; }

    [Required(ErrorMessage = "Transferred Stock ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Transferred Stock ID must be greater than 0")]
    public int TransferredStockId { get; set; }

    public List<ReceivedItemDTO>? Items { get; set; }
}

public class AddReceivedStockDTO
{
    [Required(ErrorMessage = "Transferred Stock ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Transferred Stock ID must be greater than 0")]
    public int TransferredStockId { get; set; }

    [Required(ErrorMessage = "Due Date is required")]
    public DateTime DueDate { get; set; }

    public string? Comment { get; set; }
}

public class UpdateReceivedStockDTO
{
    [Required(ErrorMessage = "ReceivedStockId is required")]
    public int ReceivedStockId { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }
}

public class ReceivedItemDTO
{
    public int ReceivedItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Unit of Measure is required")]
    public int UoMEntry { get; set; }

    public string? BarCode { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Unit Price must be zero or greater")]
    public decimal? UnitPrice { get; set; }

    public string? ErrorMessage { get; set; }

    public string Status { get; set; }

    public string? Comment { get; set; }

    [Required(ErrorMessage = "ReceivedStockId is required")]
    public int ReceivedStockId { get; set; }

    [Required(ErrorMessage = "TransferredItemId is required")]
    public int TransferredItemId { get; set; }

    [Required(ErrorMessage = "ItemId is required")]
    public int ItemId { get; set; }

    public List<ReceivedStockBatchDTO>? Batches { get; set; }
}

public class AddReceivedItemDTO
{
    [Required(ErrorMessage = "Transferred Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Transferred Item ID must be greater than 0")]
    public int TransferredItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }
}

public class UpdateReceivedItemDTO
{
    [Required(ErrorMessage = "ReceivedItemId is required")]
    public int ReceivedItemId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal? Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }
}

public class ReceivedStockBatchDTO
{
    public int ReceivedStockBatchId { get; set; }

    [Required(ErrorMessage = "Received Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Received Item ID must be greater than 0")]
    public int ReceivedItemId { get; set; }

    [Required(ErrorMessage = "Transferred Stock Batch ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Transferred Stock Batch ID must be greater than 0")]
    public int TransferredStockBatchId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }

    [StringLength(100, ErrorMessage = "Batch Number cannot exceed 100 characters")]
    public string? BatchNumber { get; set; }

    public DateTime? ExpiryDate { get; set; }
}

public class AddReceivedStockBatchDTO
{
    [Required(ErrorMessage = "Received Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Received Item ID must be greater than 0")]
    public int ReceivedItemId { get; set; }

    [Required(ErrorMessage = "Transferred Stock Batch ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Transferred Stock Batch ID must be greater than 0")]
    public int TransferredStockBatchId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }
}

public class UpdateReceivedStockBatchDTO
{
    [Required(ErrorMessage = "ReceivedStockBatchId is required")]
    public int ReceivedStockBatchId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }
}

public class ReceivedStockResponseDTO
{
    public int ReceivedStockId { get; set; }
    public string Status { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserId { get; set; }
    public string? UserFullName { get; set; }
    public int WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public int SourceWarehouseId { get; set; }
    public string? SourceWarehouseName { get; set; }
    public int TransferredStockId { get; set; }
    public int ItemsCount { get; set; }
}
