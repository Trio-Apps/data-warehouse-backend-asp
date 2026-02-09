using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes.PurchaseOrders;

public class PurchaseOrderDTO : GeneralOrderDto
{
    public int PurchaseOrderId { get; set; }

    [Required(ErrorMessage = "Posting Date is required")]
    public DateTime PostingDate { get; set; }

    [Required(ErrorMessage = "Due Date is required")]
    public DateTime DueDate { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedDate { get; set; } 
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; }
    public string? SupplierName { get; set; }
   
    public bool IsReceipt { get; set; } 

    public int? ReceiptOrderId { get; set; }

    public bool IsReturn {  get; set; }

    public int? ReturnOrderId { get; set; }


    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }

    
    public List<PurchaseOrderItemDTO>? PurchaseOrderItems { get; set; }
}
public class AddPurchaseOrderDTO
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
public class UpdatePurchaseOrderDTO
{
    public int PurchaseOrderId { get; set; }

    public DateTime? PostingDate { get; set; }

    public DateTime? DueDate { get; set; }

    public int? SupplierId { get; set; }
    public bool IsDraft { get; set; }
}
