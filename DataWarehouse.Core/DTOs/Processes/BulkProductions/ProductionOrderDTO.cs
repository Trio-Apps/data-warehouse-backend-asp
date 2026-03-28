using DataWarehouse.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes.BulkProductions;

public class ProductionOrderDTO
{
    public int ProductionOrderId { get; set; }
    public int? ReasonId { get; set; }
    public string? ReasonName { get; set; }

    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } // ProductionStatus enum

    [Required(ErrorMessage = "Posting Date is required")]
    public DateTime PostingDate { get; set; }

    [Required(ErrorMessage = "Due Date is required")]
    public DateTime DueDate { get; set; }

    [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
    public string? Remarks { get; set; }

    [Required(ErrorMessage = "User ID is required")]
    public  string UserId { get; set; }

    public ApplicationUser User { get; set; }

    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }

    public int NumberOfProductionItem {  get; set; }
    public bool Approval { get; set; }
    public string? ApprovalStatus { get; set; }
    public List<ProductionOrderItemDTO>? ProductionOrderItems { get; set; }
}
public class AddProductionOrderDTO
{

    [Required(ErrorMessage = "Posting Date is required")]
    public DateTime PostingDate { get; set; }
    [Required(ErrorMessage = "Due Date is required")]
    public DateTime DueDate { get; set; }

    [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
    public string? Remarks { get; set; }

    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }
    public int? ReasonId { get; set; }

}

public class UpdateProductionOrderDTO
{
    public int ProductionOrderId { get; set; }
    public int? ReasonId { get; set; }

    [Required(ErrorMessage = "Posting Date is required")]
    public DateTime PostingDate { get; set; }

    [Required(ErrorMessage = "Due Date is required")]
    public DateTime DueDate { get; set; }

    [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
    public string? Remarks { get; set; }


 
}

public class ProductionOrderReportFilterDto
{
    [Required(ErrorMessage = "WarehouseId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "WarehouseId must be greater than 0")]
    public int WarehouseId { get; set; }

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Status { get; set; }
    public string? SearchTerm { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than 0")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 500, ErrorMessage = "PageSize must be between 1 and 500")]
    public int PageSize { get; set; } = 20;
}

public class ProductionOrderReportItemDto
{
    public int ProductionOrderId { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public int NumberOfProductionItems { get; set; }
    public decimal TotalPlannedQuantity { get; set; }
    public decimal TotalProducedQuantity { get; set; }
    public bool Approval { get; set; }
    public string? ApprovalStatus { get; set; }
}
