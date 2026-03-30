using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Domain.Validations;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes;

public class TransferredRequestDTO : GeneralOrderDto
{
    public int TransferredRequestId { get; set; }
    public int? ReasonId { get; set; }
    public string? ReasonName { get; set; }


    public DateTime? PostingDate { get; set; }

    [Required(ErrorMessage = "Destination Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Destination Warehouse ID must be greater than 0")]
    public int DistinationWarehouseId { get; set; }

    public string? WarehouseName { get; set; }
    public string? DistinationWarehouseName { get; set; }

    public int? TransferredStockId { get; set; }
    public List<int>? TransferredStockIds { get; set; }
    public List<TransferredRequestItemDTO>? Items { get; set; }
}

public class AddTransferredRequestDTO : AddGeneralOrderDto
{

    [NotFutureDate]
    public DateTime? PostingDate { get; set; }

    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }

    [Required(ErrorMessage = "Destination Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Destination Warehouse ID must be greater than 0")]
    public int DistinationWarehouseId { get; set; }
    public int? ReasonId { get; set; }
}

public class UpdateTransferredRequestDTO : UpdateGeneralOrderDto
{
    [Required(ErrorMessage = "TransferredRequestId is required")]
    public int TransferredRequestId { get; set; }

  

    [Required(ErrorMessage = "Destination Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Destination Warehouse ID must be greater than 0")]
    public int DistinationWarehouseId { get; set; }
    public int? ReasonId { get; set; }

}

public class TransferredRequestItemDTO  : GeneralItemDto
{
    public int TransferredRequestItemId { get; set; }


    [Required(ErrorMessage = "Transferred Request ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Transferred Request ID must be greater than 0")]
    public int TransferredRequestId { get; set; }

    public decimal ExecuteQuantity { get; set; }

   
}

public class AddTransferredRequestItemDTO : AddGeneralItemDto
{


    [Required(ErrorMessage = "Transferred Request ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Transferred Request ID must be greater than 0")]
    public int TransferredRequestId { get; set; }


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
    public decimal? VatPercent { get; set; }
    public decimal? VatAmount { get; set; }
    public decimal? LineTotalBeforeVat { get; set; }
    public decimal? LineTotalAfterVat { get; set; }
}

public class AddTransferredRequestItemCreateRequest
{
    public DynamicBarcodesDto? Barcode { get; set; }
    public AddGeneralItemDto? Item { get; set; }
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
