using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes.OutSide;

// 1. Main DTO for Retrieval
public class DeliveryNoteOrderDTO : GeneralOrderDto
{
    public int DeliveryNoteOrderId { get; set; }

    [Required(ErrorMessage = "Customer ID is required")]
    public int CustomerId { get; set; }

    public string? CustomerName { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Sales Order ID must be greater than 0")]
    public int? SalesOrderId { get; set; } // Reference to Parent (Sales Order)

    public List<DeliveryNoteItemDTO>? Items { get; set; }
}

// 2. Add DTO (With Reference to Sales Order)
public class AddDeliveryNoteOrderDTO : AddGeneralOrderDto
{
    [Required(ErrorMessage = "Sales Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Sales Order ID must be greater than 0")]
    public int SalesOrderId { get; set; }

    [Required(ErrorMessage = "Posting Date is required")]
    public DateTime PostingDate { get; set; }

  

}

// 3. Add DTO (Direct - Without Sales Order Reference)
public class AddDeliveryNoteOrderWithoutRefDTO : AddGeneralOrderDto
{
    [Required(ErrorMessage = "Posting Date is required")]
    public DateTime PostingDate { get; set; }

  

    [Required(ErrorMessage = "Customer Id is required")]
    public int CustomerId { get; set; }

   

    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }
}

// 4. Update DTO
public class UpdateDeliveryNoteOrderDTO
{
    [Required(ErrorMessage = "DeliveryNoteOrderId is required")]
    public int DeliveryNoteOrderId { get; set; }

    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Comment { get; set; }
    public int? CustomerId { get; set; }
    public bool IsDraft { get; set; }
}

// 5. Item DTO
public class DeliveryNoteItemDTO : GeneralItemDto
{
    public int DeliveryNoteItemId { get; set; }

    [Required(ErrorMessage = "DeliveryNoteOrderId is required")]
    public int DeliveryNoteOrderId { get; set; }

    public int? SalesOrderItemId { get; set; } // Link to Parent Item

    public List<DeliveryNoteBatchDTO>? Batches { get; set; }
}

// 6. Add Item DTO
public class AddDeliveryNoteOrderItemDTO
{
    [Required(ErrorMessage = "Sales Order Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Sales Order Item ID must be greater than 0")]
    public int SalesOrderItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }
}

// 7. Update Item DTO
public class UpdateDeliveryNoteOrderItemDTO : UpdateGeneralItemDto
{
    [Required(ErrorMessage = "DeliveryNoteItemId is required")]
    public int DeliveryNoteItemId { get; set; }
}

// 8. Batch DTO
public class DeliveryNoteBatchDTO : GeneralBatchDto
{
    public int DeliveryNoteBatchId { get; set; }

    [Required(ErrorMessage = "Delivery Note Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Delivery Note Item ID must be greater than 0")]
    public int DeliveryNoteItemId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Sales Order Batch ID must be greater than 0")]
    public int? SalesOrderBatchId { get; set; } // Link to Parent Batch
}

// 9. Add Batch DTO
public class AddDeliveryNoteOrderBatchDTO : AddGeneralBatchDto
{
    [Required(ErrorMessage = "Delivery Note Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Delivery Note Item ID must be greater than 0")]
    public int DeliveryNoteItemId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Sales Order Batch ID must be greater than 0")]
    public int? SalesOrderBatchId { get; set; }
}

// 10. Update Batch DTO
public class UpdateDeliveryNoteOrderBatchDTO : UpdateGeneralBatchDto
{
    [Required(ErrorMessage = "DeliveryNoteBatchId is required")]
    public int DeliveryNoteBatchId { get; set; }
}