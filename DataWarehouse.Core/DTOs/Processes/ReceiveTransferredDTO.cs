using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes;

public class ReceiveTransferredStockDTO
{
    [Required(ErrorMessage = "TransferredStockId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "TransferredStockId must be greater than 0")]
    public int TransferredStockId { get; set; }

    [Required(ErrorMessage = "IsDraft is required")]
    public bool IsDraft { get; set; }

    [Required(ErrorMessage = "Items are required")]
    public List<ReceiveTransferredItemDTO> Items { get; set; } = new();
}

public class ReceiveTransferredItemDTO
{
    [Required(ErrorMessage = "TransferredItemId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "TransferredItemId must be greater than 0")]
    public int TransferredItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    public List<ReceiveTransferredBatchDTO>? Batches { get; set; }
}

public class ReceiveTransferredBatchDTO
{
    [Required(ErrorMessage = "TransferredStockBatchId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "TransferredStockBatchId must be greater than 0")]
    public int TransferredStockBatchId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }
}
