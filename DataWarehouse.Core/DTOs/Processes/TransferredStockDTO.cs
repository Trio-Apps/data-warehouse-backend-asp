using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Domain.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes;

public class TransferredStockDTO : GeneralOrderDto
{
    public int TransferredStockId { get; set; }
    public int? ReasonId { get; set; }
    public string? ReasonName { get; set; }

    public string? Reference { get; set; }
    public string? ReceivingStatus { get; set; }


    public DateTime? PostingDate { get; set; }


    [Required(ErrorMessage = "Destination Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Destination Warehouse ID must be greater than 0")]
    public int DistinationWarehouseId { get; set; }

    public int? TransferredRequestId { get; set; }
    public bool? IsReceived { get; set; }
    public int? ReceivedStockId { get; set; }
    public string? DistinationWarehouseName { get; set; }


    public List<TransferredItemDTO>? TransferredItems { get; set; }
}

public class AddTransferredStockDTO : AddGeneralOrderDto
{
    [NotFutureDate]
    public DateTime? PostingDate { get; set; }

    public string? Reference { get; set; }

  
    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }

    [Required(ErrorMessage = "Destination Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Destination Warehouse ID must be greater than 0")]
    public int DistinationWarehouseId { get; set; }


    [Required(ErrorMessage = "TransferredRequestId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "TransferredRequestId must be greater than 0")]
    public int TransferredRequestId { get; set; }
    public int? ReasonId { get; set; }

    
}


public class AddTransferredStockWithoutRefDTO : AddGeneralOrderDto
{
  
    public string? Reference { get; set; }

 

    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }

    [Required(ErrorMessage = "Destination Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Destination Warehouse ID must be greater than 0")]
    public int DistinationWarehouseId { get; set; }
    public int? ReasonId { get; set; }
}

public class UpdateTransferredStockDTO : UpdateGeneralOrderDto
{
    public int TransferredStockId { get; set; }
    public int? ReasonId { get; set; }

  
    public string? Reference { get; set; }


    [Range(1, int.MaxValue, ErrorMessage = "Destination Warehouse ID must be greater than 0")]
    public int? DistinationWarehouseId { get; set; }

}

public class TransferredItemDTO : GeneralItemDto
{
    public int TransferredItemId { get; set; }

    public decimal? ReceivedQuantity { get; set; }

  

    [Required(ErrorMessage = "Transferred Stock ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Transferred Stock ID must be greater than 0")]
    public int TransferredStockId { get; set; }

   

    public int? TransferredRequestItemId { get; set; }
 
    public List<TransferredStockBatchDTO>? Batches { get; set; }
}

public class AddTransferredItemDTO
{
    [Required(ErrorMessage = "Unit is required")]
    public int UoMEntry { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal? Quantity { get; set; }

    [Required(ErrorMessage = "Transferred Stock ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Transferred Stock ID must be greater than 0")]
    public int TransferredStockId { get; set; }

    [Required(ErrorMessage = "Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Item ID must be greater than 0")]
    public int ItemId { get; set; }

    [Required(ErrorMessage = "TransferredRequestItemId is required")]

    public int TransferredRequestItemId { get; set; }  
}

public class UpdateTransferredItemDTO
{
    [Required(ErrorMessage = "TransferredItemId is required")]
    public int TransferredItemId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal? Quantity { get; set; }

    [Required(ErrorMessage = "Unit is required")]
    public int UoMEntry { get; set; }
}

public class AddTransferredItemCreateRequest
{
    public DynamicBarcodesDto? Barcode { get; set; }

    public AddGeneralItemDto? Item { get; set; }
}

public class TransferredStockBatchDTO
{
    public int TransferredStockBatchId { get; set; }

    [Required(ErrorMessage = "Transferred Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Transferred Item ID must be greater than 0")]
    public int TransferredItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }
    public decimal? ReceivedQuantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }

    [StringLength(100, ErrorMessage = "Batch Number cannot exceed 100 characters")]
    public string? BatchNumber { get; set; }

    public DateTime? ExpiryDate { get; set; }
}

public class AddTransferredStockBatchDTO
{
    [Required(ErrorMessage = "Transferred Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Transferred Item ID must be greater than 0")]
    public int TransferredItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }

    [StringLength(100, ErrorMessage = "Batch Number cannot exceed 100 characters")]
    public string? BatchNumber { get; set; }

    public DateTime? ExpiryDate { get; set; }
}

public class UpdateTransferredStockBatchDTO
{
    [Required(ErrorMessage = "TransferredStockBatchId is required")]
    public int TransferredStockBatchId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }

    public string? BatchNumber { get; set; }

    public DateTime? ExpiryDate { get; set; }
}

public class TransferredStockResponseDTO
{
    public int TransferredStockId { get; set; }
    public int? ReasonId { get; set; }
    public string? ReasonName { get; set; }
    public string Status { get; set; }
    public string? ReceivingStatus { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserId { get; set; }
    public string? Reference { get; set; }
    public string? UserFullName { get; set; }
    public int WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public int DistinationWarehouseId { get; set; }
    public string? DistinationWarehouseName { get; set; }
    public int ItemsCount { get; set; }
}
