using DataWarehouse.Domain.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes.OutSide;

public class SalesReturnOrderDTO : GeneralOrderDto
{
    public int SalesReturnOrderId { get; set; }
    public int? ReasonId { get; set; }
    public string? ReasonName { get; set; }

    [Required(ErrorMessage = "Customer ID is required")]
    public int CustomerId { get; set; }

    public string? CustomerName { get; set; }

   // [Required(ErrorMessage = "Sales Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Sales Order ID must be greater than 0")]
    public int? DeliveryNoteOrderId { get; set; }

 


    public List<SalesReturnOrderItemDTO>? Items { get; set; }
}

public class AddSalesReturnOrderDTO
{
    [Required(ErrorMessage = "Delivery Note Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Sales Order ID must be greater than 0")]
    public int DeliveryNoteOrderId { get; set; }
    [Required(ErrorMessage = "Posting Date is required")]
    [NotFutureDate]
    public DateTime PostingDate { get; set; }
    [Required(ErrorMessage = "Due Date is required")]
    public DateTime DueDate { get; set; }

    public string? Comment { get; set; }

    public bool IsDraft { get; set; }
    public int? ReasonId { get; set; }
}

public class AddSalesReturnOrderWithoutRefDTO
{
    [Required(ErrorMessage = "Posting Date is required")]
    [NotFutureDate]
    public DateTime PostingDate { get; set; }
    [Required(ErrorMessage = "Due Date is required")]
    public DateTime DueDate { get; set; }
    public string? Comment { get; set; }

    [Required(ErrorMessage = "Customer Id is required")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "IsDraft is required")]
    public bool IsDraft { get; set; }

    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }
    public int? ReasonId { get; set; }
}

public class UpdateSalesReturnOrderDTO
{
    [Required(ErrorMessage = "SalesReturnOrderId is required")]
    public int SalesReturnOrderId { get; set; }
    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Comment { get; set; }

    public int? CustomerId { get; set; }

    public bool IsDraft { get; set; }
    public int? ReasonId { get; set; }

}

public class SalesReturnOrderItemDTO : GeneralItemDto
{
    public int SalesReturnOrderItemId { get; set; }   

    [Required(ErrorMessage = "SalesReturnOrderId is required")]
    public int SalesReturnOrderId { get; set; }

   // [Required(ErrorMessage = "SalesOrderItemId is required")]
    public int? DeliveryNoteItemId { get; set; }

    public List<SalesReturnOrderBatchDTO>? Batches { get; set; }
}
public class AddSalesReturnOrderItemDTO
{
    [Required(ErrorMessage = "Delivery Note Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Sales Order Item ID must be greater than 0")]
    public int DeliveryNoteItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }


    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }
}

public class UpdateSalesReturnOrderItemDTO : UpdateGeneralItemDto
{
    [Required(ErrorMessage = "SalesReturnOrderItemId is required")]
    public int SalesReturnOrderItemId { get; set; }
}

public class SalesReturnOrderBatchDTO : GeneralBatchDto
{
    public int SalesReturnOrderBatchId { get; set; }

    [Required(ErrorMessage = "Sales Return Order Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Sales Return Order Item ID must be greater than 0")]
    public int SalesReturnOrderItemId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Sales Order Batch ID must be greater than 0")]
    public int? DeliveryNoteBatchId { get; set; }

}

public class AddSalesReturnOrderBatchDTO : AddGeneralBatchDto
{
    [Required(ErrorMessage = "Sales Return Order Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Sales Return Order Item ID must be greater than 0")]
    public int SalesReturnOrderItemId { get; set; }

   // [Required(ErrorMessage = "Sales Order Batch ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Sales Order Batch ID must be greater than 0")]
    public int? DeliveryNoteBatchId { get; set; }

}

public class UpdateSalesReturnOrderBatchDTO : UpdateGeneralBatchDto
{
    [Required(ErrorMessage = "SalesReturnOrderBatchId is required")]
    public int SalesReturnOrderBatchId { get; set; }

 
}

