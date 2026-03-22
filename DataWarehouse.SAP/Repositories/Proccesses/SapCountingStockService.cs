using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.SAP.Interfaces.Based;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;
using DataWarehouse.SAP.Interfaces.Proccesses;

namespace DataWarehouse.SAP.Repositories.Proccesses
{
   

    //public interface ISapCountingStockService
    //{
    //    Task<string> SyncCountingStockAsync(int countStockId);
    //}

    public class SapCountingStockService : ISapCountingStockService
    {
        private readonly IBaseSap<SapInventoryCountingDto> _sap;
        private readonly ILogger<SapCountingStockService> _logger;
        private readonly DataWarehouseDbContext _context;

        public SapCountingStockService(
            IBaseSap<SapInventoryCountingDto> sap,
            DataWarehouseDbContext context,
            ILogger<SapCountingStockService> logger)
        {
            _sap = sap;
            _context = context;
            _logger = logger;
        }

        public async Task<string> SyncCountingStockAsync(int countStockId)
        {
            // ✅ Approved IDs from process table
            // ✳️ غيّر ProcessType.CountStock لو اسم الـ enum مختلف عندك
            var approvedIdsQuery = _context.ProcessItemIsProgresses
                .AsNoTracking()
                .Where(p => p.ProcessType == ProcessType.Counting && p.Status == ProcessStatus.Approved)
                .Select(p => p.ReferenceId)
                .Distinct();

            // ✅ Load order
            var order = await _context.Set<CountStock>()
                .AsTracking()
                .Include(x => x.Warehouse)
                .Include(x => x.User)
                .Include(x => x.CountStockItem)
                .Where(x =>
                    x.Status == GeneralStatus.Processing &&
                    approvedIdsQuery.Contains(x.CountStockId))
                .FirstOrDefaultAsync(x => x.CountStockId == countStockId);

            if (order == null)
                return "This stock count must be approved before sending it to SAP.";

            var (_, success, body, error) = await ProcessCountingStockAsync(order.Warehouse.SapId, order);

            if (success)
            {
                order.Status = GeneralStatus.Completed;
                order.ErrorMessage = null;

                try
                {
                    var res = JsonSerializer.Deserialize<InventoryCountingResponse>(
                        body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (res != null)
                    {
                        order.DocEntry = res.DocEntry;
                        order.DocNum = res.DocNum;
                        order.DocType = res.DocType;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Could not deserialize InventoryCounting response for CountStockId={Id}",
                        order.CountStockId);
                }

                _context.Entry(order).Property(x => x.Status).IsModified = true;
                _context.Entry(order).Property(x => x.DocEntry).IsModified = true;
                _context.Entry(order).Property(x => x.DocNum).IsModified = true;
                _context.Entry(order).Property(x => x.DocType).IsModified = true;
                _context.Entry(order).Property(x => x.ErrorMessage).IsModified = true;
            }
            else
            {
                order.Status = GeneralStatus.PartiallyFailed;
                order.ErrorMessage = error;

                _context.Entry(order).Property(x => x.Status).IsModified = true;
                _context.Entry(order).Property(x => x.ErrorMessage).IsModified = true;
            }

            try
            {
                _context.ChangeTracker.DetectChanges();
                var affected = await _context.SaveChangesAsync();
                _logger.LogInformation("CountingStock SaveChanges affected rows={Affected}", affected);

                _context.ChangeTracker.Clear();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex,
                    "Concurrency issue while saving CountingStock for sapId={SapId}",
                    order.Warehouse.SapId);
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex,
                    "DB update issue while saving CountingStock for sapId={SapId}",
                    order.Warehouse.SapId);
                throw;
            }

            return "Sync completed.";
        }

        private async Task<(CountStock order, bool success, string res, string? error)>
            ProcessCountingStockAsync(int sapId, CountStock order)
        {
            try
            {
                if (order.Warehouse == null || string.IsNullOrWhiteSpace(order.Warehouse.WarehouseCode))
                    throw new InvalidOperationException($"CountStock#{order.CountStockId}: WarehouseCode is missing.");

                if (order.CountStockItem == null || !order.CountStockItem.Any())
                    throw new InvalidOperationException($"CountStock#{order.CountStockId}: No items.");

                var sapDto = new SapInventoryCountingDto
                {
                    CountDate = ConvertToSapDateFormat(order.PostingDate == default ? order.CreatedAt : order.PostingDate),
                    Remarks = string.IsNullOrWhiteSpace(order.Comment)
                        ? $"CountStock-{order.CountStockId}"
                        : order.Comment,
                    JournalRemark = $"CountStock-{order.CountStockId}",
                    BPLID = TryResolveBplId(order),
                    InventoryCountingLines = new List<SapInventoryCountingLineDto>()
                };

                foreach (var line in order.CountStockItem.OrderBy(x => GetIntValue(x, "CountStockItemId")))
                {
                    var itemCode = ResolveItemCode(line);
                    if (string.IsNullOrWhiteSpace(itemCode))
                        throw new InvalidOperationException(
                            $"CountStock#{order.CountStockId}: ItemCode is missing on one of the lines.");

                    var countedQty = ResolveCountedQuantity(line);
                    if (countedQty < 0)
                        throw new InvalidOperationException(
                            $"CountStock#{order.CountStockId}: Counted quantity cannot be negative for ItemCode={itemCode}.");

                    var lineWarehouseCode =
                        ResolveString(line, "WarehouseCode") ??
                        ResolveWarehouseCodeFromNavigation(line) ??
                        order.Warehouse.WarehouseCode;

                    if (string.IsNullOrWhiteSpace(lineWarehouseCode))
                        throw new InvalidOperationException(
                            $"CountStock#{order.CountStockId}: WarehouseCode is missing for ItemCode={itemCode}.");

                    var sapLine = new SapInventoryCountingLineDto
                    {
                        ItemCode = itemCode,
                        WarehouseCode = lineWarehouseCode,
                        CountedQuantity = countedQty,
                        UoMCode = ResolveString(line, "UoMCode"),
                        UoMCountedQuantity = ResolveNullableDecimal(line, "UoMCountedQuantity"),
                        BarCode = ResolveString(line, "BarCode"),
                        BinEntry = ResolveNullableInt(line, "BinEntry"),
                        Freeze = ResolveNullableYesNo(line, "Freeze"),
                        Counted = "tYES"
                    };

                    var batches = ResolveBatches(line);
                    if (batches.Count > 0)
                    {
                        var sum = batches.Sum(x => x.Quantity);
                        if (sum != countedQty)
                        {
                            throw new InvalidOperationException(
                                $"CountStock#{order.CountStockId}: Sum(BatchNumbers.Quantity)={sum} != CountedQuantity={countedQty} for ItemCode={itemCode}.");
                        }

                        sapLine.InventoryCountingBatchNumbers = batches;
                    }

                    sapDto.InventoryCountingLines.Add(sapLine);
                }

                var url = "InventoryCountings";
                var responseJson = await _sap.AddSapAsync(sapId, url, sapDto);

                _logger.LogInformation(
                    "Counting stock synced successfully. CountStockId={Id}, sapResponse={Resp}",
                    order.CountStockId, responseJson);

                return (order, true, responseJson, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync counting stock: CountStockId={Id}", order.CountStockId);
                return (order, false, "", ex.Message);
            }
        }

        private static string ConvertToSapDateFormat(DateTime date)
            => date.Date.ToString("yyyy-MM-dd");

        private int? TryResolveBplId(CountStock order)
        {
            var prop = order.Warehouse?.GetType().GetProperty("BplId");
            if (prop?.GetValue(order.Warehouse) is int bpl && bpl > 0)
                return bpl;


            return null;
        }

        // =========================
        // Reflection Helpers
        // =========================

        private static string? ResolveItemCode(object line)
        {
            // 1) direct ItemCode
            var directItemCode = ResolveString(line, "ItemCode");
            if (!string.IsNullOrWhiteSpace(directItemCode))
                return directItemCode;

            // 2) from Item navigation => Item.ItemCode
            var itemProp = line.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
            var itemObj = itemProp?.GetValue(line);
            if (itemObj != null)
            {
                var itemCodeProp = itemObj.GetType().GetProperty("ItemCode", BindingFlags.Public | BindingFlags.Instance);
                var code = itemCodeProp?.GetValue(itemObj)?.ToString();
                if (!string.IsNullOrWhiteSpace(code))
                    return code;
            }

            return null;
        }

        private static decimal ResolveCountedQuantity(object line)
        {
            // حاول يجيب CountedQuantity الأول
            var counted = ResolveNullableDecimal(line, "CountedQuantity");
            if (counted.HasValue)
                return counted.Value;

            // fallback: Quantity
            var qty = ResolveNullableDecimal(line, "Quantity");
            if (qty.HasValue)
                return qty.Value;

            // fallback: InQty / CountQty
            var inQty = ResolveNullableDecimal(line, "InQty");
            if (inQty.HasValue)
                return inQty.Value;

            var countQty = ResolveNullableDecimal(line, "CountQty");
            if (countQty.HasValue)
                return countQty.Value;

            throw new InvalidOperationException("Could not resolve CountedQuantity from CountStockItem.");
        }

        private static string? ResolveWarehouseCodeFromNavigation(object line)
        {
            var whProp = line.GetType().GetProperty("Warehouse", BindingFlags.Public | BindingFlags.Instance);
            var whObj = whProp?.GetValue(line);
            if (whObj == null)
                return null;

            var codeProp = whObj.GetType().GetProperty("WarehouseCode", BindingFlags.Public | BindingFlags.Instance);
            return codeProp?.GetValue(whObj)?.ToString();
        }

        private static List<SapInventoryCountingBatchNumberDto> ResolveBatches(object line)
        {
            var result = new List<SapInventoryCountingBatchNumberDto>();

            var batchesProp =
                line.GetType().GetProperty("CountStockBatches", BindingFlags.Public | BindingFlags.Instance) ??
                line.GetType().GetProperty("InventoryCountingBatches", BindingFlags.Public | BindingFlags.Instance) ??
                line.GetType().GetProperty("Batches", BindingFlags.Public | BindingFlags.Instance);

            if (batchesProp?.GetValue(line) is not System.Collections.IEnumerable batches)
                return result;

            foreach (var batch in batches)
            {
                var batchNumber = ResolveString(batch, "BatchNumber");
                if (string.IsNullOrWhiteSpace(batchNumber))
                    throw new InvalidOperationException("BatchNumber is missing in one of counting stock batches.");

                var qty = ResolveNullableDecimal(batch, "Quantity");
                if (!qty.HasValue || qty.Value <= 0)
                    throw new InvalidOperationException($"Batch quantity must be > 0 for Batch={batchNumber}.");

                result.Add(new SapInventoryCountingBatchNumberDto
                {
                    BatchNumber = batchNumber,
                    Quantity = qty.Value
                });
            }

            return result;
        }

        private static string? ResolveString(object obj, string propertyName)
        {
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            return prop?.GetValue(obj)?.ToString();
        }

        private static int GetIntValue(object obj, string propertyName)
        {
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            var value = prop?.GetValue(obj);

            if (value == null)
                return 0;

            return Convert.ToInt32(value);
        }

        private static int? ResolveNullableInt(object obj, string propertyName)
        {
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            var value = prop?.GetValue(obj);

            if (value == null)
                return null;

            return Convert.ToInt32(value);
        }

        private static decimal? ResolveNullableDecimal(object obj, string propertyName)
        {
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            var value = prop?.GetValue(obj);

            if (value == null)
                return null;

            return Convert.ToDecimal(value);
        }

        private static string? ResolveNullableYesNo(object obj, string propertyName)
        {
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            var value = prop?.GetValue(obj);

            if (value == null)
                return null;

            if (value is bool b)
                return b ? "tYES" : "tNO";

            var str = value.ToString();
            if (string.Equals(str, "tYES", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(str, "tNO", StringComparison.OrdinalIgnoreCase))
                return str;

            return null;
        }

        // =========================
        // SAP DTOs (InventoryCountings)
        // =========================

        public sealed class SapInventoryCountingDto
        {
            public string CountDate { get; set; } = string.Empty;
            public string? Remarks { get; set; }
            public string? JournalRemark { get; set; }
            public int? BPLID { get; set; }
            public List<SapInventoryCountingLineDto> InventoryCountingLines { get; set; } = new();
        }

        public sealed class SapInventoryCountingLineDto
        {
            public string ItemCode { get; set; } = string.Empty;
            public string WarehouseCode { get; set; } = string.Empty;
            public decimal CountedQuantity { get; set; }
            public string Counted { get; set; } = "tYES";
            public string? UoMCode { get; set; }
            public decimal? UoMCountedQuantity { get; set; }
            public string? BarCode { get; set; }
            public int? BinEntry { get; set; }
            public string? Freeze { get; set; }
            public List<SapInventoryCountingBatchNumberDto>? InventoryCountingBatchNumbers { get; set; }
        }

        public sealed class SapInventoryCountingBatchNumberDto
        {
            public string BatchNumber { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
        }

        // =========================
        // SAP Response
        // =========================

        public sealed class InventoryCountingResponse
        {
            public int? DocEntry { get; set; }
            public int? DocNum { get; set; }
            public string? DocType { get; set; }
        }
    }

}
