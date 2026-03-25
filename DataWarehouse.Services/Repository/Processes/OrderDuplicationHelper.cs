using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;

namespace DataWarehouse.Services.Repository.Processes;

internal static class OrderDuplicationHelper
{
    public static PurchaseOrder Clone(PurchaseOrder source, string userId)
    {
        var clone = new PurchaseOrder
        {
            Status = GeneralStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            PostingDate = source.PostingDate,
            DueDate = source.DueDate,
            Comment = source.Comment,
            ErrorMessage = null,
            DocEntry = null,
            DocNum = null,
            DocType = source.DocType,
            UserId = userId,
            WarehouseId = source.WarehouseId,
            SupplierId = source.SupplierId,
            PurchaseOrderItems = source.PurchaseOrderItems
                .OrderBy(x => x.LineNum ?? int.MaxValue)
                .ThenBy(x => x.PurchaseOrderItemId)
                .Select(Clone)
                .ToList()
        };

        return clone;
    }

    public static SalesOrder Clone(SalesOrder source, string userId)
    {
        var clone = new SalesOrder
        {
            Status = GeneralStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            PostingDate = source.PostingDate,
            DueDate = source.DueDate,
            Comment = source.Comment,
            ErrorMessage = null,
            DocEntry = null,
            DocNum = null,
            DocType = source.DocType,
            UserId = userId,
            WarehouseId = source.WarehouseId,
            CustomerId = source.CustomerId,
            SapId = source.SapId,
            SalesOrderItems = source.SalesOrderItems
                .OrderBy(x => x.LineNum ?? int.MaxValue)
                .ThenBy(x => x.SalesOrderItemId)
                .Select(Clone)
                .ToList()
        };

        return clone;
    }

    public static ReceiptPurchaseOrder Clone(ReceiptPurchaseOrder source, string userId)
    {
        var clone = new ReceiptPurchaseOrder
        {
            Status = GeneralStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            PostingDate = source.PostingDate,
            DueDate = source.DueDate,
            Comment = source.Comment,
            ErrorMessage = null,
            DocEntry = null,
            DocNum = null,
            DocType = source.DocType,
            UserId = userId,
            WarehouseId = source.WarehouseId,
            SupplierId = source.SupplierId,
            PurchaseOrderId = source.PurchaseOrderId,
            ReceiptPurchaseOrderItems = source.ReceiptPurchaseOrderItems
                .OrderBy(x => x.LineNum ?? int.MaxValue)
                .ThenBy(x => x.ReceiptPurchaseOrderItemId)
                .Select(Clone)
                .ToList()
        };

        return clone;
    }

    public static GoodsReturnOrder Clone(GoodsReturnOrder source, string userId)
    {
        var clone = new GoodsReturnOrder
        {
            Status = GeneralStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            PostingDate = source.PostingDate,
            DueDate = source.DueDate,
            Comment = source.Comment,
            ErrorMessage = null,
            DocEntry = null,
            DocNum = null,
            DocType = source.DocType,
            UserId = userId,
            WarehouseId = source.WarehouseId,
            SupplierId = source.SupplierId,
            ReceiptPurchaseOrderId = source.ReceiptPurchaseOrderId,
            GoodsReturnOrderItems = source.GoodsReturnOrderItems
                .OrderBy(x => x.LineNum ?? int.MaxValue)
                .ThenBy(x => x.GoodsReturnOrderItemId)
                .Select(Clone)
                .ToList()
        };

        return clone;
    }

    public static DeliveryNoteOrder Clone(DeliveryNoteOrder source, string userId)
    {
        var clone = new DeliveryNoteOrder
        {
            Status = GeneralStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            PostingDate = source.PostingDate,
            DueDate = source.DueDate,
            Comment = source.Comment,
            ErrorMessage = null,
            DocEntry = null,
            DocNum = null,
            DocType = source.DocType,
            UserId = userId,
            WarehouseId = source.WarehouseId,
            SalesOrderId = source.SalesOrderId,
            CustomerId = source.CustomerId,
            DeliveryNoteItems = source.DeliveryNoteItems
                .OrderBy(x => x.LineNum ?? int.MaxValue)
                .ThenBy(x => x.DeliveryNoteItemId)
                .Select(Clone)
                .ToList()
        };

        return clone;
    }

    public static SalesReturnOrder Clone(SalesReturnOrder source, string userId)
    {
        var clone = new SalesReturnOrder
        {
            Status = GeneralStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            PostingDate = source.PostingDate,
            DueDate = source.DueDate,
            Comment = source.Comment,
            ErrorMessage = null,
            DocEntry = null,
            DocNum = null,
            DocType = source.DocType,
            UserId = userId,
            WarehouseId = source.WarehouseId,
            CustomerId = source.CustomerId,
            DeliveryNoteOrderId = source.DeliveryNoteOrderId,
            SalesReturnOrderItems = source.SalesReturnOrderItems
                .OrderBy(x => x.LineNum ?? int.MaxValue)
                .ThenBy(x => x.SalesReturnOrderItemId)
                .Select(Clone)
                .ToList()
        };

        return clone;
    }

    public static TransferredRequest Clone(TransferredRequest source, string userId)
    {
        var clone = new TransferredRequest
        {
            Status = GeneralStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            PostingDate = source.PostingDate,
            DueDate = source.DueDate,
            Comment = source.Comment,
            ErrorMessage = null,
            DocEntry = null,
            DocNum = null,
            DocType = source.DocType,
            UserId = userId,
            WarehouseId = source.WarehouseId,
            DistinationWarehouseId = source.DistinationWarehouseId,
            TransferredRequestItems = source.TransferredRequestItems
                .OrderBy(x => x.LineNum ?? int.MaxValue)
                .ThenBy(x => x.TransferredRequestItemId)
                .Select(Clone)
                .ToList()
        };

        return clone;
    }

    public static TransferredStock Clone(TransferredStock source, string userId)
    {
        var clone = new TransferredStock
        {
            Status = GeneralStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            PostingDate = source.PostingDate,
            DueDate = source.DueDate,
            Comment = source.Comment,
            ErrorMessage = null,
            DocEntry = null,
            DocNum = null,
            DocType = source.DocType,
            UserId = userId,
            WarehouseId = source.WarehouseId,
            DistinationWarehouseId = source.DistinationWarehouseId,
            TransferredRequestId = source.TransferredRequestId,
            TransferredItems = source.TransferredItems
                .OrderBy(x => x.LineNum ?? int.MaxValue)
                .ThenBy(x => x.TransferredItemId)
                .Select(Clone)
                .ToList()
        };

        return clone;
    }

    public static CountStock Clone(CountStock source, string userId)
    {
        var clone = new CountStock
        {
            Status = GeneralStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            PostingDate = source.PostingDate,
            Comment = source.Comment,
            DocEntry = null,
            DocNum = null,
            DocType = source.DocType,
            UserId = userId,
            WarehouseId = source.WarehouseId,
            CountStockItem = source.CountStockItem
                .OrderBy(x => x.LineNum ?? int.MaxValue)
                .ThenBy(x => x.CountStockItemId)
                .Select(Clone)
                .ToList()
        };

        return clone;
    }

    public static QuantityAdjustmentStock Clone(QuantityAdjustmentStock source, string userId)
    {
        var clone = new QuantityAdjustmentStock
        {
            Status = GeneralStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            PostingDate = source.PostingDate,
            DueDate = source.DueDate,
            Comment = source.Comment,
            ErrorMessage = null,
            DocEntry = null,
            DocNum = null,
            DocType = source.DocType,
            UserId = userId,
            WarehouseId = source.WarehouseId,
            QuantityAdjustmentStockItems = source.QuantityAdjustmentStockItems
                .OrderBy(x => x.LineNum ?? int.MaxValue)
                .ThenBy(x => x.QuantityAdjustmentStockItemId)
                .Select(Clone)
                .ToList()
        };

        return clone;
    }

    public static ReceivedStock Clone(ReceivedStock source, string userId)
    {
        var clone = new ReceivedStock
        {
            Status = GeneralStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            DueDate = source.DueDate,
            Comment = source.Comment,
            DocEntry = null,
            DocNum = null,
            DocType = source.DocType,
            UserId = userId,
            WarehouseId = source.WarehouseId,
            SourceWarehouseId = source.SourceWarehouseId,
            TransferredStockId = source.TransferredStockId,
            ReceivedItems = source.ReceivedItems
                .OrderBy(x => x.LineNum ?? int.MaxValue)
                .ThenBy(x => x.ReceivedItemId)
                .Select(Clone)
                .ToList()
        };

        return clone;
    }

    public static ProductionOrder Clone(ProductionOrder source, string userId)
    {
        var clone = new ProductionOrder
        {
            Status = GeneralStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            PostingDate = source.PostingDate,
            DueDate = source.DueDate,
            Remarks = source.Remarks,
            UserId = userId,
            WarehouseId = source.WarehouseId,
            ProductionOrderItems = source.ProductionOrderItems
                .OrderBy(x => x.ProductionOrderItemId)
                .Select(Clone)
                .ToList(),
            ProductionHeaderBatches = source.ProductionHeaderBatches
                .OrderBy(x => x.ProductionHeaderBatchId)
                .Select(Clone)
                .ToList()
        };

        return clone;
    }

    private static PurchaseOrderItem Clone(PurchaseOrderItem source) => new()
    {
        Quantity = source.Quantity,
        UoMEntry = source.UoMEntry,
        BarCode = source.BarCode,
        Status = GeneralItemStatus.Planned,
        UnitPrice = source.UnitPrice,
        ErrorMessage = null,
        LineNum = source.LineNum,
        ItemId = source.ItemId
    };

    private static SalesOrderItem Clone(SalesOrderItem source) => new()
    {
        Quantity = source.Quantity,
        UoMEntry = source.UoMEntry,
        BarCode = source.BarCode,
        Status = GeneralItemStatus.Planned,
        UnitPrice = source.UnitPrice,
        ErrorMessage = null,
        LineNum = source.LineNum,
        ItemId = source.ItemId,
        SalesOrderBatches = source.SalesOrderBatches
            .OrderBy(x => x.SalesOrderBatchId)
            .Select(Clone)
            .ToList()
    };

    private static ReceiptPurchaseOrderItem Clone(ReceiptPurchaseOrderItem source) => new()
    {
        Quantity = source.Quantity,
        UoMEntry = source.UoMEntry,
        BarCode = source.BarCode,
        Status = GeneralItemStatus.Planned,
        UnitPrice = source.UnitPrice,
        ErrorMessage = null,
        Comment = source.Comment,
        LineNum = source.LineNum,
        ItemId = source.ItemId,
        PurchaseOrderItemId = source.PurchaseOrderItemId,
        ReceiptPurchaseOrderBatches = source.ReceiptPurchaseOrderBatches
            .OrderBy(x => x.ReceiptPurchaseOrderBatchId)
            .Select(Clone)
            .ToList()
    };

    private static GoodsReturnOrderItem Clone(GoodsReturnOrderItem source) => new()
    {
        Quantity = source.Quantity,
        UoMEntry = source.UoMEntry,
        BarCode = source.BarCode,
        Status = GeneralItemStatus.Planned,
        UnitPrice = source.UnitPrice,
        ErrorMessage = null,
        Comment = source.Comment,
        LineNum = source.LineNum,
        ItemId = source.ItemId,
        ReceiptPurchaseOrderItemId = source.ReceiptPurchaseOrderItemId,
        GoodsReturnOrderBatches = source.GoodsReturnOrderBatches
            .OrderBy(x => x.GoodsReturnOrderBatchId)
            .Select(Clone)
            .ToList()
    };

    private static DeliveryNoteItem Clone(DeliveryNoteItem source) => new()
    {
        Quantity = source.Quantity,
        UoMEntry = source.UoMEntry,
        BarCode = source.BarCode,
        Status = GeneralItemStatus.Planned,
        UnitPrice = source.UnitPrice,
        ErrorMessage = null,
        LineNum = source.LineNum,
        ItemId = source.ItemId,
        SalesOrderItemId = source.SalesOrderItemId,
        DeliveryNoteBatches = source.DeliveryNoteBatches
            .OrderBy(x => x.DeliveryNoteBatchId)
            .Select(Clone)
            .ToList()
    };

    private static SalesReturnOrderItem Clone(SalesReturnOrderItem source) => new()
    {
        Quantity = source.Quantity,
        UoMEntry = source.UoMEntry,
        BarCode = source.BarCode,
        Status = GeneralItemStatus.Planned,
        UnitPrice = source.UnitPrice,
        ErrorMessage = null,
        LineNum = source.LineNum,
        ItemId = source.ItemId,
        DeliveryNoteItemId = source.DeliveryNoteItemId,
        SalesReturnOrderBatches = source.SalesReturnOrderBatches
            .OrderBy(x => x.SalesReturnOrderBatchId)
            .Select(Clone)
            .ToList()
    };

    private static TransferredRequestItem Clone(TransferredRequestItem source) => new()
    {
        Quantity = source.Quantity,
        UoMEntry = source.UoMEntry,
        BarCode = source.BarCode,
        Status = GeneralItemStatus.Planned,
        UnitPrice = source.UnitPrice,
        ErrorMessage = null,
        Comment = source.Comment,
        LineNum = source.LineNum,
        ItemId = source.ItemId,
        TransferredRequestBatches = source.TransferredRequestBatches
            .OrderBy(x => x.TransferredRequestBatchId)
            .Select(Clone)
            .ToList()
    };

    private static TransferredItem Clone(TransferredItem source) => new()
    {
        Quantity = source.Quantity,
        UoMEntry = source.UoMEntry,
        BarCode = source.BarCode,
        Status = GeneralItemStatus.Planned,
        UnitPrice = source.UnitPrice,
        ErrorMessage = null,
        Comment = source.Comment,
        LineNum = source.LineNum,
        ItemId = source.ItemId,
        TransferredRequestItemId = source.TransferredRequestItemId,
        TransferredStockBatches = source.TransferredStockBatches
            .OrderBy(x => x.TransferredStockBatchId)
            .Select(Clone)
            .ToList()
    };

    private static CountStockItem Clone(CountStockItem source) => new()
    {
        Quantity = source.Quantity,
        UoMEntry = source.UoMEntry,
        BarCode = source.BarCode,
        Status = GeneralItemStatus.Planned,
        UnitPrice = source.UnitPrice,
        ErrorMessage = null,
        Comment = source.Comment,
        LineNum = source.LineNum,
        ItemId = source.ItemId,
        CountStockBatches = source.CountStockBatches
            .OrderBy(x => x.CountStockBatchId)
            .Select(Clone)
            .ToList()
    };

    private static QuantityAdjustmentStockItem Clone(QuantityAdjustmentStockItem source) => new()
    {
        Quantity = source.Quantity,
        UoMEntry = source.UoMEntry,
        BarCode = source.BarCode,
        Status = GeneralItemStatus.Planned,
        UnitPrice = source.UnitPrice,
        ErrorMessage = null,
        Comment = source.Comment,
        LineNum = source.LineNum,
        ItemId = source.ItemId,
        QuantityAdjustmentStockBatches = source.QuantityAdjustmentStockBatches
            .OrderBy(x => x.QuantityAdjustmentStockBatchId)
            .Select(Clone)
            .ToList()
    };

    private static ReceivedItem Clone(ReceivedItem source) => new()
    {
        Quantity = source.Quantity,
        UoMEntry = source.UoMEntry,
        BarCode = source.BarCode,
        Status = GeneralItemStatus.Planned,
        UnitPrice = source.UnitPrice,
        ErrorMessage = null,
        Comment = source.Comment,
        LineNum = source.LineNum,
        ItemId = source.ItemId,
        TransferredItemId = source.TransferredItemId,
        ReceivedStockBatches = source.ReceivedStockBatches
            .OrderBy(x => x.ReceivedStockBatchId)
            .Select(Clone)
            .ToList()
    };

    private static ProductionOrderItem Clone(ProductionOrderItem source) => new()
    {
        PlannedQuantity = source.PlannedQuantity,
        ProducedQuantity = null,
        AbsoluteEntry = null,
        Status = GeneralItemStatus.Planned,
        ErrorMessage = null,
        CreatedAt = DateTime.UtcNow,
        ProcessedAt = null,
        ItemId = source.ItemId
    };

    private static ReceiptPurchaseOrderBatch Clone(ReceiptPurchaseOrderBatch source) => new()
    {
        Quantity = source.Quantity,
        Comment = source.Comment,
        BatchNumber = source.BatchNumber,
        ExpiryDate = source.ExpiryDate,
        CreatedAt = DateTime.UtcNow
    };

    private static GoodsReturnOrderBatch Clone(GoodsReturnOrderBatch source) => new()
    {
        Quantity = source.Quantity,
        Comment = source.Comment,
        BatchNumber = source.BatchNumber,
        ExpiryDate = source.ExpiryDate,
        CreatedAt = DateTime.UtcNow,
        ReceiptPurchaseOrderBatchId = source.ReceiptPurchaseOrderBatchId
    };

    private static SalesOrderBatch Clone(SalesOrderBatch source) => new()
    {
        Quantity = source.Quantity,
        Comment = source.Comment,
        BatchNumber = source.BatchNumber,
        ExpiryDate = source.ExpiryDate,
        CreatedAt = DateTime.UtcNow
    };

    private static DeliveryNoteBatch Clone(DeliveryNoteBatch source) => new()
    {
        Quantity = source.Quantity,
        Comment = source.Comment,
        BatchNumber = source.BatchNumber,
        ExpiryDate = source.ExpiryDate,
        CreatedAt = DateTime.UtcNow,
        SalesOrderBatchId = source.SalesOrderBatchId
    };

    private static SalesReturnOrderBatch Clone(SalesReturnOrderBatch source) => new()
    {
        Quantity = source.Quantity,
        Comment = source.Comment,
        BatchNumber = source.BatchNumber,
        ExpiryDate = source.ExpiryDate,
        CreatedAt = DateTime.UtcNow,
        DeliveryNoteBatchId = source.DeliveryNoteBatchId
    };

    private static TransferredRequestBatch Clone(TransferredRequestBatch source) => new()
    {
        Quantity = source.Quantity,
        Comment = source.Comment,
        BatchNumber = source.BatchNumber,
        ExpiryDate = source.ExpiryDate,
        CreatedAt = DateTime.UtcNow
    };

    private static TransferredStockBatch Clone(TransferredStockBatch source) => new()
    {
        Quantity = source.Quantity,
        Comment = source.Comment,
        BatchNumber = source.BatchNumber,
        ExpiryDate = source.ExpiryDate,
        CreatedAt = DateTime.UtcNow,
        TransferredRequestBatchId = source.TransferredRequestBatchId
    };

    private static CountStockBatch Clone(CountStockBatch source) => new()
    {
        Quantity = source.Quantity,
        Comment = source.Comment,
        BatchNumber = source.BatchNumber,
        ExpiryDate = source.ExpiryDate,
        CreatedAt = DateTime.UtcNow
    };

    private static QuantityAdjustmentStockBatch Clone(QuantityAdjustmentStockBatch source) => new()
    {
        Quantity = source.Quantity,
        Comment = source.Comment,
        BatchNumber = source.BatchNumber,
        ExpiryDate = source.ExpiryDate,
        CreatedAt = DateTime.UtcNow
    };

    private static ReceivedStockBatch Clone(ReceivedStockBatch source) => new()
    {
        Quantity = source.Quantity,
        Comment = source.Comment,
        BatchNumber = source.BatchNumber,
        ExpiryDate = source.ExpiryDate,
        CreatedAt = DateTime.UtcNow,
        TransferredStockBatchId = source.TransferredStockBatchId
    };

    private static ProductionHeaderBatch Clone(ProductionHeaderBatch source) => new()
    {
        Quantity = source.Quantity,
        BatchNumber = source.BatchNumber,
        ExpiryDate = source.ExpiryDate,
        CreatedAt = DateTime.UtcNow
    };
}
