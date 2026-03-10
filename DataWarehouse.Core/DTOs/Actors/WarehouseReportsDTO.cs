using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Actors;

public class TransactionReportFilterDto
{
    [Required(ErrorMessage = "WarehouseId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "WarehouseId must be greater than 0")]
    public int WarehouseId { get; set; }

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? TransactionType { get; set; }
    public string? ItemCodeOrName { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than 0")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 500, ErrorMessage = "PageSize must be between 1 and 500")]
    public int PageSize { get; set; } = 20;
}

public class TransactionReportItemDto
{
    public string Document { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public int? BaseReference { get; set; }
    public DateTime TransactionDate { get; set; }

    public int WarehouseId { get; set; }
    public string? WarehouseCode { get; set; }

    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

public class InWarehouseReportFilterDto
{
    [Required(ErrorMessage = "WarehouseId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "WarehouseId must be greater than 0")]
    public int WarehouseId { get; set; }

    public string? ItemCodeOrName { get; set; }
    public bool ShowItemsWithNoQuantityInStock { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than 0")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 500, ErrorMessage = "PageSize must be between 1 and 500")]
    public int PageSize { get; set; } = 20;
}

public class InWarehouseReportItemDto
{
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string UoM { get; set; } = string.Empty;
    public double? InStock { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
}

public class TransactionReportSourcesCountDto
{
    public int WarehouseId { get; set; }
    public int GoodsReceiptCount { get; set; }
    public int GoodsIssueCount { get; set; }
    public int TransferOutCount { get; set; }
    public int TransferInCount { get; set; }
    public int CountingCount { get; set; }
    public int ProductionReceiptCount { get; set; }
    public int SalesDeliveryCount { get; set; }
    public int SalesReturnCount { get; set; }
    public int TotalCount { get; set; }
}
