using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Domain.Entities.Actors;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes.OutSide;

public class SalesOrderDTO : GeneralOrderDto
{
    public int SalesOrderId { get; set; }
    public int? ReasonId { get; set; }
    public string? ReasonName { get; set; }
    [Required(ErrorMessage = "Customer ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Customer ID must be greater than 0")]
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public Customer Customer { get; set; }

    public List<SalesOrderItemDTO>? SalesOrderItems { get; set; }
}

public class AddSalesOrderDTO : AddGeneralOrderDto
{
  

    [Required(ErrorMessage = "Customer is required")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }
    public int? ReasonId { get; set; }
}

public class UpdateSalesOrderDTO : UpdateGeneralOrderDto
{
    public int SalesOrderId { get; set; }
    [Required(ErrorMessage = "Customer ID is required")]
    public int? CustomerId { get; set; }
    public int? ReasonId { get; set; }
 
}

public class SalesOrderItemDTO : GeneralItemDto
{ 
    public int SalesOrderItemId { get; set; }

    [Required(ErrorMessage = "Sales Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Sales Order ID must be greater than 0")]
    public int SalesOrderId { get; set; }

    public decimal ExecuteQuantity { get; set; }


}

public class AddSalesOrderItemDTO
{
    [Required(ErrorMessage = "Unit is required")]
    public int UoMEntry { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Planned Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    //[Required(ErrorMessage = "Sales Order ID is required")]
    //[Range(1, int.MaxValue, ErrorMessage = "Sales Order ID must be greater than 0")]
    //public int SalesOrderId { get; set; }

    [Required(ErrorMessage = "Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Item ID must be greater than 0")]
    public int ItemId { get; set; }
}

public class UpdateSalesOrderItemDTO
{
    [Required(ErrorMessage = "SalesOrderItemId is required")]
    public int SalesOrderItemId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Planned Quantity must be greater than 0")]
    public decimal? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }
    public decimal? VatPercent { get; set; }
    public decimal? VatAmount { get; set; }
    public decimal? LineTotalBeforeVat { get; set; }
    public decimal? LineTotalAfterVat { get; set; }

    [Required(ErrorMessage = "Unit is required")]
    public int UoMEntry { get; set; }
}

public class AddSalesOrderItemCreateRequest
{
    public DynamicBarcodesDto? Barcode { get; set; }

    public AddGeneralItemDto? Item { get; set; }
}

public class SalesOrderResponseDTO
{
    public int SalesOrderId { get; set; }
    public int? ReasonId { get; set; }
    public string? ReasonName { get; set; }
    public decimal TotalSalesPrice { get; set; }
    public string Status { get; set; }
    public string? LiveStatus { get; set; }
    public DateTime CreatedDate { get; set; }
    public string UserId { get; set; }
    public string? UserFullName { get; set; }
    public int WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int ItemsCount { get; set; }
}

public class SalesOrderBatchDTO
{
    public int SalesOrderBatchId { get; set; }

    [Required(ErrorMessage = "Sales Order Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Sales Order Item ID must be greater than 0")]
    public int SalesOrderItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }

    [StringLength(100, ErrorMessage = "Batch Number cannot exceed 100 characters")]
    public string? BatchNumber { get; set; }

    public DateTime? ExpiryDate { get; set; }
}

public class AddSalesOrderBatchDTO
{
    [Required(ErrorMessage = "Sales Order Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Sales Order Item ID must be greater than 0")]
    public int SalesOrderItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }


    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }

    public DateTime? ExpiryDate { get; set; }
}

public class UpdateSalesOrderBatchDTO
{
    [Required(ErrorMessage = "SalesOrderBatchId is required")]
    public int SalesOrderBatchId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }
    
    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
}
