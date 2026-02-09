using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes.PurchaseOrders;

public class PurchaseOrderItemDTO
{
    public int PurchaseOrderItemId { get; set; }

    [Required(ErrorMessage = "Planned Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Planned Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } // PurchaseItemStatus enum

    [StringLength(500, ErrorMessage = "Error Message cannot exceed 500 characters")]
    public string? ErrorMessage { get; set; }

    public int UoMEntry { get; set; }
    public string? UnitName { get; set; }
    public string? BarCode { get; set; }

    public decimal? UnitPrice { get; set; }
    // Navigation

    [Required(ErrorMessage = "Purchase Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Purchase Order ID must be greater than 0")]
    public int PurchaseOrderId { get; set; }

    [Required(ErrorMessage = "Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Item ID must be greater than 0")]
    public int ItemId { get; set; }

    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    // public List<PurchaseReceiptDTO>? PurchaseReceipts { get; set; }
}

public class PurchaseOrderItemStatusDTO
{
    public string? PerantStatus { get; set; }
    public ICollection<PurchaseOrderItemDTO> items {  get; set; }

}
public class AddPurchaseOrderItemDTO
{
   
    [Required(ErrorMessage = "Unit is required")]
    public int UoMEntry { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Planned Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Purchase Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Purchase Order ID must be greater than 0")]
    public int PurchaseOrderId { get; set; }

    [Required(ErrorMessage = "Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Item ID must be greater than 0")]
    public int ItemId { get; set; }


}

public class UpdatePurchaseOrderItemDTO
{
    [Required(ErrorMessage = "PurchaseOrderItemId is required")]
    public int PurchaseOrderItemId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Planned Quantity must be greater than 0")]
    public decimal? Quantity { get; set; }

    [Required(ErrorMessage = "Unit is required")]
    public int UoMEntry { get; set; }
}
public class AddPurchaseOrderItemCreateRequest
{
   // public StaticBarcodesDto? StaticBarcodes { get; set; }
    public DynamicBarcodesDto? Barcode { get; set; }

    public AddPurchaseOrderItemDTO? Item { get; set; }
}


