using DataWarehouse.Domain.Entities.Actors;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes.OutSide;

public class GoodsReturnOrderDTO : GeneralOrderDto
{
    public int GoodsReturnOrderId { get; set; }


    [Required(ErrorMessage = "Supplier ID is required")]
    public int SupplierId { get; set; }

    public string? SupplierName { get; set; }
    public string? SupplierCode { get; set; }

    [Required(ErrorMessage = "Receipt Purchase Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Receipt Purchase Order ID must be greater than 0")]
    public int? ReceiptPurchaseOrderId { get; set; }

   


    public List<GoodsReturnOrderItemDTO>? Items { get; set; }
}

public class AddGoodsReturnOrderModel
{
    [Required(ErrorMessage = "Receipt Purchase Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Receipt Purchase Order ID must be greater than 0")]
    public int ReceiptPurchaseOrderId { get; set; }

    public string? Comment { get; set; }
}

public class AddGoodsReturnOrderDTO : AddGeneralOrderDto
{
    [Required(ErrorMessage = "Receipt Purchase Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Receipt Purchase Order ID must be greater than 0")]
    public int ReceiptPurchaseOrderId { get; set; }

    [Required(ErrorMessage = "PostingDate is required")]
    public DateTime PostingDate { get; set; }

}

public class AddGoodsReturnOrderWithoutRefDTO
{

    [Required(ErrorMessage = "Posting Date is required")]

    public DateTime PostingDate { get; set; }
    [Required(ErrorMessage = "Due Date is required")]
    public DateTime DueDate { get; set; }
    public string? Comment { get; set; }

    [Required(ErrorMessage = "Supplier is required")]
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "IsDraft is required")]
    public bool IsDraft { get; set; }

    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }
}

public class UpdateGoodsReturnOrderDTO
{
    [Required(ErrorMessage = "GoodsReturnOrderId is required")]
    public int GoodsReturnOrderId { get; set; }
    public string? Comment { get; set; }
    public DateTime? PostingDate { get; set; }

    public DateTime? DueDate { get; set; }

    public int? SupplierId { get; set; }
    public bool IsDraft { get; set; }

}

public class GoodsReturnOrderItemDTO
{
    public int GoodsReturnOrderItemId { get; set; }

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

    [Required(ErrorMessage = "GoodsReturnOrderId is required")]
    public int GoodsReturnOrderId { get; set; }

    [Required(ErrorMessage = "ReceiptPurchaseOrderItemId is required")]
    public int? ReceiptPurchaseOrderItemId { get; set; }

    [Required(ErrorMessage = "ItemId is required")]
    public int ItemId { get; set; }
    public Item? Item { get; set; }

    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }

    


    public List<GoodsReturnOrderBatchDTO>? Batches { get; set; }
}

public class AddGoodsReturnOrderItemDTO
{
    [Required(ErrorMessage = "Receipt Purchase Order Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Receipt Purchase Order Item ID must be greater than 0")]
    public int ReceiptPurchaseOrderItemId { get; set; }

    //[Required(ErrorMessage = "Receipt Purchase Order  ID is required")]
    //[Range(1, int.MaxValue, ErrorMessage = "Receipt Purchase Order ID must be greater than 0")]
    //public int ReceiptPurchaseOrderId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }
}

public class UpdateGoodsReturnOrderItemDTO
{
    [Required(ErrorMessage = "GoodsReturnOrderItemId is required")]
    public int GoodsReturnOrderItemId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal? Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }
}


public class AddGoodsReturnOrderItemWithoutRefDTO : AddGeneralItemDto
{
    [Required(ErrorMessage = "GoodsReturnOrderItemId is required")]
    public int GoodsReturnOrderItemId { get; set; }

}

public class UpdateGoodsReturnOrderItemWithoutRefDTO : UpdateGeneralItemDto
{
    [Required(ErrorMessage = "ReceiptPurchaseOrderItemId is required")]
    public int ReceiptPurchaseOrderItemId { get; set; }

}
public class GoodsReturnOrderBatchDTO
{
    public int GoodsReturnOrderBatchId { get; set; }

    [Required(ErrorMessage = "Goods Return Order Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Goods Return Order Item ID must be greater than 0")]
    public int GoodsReturnOrderItemId { get; set; }

    [Required(ErrorMessage = "Receipt Purchase Order Batch ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Receipt Purchase Order Batch ID must be greater than 0")]
    public int? ReceiptPurchaseOrderBatchId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }

    [StringLength(100, ErrorMessage = "Batch Number cannot exceed 100 characters")]
    public string? BatchNumber { get; set; }

    public DateTime? ExpiryDate { get; set; }
}

public class AddGoodsReturnOrderBatchDTO
{
    [Required(ErrorMessage = "Goods Return Order Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Goods Return Order Item ID must be greater than 0")]
    public int GoodsReturnOrderItemId { get; set; }

//    [Required(ErrorMessage = "Receipt Purchase Order Batch ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Receipt Purchase Order Batch ID must be greater than 0")]
    public int? ReceiptPurchaseOrderBatchId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }
}

public class UpdateGoodsReturnOrderBatchDTO
{
    [Required(ErrorMessage = "GoodsReturnOrderBatchId is required")]
    public int GoodsReturnOrderBatchId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }
}

