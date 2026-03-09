using DataWarehouse.Core.DTOs.BarCode;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes;

public class TransferredRequestDTO : GeneralOrderDto
{
    public int TransferredRequestId { get; set; }

  

  
    [Required(ErrorMessage = "Destination Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Destination Warehouse ID must be greater than 0")]
    public int DistinationWarehouseId { get; set; }

    public string? WarehouseName { get; set; }
    public string? DistinationWarehouseName { get; set; }

    public int? TransferredStockId { get; set; }
    public List<TransferredRequestItemDTO>? Items { get; set; }
}

public class AddTransferredRequestDTO
{
    [Required(ErrorMessage = "Due Date is required")]
    public DateTime DueDate { get; set; }

    public string? Comment { get; set; }

    [Required(ErrorMessage = "IsDraft is required")]
    public bool IsDraft { get; set; }

    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }

    [Required(ErrorMessage = "Destination Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Destination Warehouse ID must be greater than 0")]
    public int DistinationWarehouseId { get; set; }
}

public class UpdateTransferredRequestDTO
{
    [Required(ErrorMessage = "TransferredRequestId is required")]
    public int TransferredRequestId { get; set; }

    [Required(ErrorMessage = "Due Date is required")]
    public DateTime DueDate { get; set; }

    public string? Comment { get; set; }

    [Required(ErrorMessage = "Destination Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Destination Warehouse ID must be greater than 0")]
    public int DistinationWarehouseId { get; set; }

    public bool IsDraft { get; set; }
}

public class TransferredRequestItemDTO
{
    public int TransferredRequestItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; }

    public string? ErrorMessage { get; set; }
    public int UoMEntry { get; set; }
    public string? BarCode { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Comment { get; set; }

    [Required(ErrorMessage = "Transferred Request ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Transferred Request ID must be greater than 0")]
    public int TransferredRequestId { get; set; }

    [Required(ErrorMessage = "Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Item ID must be greater than 0")]
    public int ItemId { get; set; }

    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public string? UnitName { get; set; }
}

public class AddTransferredRequestItemDTO
{
    [Required(ErrorMessage = "Unit is required")]
    public int UoMEntry { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Transferred Request ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Transferred Request ID must be greater than 0")]
    public int TransferredRequestId { get; set; }

    [Required(ErrorMessage = "Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Item ID must be greater than 0")]
    public int ItemId { get; set; }

    public decimal? UnitPrice { get; set; }
}

public class UpdateTransferredRequestItemDTO
{
    [Required(ErrorMessage = "TransferredRequestItemId is required")]
    public int TransferredRequestItemId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal? Quantity { get; set; }

    [Required(ErrorMessage = "Unit is required")]
    public int UoMEntry { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Planned Quantity must be greater than 0")]
    public decimal? UnitPrice { get; set; }
}

public class AddTransferredRequestItemCreateRequest
{
    public DynamicBarcodesDto? Barcode { get; set; }
    public AddTransferredRequestItemDTO? Item { get; set; }
}

public class TransferredRequestBatchDTO
{
    public int TransferredRequestBatchId { get; set; }

    [Required(ErrorMessage = "Transferred Request Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Transferred Request Item ID must be greater than 0")]
    public int TransferredRequestItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    public string? Comment { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class AddTransferredRequestBatchDTO
{
    [Required(ErrorMessage = "Transferred Request Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Transferred Request Item ID must be greater than 0")]
    public int TransferredRequestItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    public string? Comment { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class UpdateTransferredRequestBatchDTO
{
    [Required(ErrorMessage = "TransferredRequestBatchId is required")]
    public int TransferredRequestBatchId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal? Quantity { get; set; }

    public string? Comment { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
