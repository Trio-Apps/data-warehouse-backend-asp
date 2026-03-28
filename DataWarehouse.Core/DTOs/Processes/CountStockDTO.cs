using DataWarehouse.Core.DTOs.BarCode;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes;

public class CountStockDTO : GeneralOrderDto
{
    public int CountStockId { get; set; }
    public int? ReasonId { get; set; }
    public string? ReasonName { get; set; }

  
    [StringLength(50, ErrorMessage = "Live Status cannot exceed 50 characters")]
    public string? LiveStatus { get; set; }

  

    [StringLength(20, ErrorMessage = "Mode cannot exceed 20 characters")]
    public string? Mode { get; set; }

    public List<CountStockItemDTO>? CountStockItems { get; set; }
}

public class AddCountStockDTO
{
    [Required(ErrorMessage = "IsDraft is required")]
    public bool IsDraft { get; set; }

    public string? Comment { get; set; }

    [Required(ErrorMessage = "Posting Date is required")]
    public DateTime PostingDate { get; set; }

    [StringLength(20, ErrorMessage = "Mode cannot exceed 20 characters")]
    public string? Mode { get; set; } = "Counting";

   
    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }

    public int? ReasonId { get; set; }
}

public class UpdateCountStockDTO
{
    [Required(ErrorMessage = "CountStockId is required")]
    public int CountStockId { get; set; }

    [Required(ErrorMessage = "Posting Date is required")]
    public DateTime PostingDate { get; set; }
    public string? Comment { get; set; }

    [StringLength(20, ErrorMessage = "Mode cannot exceed 20 characters")]
    public string? Mode { get; set; }

    public int? ReasonId { get; set; }
}

public class CountStockItemDTO
{
    public int CountStockItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } // GeneralItemStatus enum

    [StringLength(500, ErrorMessage = "Error Message cannot exceed 500 characters")]
    public string? ErrorMessage { get; set; }

    public int UoMEntry { get; set; }
    public string? BarCode { get; set; }
    public decimal? UnitPrice { get; set; }

    
    public decimal? VatPercent { get; set; }
    public decimal? VatAmount { get; set; }
    public decimal? LineTotalBeforeVat { get; set; }
    public decimal? LineTotalAfterVat { get; set; }

    public string? Comment { get; set; }

    [Required(ErrorMessage = "Count Stock ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Count Stock ID must be greater than 0")]
    public int CountStockId { get; set; }

    [Required(ErrorMessage = "Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Item ID must be greater than 0")]
    public int ItemId { get; set; }

    public bool IsBatchManaged { get; set; }
}

public class AddCountStockItemDTO
{
    [Required(ErrorMessage = "Unit is required")]
    public int UoMEntry { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Count Stock ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Count Stock ID must be greater than 0")]
    public int CountStockId { get; set; }

    [Required(ErrorMessage = "Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Item ID must be greater than 0")]
    public int ItemId { get; set; }
}

public class UpdateCountStockItemDTO
{
    [Required(ErrorMessage = "CountStockItemId is required")]
    public int CountStockItemId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0")]
    public decimal? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }
    public decimal? VatPercent { get; set; }
    public decimal? VatAmount { get; set; }
    public decimal? LineTotalBeforeVat { get; set; }
    public decimal? LineTotalAfterVat { get; set; }

    [Required(ErrorMessage = "Unit is required")]
    public int UoMEntry { get; set; }
}
public class AddCountStockItemCreateRequest
{
    public DynamicBarcodesDto? Barcode { get; set; }

    public AddCountStockItemDTO? Item { get; set; }
}
public class CountStockBatchDTO
{
    public int CountStockBatchId { get; set; }

    [Required(ErrorMessage = "Count Stock Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Count Stock Item ID must be greater than 0")]
    public int CountStockItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0")]
    public decimal Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }

    [StringLength(100, ErrorMessage = "Batch Number cannot exceed 100 characters")]
    public string? BatchNumber { get; set; }

    public DateTime? ExpiryDate { get; set; }
}
public class AddCountStockBatchDTO
{
    [Required(ErrorMessage = "Count Stock Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Count Stock Item ID must be greater than 0")]
    public int CountStockItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0")]
    public decimal Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }

    [StringLength(100, ErrorMessage = "Batch Number cannot exceed 100 characters")]
    public string? BatchNumber { get; set; }

    public DateTime? ExpiryDate { get; set; }
}
public class UpdateCountStockBatchDTO
{
    [Required(ErrorMessage = "CountStockBatchId is required")]
    public int CountStockBatchId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0")]
    public decimal Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }

    [StringLength(100, ErrorMessage = "Batch Number cannot exceed 100 characters")]
    public string? BatchNumber { get; set; }

    public DateTime? ExpiryDate { get; set; }
}
public class CountStockResponseDTO
{
    public int CountStockId { get; set; }
    public int? ReasonId { get; set; }
    public string? ReasonName { get; set; }
    public string Status { get; set; }
    public string? LiveStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Mode { get; set; }
    public string UserId { get; set; }
    public string? UserFullName { get; set; }
    public int WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public int ItemsCount { get; set; }
}
