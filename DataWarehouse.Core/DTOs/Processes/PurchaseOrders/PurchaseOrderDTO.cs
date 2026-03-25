using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Validations;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes.PurchaseOrders;

public class PurchaseOrderDTO : GeneralOrderDto
{
    public int PurchaseOrderId { get; set; }


    public string? Comment { get; set; }
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierCode { get; set; }

    public bool IsReceipt { get; set; } 

    public int? ReceiptOrderId { get; set; }

    public bool IsReturn {  get; set; }

    public int? ReturnOrderId { get; set; }


    
    public List<PurchaseOrderItemDTO>? PurchaseOrderItems { get; set; }
}
public class AddPurchaseOrderDTO : AddGeneralOrderDto
{

    [Required(ErrorMessage = "Posting Date is required")]
    [NotFutureDate]
    public DateTime PostingDate { get; set; }
  

    [Required(ErrorMessage = "Supplier is required")]
    public int SupplierId { get; set; }

 

    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }

    // Attachments
 //   public List<IFormFile>? Attachments { get; set; } = new();


}
public class UpdatePurchaseOrderDTO : UpdateGeneralOrderDto
{
    public int PurchaseOrderId { get; set; }
    public int? SupplierId { get; set; }
}
