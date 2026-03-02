using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.SAP.Enums;
using DataWarehouse.SAP.Interfaces.Based;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Repositories.Proccesses
{

    public class SapSalesService : ISapSalesService
    {
        private readonly IBaseSap<object> _sap;
        private readonly ISapSyncStatusRepository _syncRepo; // مش هنستخدم date methods
        private readonly ILogger<SapSalesService> _logger;
        private readonly DataWarehouseDbContext _context;

        public SapSalesService(
            IBaseSap<object> sap,
            ISapSyncStatusRepository syncRepo,
            DataWarehouseDbContext context,
            ILogger<SapSalesService> logger)
        {
            _sap = sap;
            _syncRepo = syncRepo;
            _context = context;
            _logger = logger;
        }

        public async Task<string> SyncSalesOrdersAsync(int sapId, CancellationToken ct = default)
        {
            // ✅ pagination state
            var state = await _syncRepo.GetLastSyncPaginationSkipAsync(sapId, EntitiesName.sales.ToString());
            int skip = state;

            int totalAdded = 0;
            int totalAlreadyExists = 0;
            int totalFailed = 0;

            int top = 2;

            while (true)
            {
                // SAP B1 Service Layer style: Orders?$skip=..&$top=..&$select=..&$expand=DocumentLines
                // ✅ بنسحب أهم الحقول + lines + batchnumbers داخل lines
                var url =
                    $"Orders?$skip={skip}&$top={top}" +
                    $"&$select=DocEntry,DocNum,DocType,DocDate,DocDueDate,CardCode,Comments,DocumentLines";
              
                _logger.LogInformation("SAP SalesOrders Sync. sapId={sapId}, skip={skip}, top={top}, url={url}",
                    sapId, skip, top, url);

                var json = await _sap.GetAllSap(sapId, url);

                SapSalesOrdersResponse? response;
                try
                {
                    response = JsonSerializer.Deserialize<SapSalesOrdersResponse>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                }
                catch (JsonException ex)
                {
                    throw new Exception("Failed to deserialize SAP sales orders response", ex);
                }

                if (response?.Value == null || response.Value.Count == 0)
                {
                    _logger.LogInformation(
                        "SalesOrders sync finished. TotalAdded={added}, TotalAlreadyExists={exists}, TotalFailed={failed}",
                        totalAdded, totalAlreadyExists, totalFailed);

                    // ✅ لو خلصنا فعلاً، نقدر نصفر الـ skip لو ده أسلوبك، لكن غالبًا لأ
                    // أنا بسيبها على آخر skip اللي وصلنا له (هيكون اتحدّث أثناء الشغل)
                    return totalAdded == 0 && totalAlreadyExists == 0
                        ? "No sales orders to sync"
                        : $"SalesOrders synced. Added={totalAdded}, AlreadyExists={totalAlreadyExists}, Failed={totalFailed}.";
                }

                // ✅ Process page
                var pageResult = await AddSalesOrdersPageAsync(sapId, response.Value, ct);

                totalAdded += pageResult.Added;
                totalAlreadyExists += pageResult.AlreadyExists;
                totalFailed += pageResult.Failed;

                var processedThisPage = pageResult.Added + pageResult.AlreadyExists;

                //if (processedThisPage == 0)
                //{
                //    // ❗ولا Order اتضاف ولا اتعدّى — معناه غالبًا missing deps
                //    // حسب طلبك: لا نحدث pagination عشان نرجع لهم تاني
                //    var sample = string.Join(", ", pageResult.FailedDocNums.Take(10));
                //    _logger.LogWarning(
                //        "SalesOrders paging stopped because no orders were processed for this page. " +
                //        "skip={skip}. FailedDocNums(sample)={sample}. Fix dependencies then rerun.",
                //        skip, sample);

                //    return
                //        $"Stopped at skip={skip} because no orders could be saved (missing Customer/Warehouse/Items). " +
                //        $"Fix dependencies then rerun. Failed DocNums(sample): {sample}";
                //}

                // ✅ update pagination only if we actually processed something
                var nextSkip = skip + top;
                await UpdatePaginationSkipAsync(sapId, EntitiesName.sales.ToString(), nextSkip, ct);

                skip = nextSkip;
            }
        }

        private async Task UpdatePaginationSkipAsync(int sapId, string entityName, int nextSkip, CancellationToken ct)
        {
            var row = await _context.SapSyncPaginations
                .FirstOrDefaultAsync(x => x.SapId == sapId && x.EntityName == entityName, ct);

            if (row == null)
                throw new InvalidOperationException($"SapSyncPagination row not found for SapId={sapId}, EntityName={entityName}");

            row.Skip = nextSkip;
            _context.Entry(row).Property(x => x.Skip).IsModified = true;

            await _context.SaveChangesAsync(ct);
        }

        private async Task<PageProcessResult> AddSalesOrdersPageAsync(int sapId, List<SapSalesOrderDto> sapOrders, CancellationToken ct)
        {
            if (sapOrders == null || sapOrders.Count == 0)
                return PageProcessResult.Empty;

            // ✅ keys (customer codes, warehouse codes, item codes)
            var customerCodes = sapOrders
                .Select(o => o.CardCode)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();

            var warehouseCodes = sapOrders
                .SelectMany(o => o.DocumentLines ?? new List<SapSalesOrderLineDto>())
                .Select(l => l.WarehouseCode)
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Distinct()
                .ToList();

            var itemCodes = sapOrders
                .SelectMany(o => o.DocumentLines ?? new List<SapSalesOrderLineDto>())
                .Select(l => l.ItemCode)
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Distinct()
                .ToList();

            // ✅ load refs once
            var customers = await _context.Customers
                .Where(c => c.SapId == sapId && customerCodes.Contains(c.CustomerCode))
                .ToListAsync(ct);
            var customersByCode = customers.ToDictionary(x => x.CustomerCode, x => x);




            var warehouses = await _context.Warehouses
                .Where(w => w.SapId == sapId && warehouseCodes.Contains(w.WarehouseCode))
                .ToListAsync(ct);
            var warehousesByCode = warehouses.ToDictionary(x => x.WarehouseCode, x => x);



            var items = await _context.Items
                .Where(i => i.SapId == sapId && itemCodes.Contains(i.ItemCode))
                .ToListAsync(ct);

            var itemsByCode = items.ToDictionary(x => x.ItemCode, x => x);



            // ✅ existing orders check (DocEntry is best key)
            var docEntries = sapOrders.Where(o => o.DocEntry.HasValue).Select(o => o.DocEntry!.Value).Distinct().ToList();

            var existingOrders = await _context.SalesOrders
                .Where(o => o.SapId == sapId && o.DocEntry.HasValue && docEntries.Contains(o.DocEntry.Value))
                .Select(o => new { o.DocEntry, o.SalesOrderId })
                .ToListAsync(ct);


            var existingDocEntrySet = existingOrders
                .Where(x => x.DocEntry.HasValue)
                .Select(x => x.DocEntry!.Value)
                .ToHashSet();

            var result = new PageProcessResult();

            // ✅ speedup
            var prevAutoDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                foreach (var sapOrder in sapOrders)
                {
                    // basic sanity
                    //if (!sapOrder.DocEntry.HasValue || string.IsNullOrWhiteSpace(sapOrder.CardCode))
                    //{
                    //    result.Failed++;
                    //    if (sapOrder.DocNum.HasValue) result.FailedDocNums.Add(sapOrder.DocNum.Value);
                    //    continue;
                    //}

                    if (existingDocEntrySet.Contains(sapOrder.DocEntry.Value))
                    {
                        result.AlreadyExists++;
                        continue;
                    }

                    // ✅ customer
                    if (!customersByCode.TryGetValue(sapOrder.CardCode!, out var customer))
                    {
                        result.Failed++;
                        if (sapOrder.DocNum.HasValue) result.FailedDocNums.Add(sapOrder.DocNum.Value);

                        _logger.LogWarning("Skipping order DocEntry={docEntry}, DocNum={docNum}: Customer not found for CardCode={cardCode}",
                            sapOrder.DocEntry, sapOrder.DocNum, sapOrder.CardCode);
                        continue;
                    }


                    // ✅ lines
                    var lines = sapOrder.DocumentLines ?? new List<SapSalesOrderLineDto>();
                    if (lines.Count == 0)
                    {
                        result.Failed++;
                        if (sapOrder.DocNum.HasValue) result.FailedDocNums.Add(sapOrder.DocNum.Value);

                        _logger.LogWarning("Skipping order DocEntry={docEntry}, DocNum={docNum}: no DocumentLines",
                            sapOrder.DocEntry, sapOrder.DocNum);
                        continue;
                    }

                    // ✅ warehouse for header: take first non-empty warehouse from lines
                    var whCode = lines.Select(l => l.WarehouseCode).FirstOrDefault(w => !string.IsNullOrWhiteSpace(w));
                    if (string.IsNullOrWhiteSpace(whCode) || !warehousesByCode.TryGetValue(whCode!, out var warehouse))
                    {
                        result.Failed++;
                        if (sapOrder.DocNum.HasValue) result.FailedDocNums.Add(sapOrder.DocNum.Value);

                        _logger.LogWarning("Skipping order DocEntry={docEntry}, DocNum={docNum}: Warehouse not found for WarehouseCode={whCode}",
                            sapOrder.DocEntry, sapOrder.DocNum, whCode);
                        continue;
                    }

                    // ✅ ensure all items exist
                    bool missingItem = false;
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line.ItemCode) || !itemsByCode.ContainsKey(line.ItemCode!))
                        {
                            missingItem = true;
                            _logger.LogWarning(
                                "Skipping order DocEntry={docEntry}, DocNum={docNum}: Item not found for ItemCode={itemCode}",
                                sapOrder.DocEntry, sapOrder.DocNum, line.ItemCode);
                            break;
                        }
                    }
                    if (missingItem)
                    {
                        result.Failed++;
                        if (sapOrder.DocNum.HasValue) result.FailedDocNums.Add(sapOrder.DocNum.Value);
                        continue;
                    }

                    // ✅ create SalesOrder
                    var order = new SalesOrder
                    {
                        SapId = sapId,
                        DocEntry = sapOrder.DocEntry,
                        DocNum = sapOrder.DocNum,
                        DocType = sapOrder.DocType,

                        CreatedAt = DateTime.UtcNow,
                        PostingDate = sapOrder.DocDate ?? DateTime.UtcNow,
                        DueDate = sapOrder.DocDueDate ?? (sapOrder.DocDate ?? DateTime.UtcNow),
                        Comment = sapOrder.Comments,

                        Status = GeneralStatus.Completed, // أو Draft حسب نظامك

                        CustomerId = customer.CustomerId,
                        WarehouseId = warehouse.WarehouseId,

                        // لو عندك UserId mandatory: لازم تحدده بطريقة عندك
                        // هنا بخليها placeholder - لازم تغيرها بما يتناسب مع مشروعك
                        //UserId = "system"
                    };

                    foreach (var line in lines)
                    {
                        var item = itemsByCode[line.ItemCode!];

                        var orderItem = new SalesOrderItem
                        {
                            ItemId = item.ItemId,
                            Quantity = (decimal)(line.Quantity ?? 0),
                            UnitPrice = (decimal?)(line.UnitPrice ?? line.Price),
                            BarCode = line.BarCode,
                            Status = GeneralItemStatus.Closed,
                            UoMEntry = 0 // لو SAP بيرجّع UoMEntry ابعتلي الحقل ونربطه
                        };

                        // ✅ batches
                        var batches = line.BatchNumbers ?? new List<SapBatchNumberDto>();
                        foreach (var b in batches)
                        {
                            orderItem.SalesOrderBatches.Add(new SalesOrderBatch
                            {
                                Quantity = (decimal)(b.Quantity ?? 0),
                                BatchNumber = b.BatchNumber,
                                ExpiryDate = b.ExpiryDate,
                                CreatedAt = DateTime.UtcNow,
                                Comment = null
                            });
                        }

                        order.SalesOrderItems.Add(orderItem);
                    }

                    await _context.SalesOrders.AddAsync(order, ct);
                    result.Added++;

                    // ✅ important: add to set so duplicates in same page don't add twice
                    existingDocEntrySet.Add(sapOrder.DocEntry.Value);
                }

                if (result.Added > 0)
                    await _context.SaveChangesAsync(ct);

                return result;
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = prevAutoDetect;
            }
        }

        private sealed class PageProcessResult
        {
            public int Added { get; set; }
            public int AlreadyExists { get; set; }
            public int Failed { get; set; }
            public List<int> FailedDocNums { get; } = new();

            public static PageProcessResult Empty => new();
        }
    }

    /* =========================
       SAP DTOs (Orders endpoint)
       ========================= */

    public sealed class SapSalesOrdersResponse
    {
        public List<SapSalesOrderDto> Value { get; set; } = new();
    }

    public sealed class SapSalesOrderDto
    {
        public int? DocEntry { get; set; }
        public int? DocNum { get; set; }
        public string? DocType { get; set; }
        public DateTime? DocDate { get; set; }
        public DateTime? DocDueDate { get; set; }
        public string? CardCode { get; set; }
        public string? Comments { get; set; }

        public List<SapSalesOrderLineDto>? DocumentLines { get; set; }
    }

    public sealed class SapSalesOrderLineDto
    {
        public int? LineNum { get; set; }
        public string? ItemCode { get; set; }
        public double? Quantity { get; set; }
        public double? Price { get; set; }
        public string? WarehouseCode { get; set; }
        public string? BarCode { get; set; }
        public double? UnitPrice { get; set; }

        public List<SapBatchNumberDto>? BatchNumbers { get; set; }
    }

    public sealed class SapBatchNumberDto
    {
        public string? BatchNumber { get; set; }
        public double? Quantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
