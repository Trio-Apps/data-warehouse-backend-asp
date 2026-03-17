using System;
using System.Collections.Generic;

namespace DataWarehouse.Core.DTOs.Dashboard
{
    public class DueTodayTransferRequestDto
    {
        public int TransferredRequestId { get; set; }
        public int WarehouseId { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string? SourceWarehouseName { get; set; }
        public string? DestinationWarehouseName { get; set; }
        public string? SubmittedBy { get; set; }
        public DateTime SubmittedDate { get; set; }
        public DateTime DueDate { get; set; }
        public int ItemCount { get; set; }
    }

    public class DueTodayProductionOrderDto
    {
        public int ProductionOrderId { get; set; }
        public int WarehouseId { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string? ItemName { get; set; }
        public decimal PlannedQuantity { get; set; }
        public string? WarehouseName { get; set; }
        public DateTime DueDate { get; set; }
        public int ItemCount { get; set; }
    }

    public class DueTodayInventoryTaskDto
    {
        public int QuantityAdjustmentStockId { get; set; }
        public int WarehouseId { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string? ItemName { get; set; }
        public string? WarehouseName { get; set; }
        public DateTime DueDate { get; set; }
        public int ItemCount { get; set; }
    }

    public class DueTodaySummaryDto
    {
        public int PurchaseOrdersDueToday { get; set; }
        public int DeliveriesDueToday { get; set; }

        // Additional transaction types used in the system
        public int ProductionOrdersDueToday { get; set; }
        public int TransferredRequestsDueToday { get; set; }
        public int TransferredStockDueToday { get; set; }
        public int ReceivedStockDueToday { get; set; }
        public int QuantityAdjustmentsDueToday { get; set; }
        public int SalesOrdersDueToday { get; set; }
        public int SalesReturnOrdersDueToday { get; set; }
        public int ReceiptPurchaseOrdersDueToday { get; set; }
        public int GoodsReturnOrdersDueToday { get; set; }

        public int TotalDueToday { get; set; }
        public List<DueTodayTransferRequestDto> TransferRequests { get; set; } = new();
        public List<DueTodayProductionOrderDto> ProductionOrders { get; set; } = new();
        public List<DueTodayInventoryTaskDto> InventoryTasks { get; set; } = new();
    }
}
