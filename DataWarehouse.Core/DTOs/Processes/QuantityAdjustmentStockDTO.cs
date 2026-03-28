using DataWarehouse.Core.DTOs.BarCode;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes;

public class QuantityAdjustmentStockDTO : GeneralOrderDto
{
    public int QuantityAdjustmentStockId { get; set; }
    public int? ReasonId { get; set; }
    public string? ReasonName { get; set; }
    public List<QuantityAdjustmentStockItemDTO>? QuantityAdjustmentStockItems { get; set; }
}

public class AddQuantityAdjustmentStockDTO : AddGeneralOrderDto
{
    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }
    public int? ReasonId { get; set; }
}

public class UpdateQuantityAdjustmentStockDTO : UpdateGeneralOrderDto
{
    public int QuantityAdjustmentStockId { get; set; }
    public int? ReasonId { get; set; }
}

public class QuantityAdjustmentStockItemDTO
{
    public int QuantityAdjustmentStockItemId { get; set; }
    public decimal Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public int UoMEntry { get; set; }
    public string? UnitName { get; set; }
    public string? BarCode { get; set; }
    public decimal? UnitPrice { get; set; }
    
    public decimal? VatPercent { get; set; }
    public decimal? VatAmount { get; set; }
    public decimal? LineTotalBeforeVat { get; set; }
    public decimal? LineTotalAfterVat { get; set; }
    public string? ItemCode { get; set; }
    public int ItemId { get; set; }
    public string? ItemName { get; set; }
    public int QuantityAdjustmentStockId { get; set; }
}

public class AddQuantityAdjustmentStockItemCreateRequest
{
    public DynamicBarcodesDto? Barcode { get; set; }
    public AddGeneralItemDto? Item { get; set; }
}

public class QuantityAdjustmentStockBatchDTO
{
    public int QuantityAdjustmentStockBatchId { get; set; }
    public int QuantityAdjustmentStockItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? Comment { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
