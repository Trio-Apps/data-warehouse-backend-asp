using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes.OutSide;

public class SalesReturnOrderDTO : GeneralOrderDto
{
    public int SalesReturnOrderId { get; set; }

    [Required(ErrorMessage = "Customer ID is required")]
    public int CustomerId { get; set; }

    public string? CustomerName { get; set; }

    [Required(ErrorMessage = "Sales Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Sales Order ID must be greater than 0")]
    public int SalesOrderId { get; set; }

 


    public List<SalesReturnOrderItemDTO>? Items { get; set; }
}

public class AddSalesReturnOrderDTO
{
    [Required(ErrorMessage = "Sales Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Sales Order ID must be greater than 0")]
    public int SalesOrderId { get; set; }

    public string? Comment { get; set; }
}

public class UpdateSalesReturnOrderDTO
{
    [Required(ErrorMessage = "SalesReturnOrderId is required")]
    public int SalesReturnOrderId { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }

    public bool IsDraft { get; set; }
}

public class SalesReturnOrderItemDTO : GeneralItemDto
{
    public int SalesReturnOrderItemId { get; set; }   

    [Required(ErrorMessage = "SalesReturnOrderId is required")]
    public int SalesReturnOrderId { get; set; }

    [Required(ErrorMessage = "SalesOrderItemId is required")]
    public int SalesOrderItemId { get; set; }

    public List<SalesReturnOrderBatchDTO>? Batches { get; set; }
}
public class AddSalesReturnOrderItemDTO
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

    [Required(ErrorMessage = "Sales Order Batch ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Sales Order Batch ID must be greater than 0")]
    public int SalesOrderBatchId { get; set; }

}

public class AddSalesReturnOrderBatchDTO : AddGeneralBatchDto
{
    [Required(ErrorMessage = "Sales Return Order Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Sales Return Order Item ID must be greater than 0")]
    public int SalesReturnOrderItemId { get; set; }

    [Required(ErrorMessage = "Sales Order Batch ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Sales Order Batch ID must be greater than 0")]
    public int SalesOrderBatchId { get; set; }

}

public class UpdateSalesReturnOrderBatchDTO : UpdateGeneralBatchDto
{
    [Required(ErrorMessage = "SalesReturnOrderBatchId is required")]
    public int SalesReturnOrderBatchId { get; set; }

 
}

