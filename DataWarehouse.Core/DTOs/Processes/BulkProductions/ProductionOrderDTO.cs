using DataWarehouse.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes.BulkProductions;

public class ProductionOrderDTO
{
    public int ProductionOrderId { get; set; }

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

}

public class UpdateProductionOrderDTO
{
    public int ProductionOrderId { get; set; }

    [Required(ErrorMessage = "Posting Date is required")]
    public DateTime PostingDate { get; set; }

    [Required(ErrorMessage = "Due Date is required")]
    public DateTime DueDate { get; set; }

    [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
    public string? Remarks { get; set; }

 

}
