using DataWarehouse.Core.DTOs.Dashboard;
using DataWarehouse.Domain.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;

namespace DataWarehouse.Services.Repository.Dashboard
{
    public class DashboardRepository : DataWarehouse.Core.Interfaces.Dashboard.IDashboardRepository
    {
        private readonly DataWarehouseDbContext _context;

        public DashboardRepository(DataWarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<DueTodaySummaryDto> GetDueTodaySummaryAsync(string userId, int? warehouseId = null)
        {
            var accessibleWarehouses = await _context.UserWarehouses
                .Where(uw => uw.UserId == userId)
                .Select(uw => uw.WarehouseId)
                .ToListAsync();

            if (!accessibleWarehouses.Any())
                return new DueTodaySummaryDto();

            if (warehouseId.HasValue && !accessibleWarehouses.Contains(warehouseId.Value))
                throw new UnauthorizedAccessException();

            var warehouses = warehouseId.HasValue ? new[] { warehouseId.Value } : accessibleWarehouses.ToArray();

            var today = DateTime.UtcNow.Date;

            // Core transaction types
            var purchaseOrdersDueToday = await _context.Set<Domain.Entities.Processes.PurchaseOrder>()
                .AsNoTracking()
                .Where(po => warehouses.Contains(po.WarehouseId) && po.DueDate.Date == today)
                .CountAsync();

            var deliveryNotesDueToday = await _context.Set<Domain.Entities.Processes.OutSide.DeliveryNoteOrder>()
                .AsNoTracking()
                .Where(dn => warehouses.Contains(dn.WarehouseId) && dn.DueDate.Date == today)
                .CountAsync();

            var productionOrdersDueToday = await _context.Set<Domain.Entities.Processes.BulkProductions.ProductionOrder>()
                .AsNoTracking()
                .Where(po => warehouses.Contains(po.WarehouseId) && po.DueDate.Date == today)
                .CountAsync();

            // Transfer / stock movements
            var transferredRequestsDueToday = await _context.Set<Domain.Entities.Processes.TransferredRequest>()
                .AsNoTracking()
                .Where(tr => warehouses.Contains(tr.WarehouseId) && tr.DueDate.Date == today)
                .CountAsync();

            var transferredStockDueToday = await _context.Set<Domain.Entities.Processes.TransferredStock>()
                .AsNoTracking()
                .Where(ts => warehouses.Contains(ts.WarehouseId) && ts.DueDate.Date == today)
                .CountAsync();

            // Inventory adjustments
            var receivedStockDueToday = await _context.Set<Domain.Entities.Processes.ReceivedStock>()
                .AsNoTracking()
                .Where(rs => warehouses.Contains(rs.WarehouseId) && rs.DueDate.Date == today)
                .CountAsync();

            //var quantityAdjustmentsDueToday = await _context.Set<Domain.Entities.Processes.QuantityAdjustmentStock>()
            //    .AsNoTracking()
            //    .Where(q => warehouses.Contains(q.WarehouseId) && q.DueDate.Date == today)
            //    .CountAsync();

            // Sales / returns
            var salesOrdersDueToday = await _context.Set<Domain.Entities.Processes.OutSide.SalesOrder>()
                .AsNoTracking()
                .Where(so => warehouses.Contains(so.WarehouseId) && so.DueDate.Date == today)
                .CountAsync();

            var salesReturnOrdersDueToday = await _context.Set<Domain.Entities.Processes.OutSide.SalesReturnOrder>()
                .AsNoTracking()
                .Where(sr => warehouses.Contains(sr.WarehouseId) && sr.DueDate.Date == today)
                .CountAsync();

            // Receipts & returns
            var receiptPurchaseOrdersDueToday = await _context.Set<Domain.Entities.Processes.OutSide.ReceiptPurchaseOrder>()
                .AsNoTracking()
                .Where(rpo => warehouses.Contains(rpo.WarehouseId) && rpo.DueDate.Date == today)
                .CountAsync();

            var goodsReturnOrdersDueToday = await _context.Set<Domain.Entities.Processes.OutSide.GoodsReturnOrder>()
                .AsNoTracking()
                .Where(gro => warehouses.Contains(gro.WarehouseId) && gro.DueDate.Date == today)
                .CountAsync();

            var transferRequests = await _context.Set<TransferredRequest>()
                .AsNoTracking()
                .Include(tr => tr.Warehouse)
                .Include(tr => tr.DistinationWarehouse)
                .Include(tr => tr.User)
                .Include(tr => tr.TransferredRequestItems)
                .Where(tr => warehouses.Contains(tr.WarehouseId) && tr.DueDate.Date == today)
                .OrderByDescending(tr => tr.CreatedAt)
                .Take(8)
                .Select(tr => new DueTodayTransferRequestDto
                {
                    TransferredRequestId = tr.TransferredRequestId,
                    WarehouseId = tr.WarehouseId,
                    ReferenceNumber = tr.DocNum.HasValue ? $"TR-{tr.DocNum.Value}" : $"TR-{tr.TransferredRequestId}",
                    SourceWarehouseName = tr.Warehouse.WarehouseName,
                    DestinationWarehouseName = tr.DistinationWarehouse.WarehouseName,
                    SubmittedBy = tr.User.FullName,
                    SubmittedDate = tr.CreatedAt,
                    DueDate = tr.DueDate,
                    ItemCount = tr.TransferredRequestItems.Count()
                })
                .ToListAsync();

            var productionOrders = await _context.Set<ProductionOrder>()
                .AsNoTracking()
                .Include(po => po.Warehouse)
                .Include(po => po.ProductionOrderItems)
                    .ThenInclude(item => item.Item)
                .Where(po => warehouses.Contains(po.WarehouseId) && po.DueDate.Date == today)
                .OrderByDescending(po => po.CreatedAt)
                .Take(8)
                .Select(po => new DueTodayProductionOrderDto
                {
                    ProductionOrderId = po.ProductionOrderId,
                    WarehouseId = po.WarehouseId,
                    ReferenceNumber = $"PRD-{po.ProductionOrderId}",
                    ItemName = po.ProductionOrderItems
                        .OrderBy(item => item.ProductionOrderItemId)
                        .Select(item => item.Item.ItemName)
                        .FirstOrDefault(),
                    PlannedQuantity = po.ProductionOrderItems.Sum(item => item.PlannedQuantity),
                    WarehouseName = po.Warehouse.WarehouseCode,
                    DueDate = po.DueDate,
                    ItemCount = po.ProductionOrderItems.Count()
                })
                .ToListAsync();

            //var inventoryTasks = await _context.Set<QuantityAdjustmentStock>()
            //    .AsNoTracking()
            //    .Include(q => q.Warehouse)
            //    .Include(q => q.QuantityAdjustmentStockItems)
            //        .ThenInclude(item => item.Item)
            //    .Where(q => warehouses.Contains(q.WarehouseId) && q.DueDate.Date == today)
            //    .OrderByDescending(q => q.CreatedAt)
            //    .Take(8)
            //    .Select(q => new DueTodayInventoryTaskDto
            //    {
            //        QuantityAdjustmentStockId = q.QuantityAdjustmentStockId,
            //        WarehouseId = q.WarehouseId,
            //        ReferenceNumber = q.DocNum.HasValue ? $"INV-{q.DocNum.Value}" : $"INV-{q.QuantityAdjustmentStockId}",
            //        ItemName = q.QuantityAdjustmentStockItems
            //            .OrderBy(item => item.QuantityAdjustmentStockItemId)
            //            .Select(item => item.Item.ItemName)
            //            .FirstOrDefault(),
            //        WarehouseName = q.Warehouse.WarehouseCode,
            //        DueDate = q.DueDate,
            //        ItemCount = q.QuantityAdjustmentStockItems.Count()
            //    })
            //    .ToListAsync();

            var totalDueToday = purchaseOrdersDueToday
                + deliveryNotesDueToday
                + productionOrdersDueToday
                + transferredRequestsDueToday
                + transferredStockDueToday
                + receivedStockDueToday
              //  + quantityAdjustmentsDueToday
                + salesOrdersDueToday
                + salesReturnOrdersDueToday
                + receiptPurchaseOrdersDueToday
                + goodsReturnOrdersDueToday;

            return new DueTodaySummaryDto
            {
                PurchaseOrdersDueToday = purchaseOrdersDueToday,
                DeliveriesDueToday = deliveryNotesDueToday,
                ProductionOrdersDueToday = productionOrdersDueToday,
                TransferredRequestsDueToday = transferredRequestsDueToday,
                TransferredStockDueToday = transferredStockDueToday,
                ReceivedStockDueToday = receivedStockDueToday,
             //   QuantityAdjustmentsDueToday = quantityAdjustmentsDueToday,
                SalesOrdersDueToday = salesOrdersDueToday,
                SalesReturnOrdersDueToday = salesReturnOrdersDueToday,
                ReceiptPurchaseOrdersDueToday = receiptPurchaseOrdersDueToday,
                GoodsReturnOrdersDueToday = goodsReturnOrdersDueToday,
                TotalDueToday = totalDueToday,
                TransferRequests = transferRequests,
                ProductionOrders = productionOrders,
              //  InventoryTasks = inventoryTasks
            };
        }
    }
}
