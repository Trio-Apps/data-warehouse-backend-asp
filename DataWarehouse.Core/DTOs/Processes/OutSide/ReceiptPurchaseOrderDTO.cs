using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Domain.Entities.Actors;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes.OutSide;

public class ReceiptPurchaseOrderDTO : GeneralOrderDto
{
    public int ReceiptPurchaseOrderId { get; set; }

   


    [Required(ErrorMessage = "Purchase Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Purchase Order ID must be greater than 0")]
    public int PurchaseOrderId { get; set; }

   

    [Required(ErrorMessage = "Supplier ID is required")]
    public int SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public List<ReceiptPurchaseOrderItemDTO>? Items { get; set; }
}

public class AddReceiptPurchaseOrderDTO
{
    [Required(ErrorMessage = "Purchase Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Purchase Order ID must be greater than 0")]
    public int PurchaseOrderId { get; set; }

    [Required(ErrorMessage = "Posting Date is required")]
    public DateTime PostingDate { get; set; }

    [Required(ErrorMessage = "Due Date is required")]
    public DateTime DueDate { get; set; }

    public string? Comment { get; set; }

   // [Required(ErrorMessage = "Warehouse ID is required")]
   // [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    //public int WarehouseId { get; set; }

    public bool IsDraft { get; set; }

    //[Required(ErrorMessage = "Supplier ID is required")]
    //public int SupplierId { get; set; }
}

public class UpdateReceiptPurchaseOrderDTO
{
    public int ReceiptPurchaseOrderId { get; set; }

    [Required(ErrorMessage = "Posting Date is required")]
    public DateTime PostingDate { get; set; }

    [Required(ErrorMessage = "Due Date is required")]
    public DateTime DueDate { get; set; }

    public string? Comment { get; set; }
    public bool IsDraft { get; set; }
}

public class ReceiptPurchaseOrderItemDTO : GeneralItemDto
{
    public int ReceiptPurchaseOrderItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Unit of Measure is required")]
    public int UoMEntry { get; set; }

    public string? BarCode { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Unit Price must be zero or greater")]
    public decimal? UnitPrice { get; set; }

    public string? ErrorMessage { get; set; }

    public string? Comment { get; set; }

    public string? UnitName { get; set; }

    // Relations (IDs only ñ ’Õ ··‹ DTO)
    [Required(ErrorMessage = "ReceiptPurchaseOrderId is required")]
    public int ReceiptPurchaseOrderId { get; set; }

    [Required(ErrorMessage = "ItemId is required")]
    public int ItemId { get; set; }

   public Item? Item { get; set; }
    
    public List<ReceiptPurchaseOrderBatchDTO>? Batches { get; set; }

}

public class AddReceiptPurchaseOrderItemDTO
{
    [Required(ErrorMessage = "Unit is required")]
    public int UoMEntry { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Receipt Purchase Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Receipt Purchase Order ID must be greater than 0")]
    public int ReceiptPurchaseOrderId { get; set; }

    [Required(ErrorMessage = "Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Item ID must be greater than 0")]
    public int ItemId { get; set; }
}

public class UpdateReceiptPurchaseOrderItemDTO
{
    [Required(ErrorMessage = "ReceiptPurchaseOrderItemId is required")]
    public int ReceiptPurchaseOrderItemId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal? Quantity { get; set; }

    [Required(ErrorMessage = "Unit is required")]
    public int UoMEntry { get; set; }
}

public class AddReceiptPurchaseOrderItemCreateRequest
{
   
    public DynamicBarcodesDto? Barcode { get; set; }

    public AddGeneralItemDto? Item { get; set; }
}


public class ReceiptPurchaseOrderItemCommentDTO
{
    public int? ReceiptPurchaseOrderItemCommentId { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }
}

public class ReceiptPurchaseOrderResponseDTO
{
    public int ReceiptPurchaseOrderId { get; set; }
    public int PurchaseOrderId { get; set; }
    public string Status { get; set; }
    public string? LiveStatus { get; set; }
    public string UserId { get; set; }
    public string? UserFullName { get; set; }
    public int ItemsCount { get; set; }
}

public class ReceiptPurchaseOrderBatchDTO
{
    public int ReceiptPurchaseOrderBatchId { get; set; }

    [Required(ErrorMessage = "Receipt Purchase Order Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Receipt Purchase Order Item ID must be greater than 0")]
    public int ReceiptPurchaseOrderItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }

    [StringLength(100, ErrorMessage = "Batch Number cannot exceed 100 characters")]
    public string? BatchNumber { get; set; }

    public DateTime? ExpiryDate { get; set; }
}

public class AddReceiptPurchaseOrderBatchDTO
{
    [Required(ErrorMessage = "Receipt Purchase Order Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Receipt Purchase Order Item ID must be greater than 0")]
    public int ReceiptPurchaseOrderItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }

    public DateTime? ExpiryDate { get; set; }
}

public class UpdateReceiptPurchaseOrderBatchDTO
{
    [Required(ErrorMessage = "ReceiptPurchaseOrderBatchId is required")]
    public int ReceiptPurchaseOrderBatchId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }

    public DateTime? ExpiryDate { get; set; }
}
