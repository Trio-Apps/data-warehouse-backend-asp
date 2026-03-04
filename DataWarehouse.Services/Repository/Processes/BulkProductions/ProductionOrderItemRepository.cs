using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.BulkProductions;

public class ProductionOrderItemRepository : BaseRepository<ProductionOrderItem>, IProductionOrderItemRepository
{
    private readonly ISapCache sapCache;
    private readonly IHttpClientFactory httpClientFactory;

    public ProductionOrderItemRepository(
        ISapCache sapCache,
        IHttpClientFactory httpClientFactory,
        DataWarehouseDbContext context) : base(context)
    {
        this.sapCache = sapCache;
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<ProductionOrderItemDTO>> GetByProductionItemByProductionOrderIdAsync(int productionOrderId)
    {
        var res = await Query()
            .Where(poi => poi.ProductionOrderId == productionOrderId)
            .ToListAsync();

        return res.Select(e => new ProductionOrderItemDTO
        {
            ItemId = e.ItemId,
            AbsoluteEntry = e.AbsoluteEntry,
            Status = GetEnumString(e.Status),
            ErrorMessage = e.ErrorMessage,
            PlannedQuantity = e.PlannedQuantity,
            ProducedQuantity = e.ProducedQuantity ?? 0,
            ProductionOrderItemId = e.ProductionOrderItemId,
            ProductionOrderId = e.ProductionOrderId,
            ProcessedAt = e.ProcessedAt
        });
    }

    public async Task<GeneralResponse<PagedResult<ProductionOrderItemDTO>>> GetByProductionItemByProductionOrderIdWithPaginationAsync(
        int productionOrderId,
        string? status,
        int pageNumber,
        int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.ProductionOrderItems
            .AsNoTracking()
            .Where(b => b.ProductionOrderId == productionOrderId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusEnum = Enum.Parse<GeneralItemStatus>(status, ignoreCase: true);
            query = query.Where(iw => iw.Status == statusEnum);
        }

        var totalRecords = await query.CountAsync();

        var data = query.Select(e => new ProductionOrderItemDTO
        {
            ItemId = e.ItemId,
            AbsoluteEntry = e.AbsoluteEntry,
            Status = e.Status.ToString(),
            ErrorMessage = e.ErrorMessage,
            PlannedQuantity = e.PlannedQuantity,
            ProducedQuantity = e.ProducedQuantity ?? 0,
            ProductionOrderItemId = e.ProductionOrderItemId,
            ProductionOrderId = e.ProductionOrderId,
            ProcessedAt = e.ProcessedAt
        }).ToList();

        return GeneralResponse<PagedResult<ProductionOrderItemDTO>>.SuccessResponse(new PagedResult<ProductionOrderItemDTO>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        });
    }

    public async Task<GeneralResponse<ProductionOrderItemDTO>> AddProductionItemByProductionOrderIdAsync(
        int productionOrderid,
        AddProductionOrderItemDTO dto)
    {
        if (productionOrderid != dto.ProductionOrderId)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Route ProductionOrderId does not match request body.");

        var order = await _context.ProductionOrders
            .Include(x => x.Warehouse)
            .FirstOrDefaultAsync(x => x.ProductionOrderId == dto.ProductionOrderId);

        if (order == null)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Production order not found.");

        var existingForOrder = await _context.ProductionOrderItems
            .FirstOrDefaultAsync(x => x.ProductionOrderId == dto.ProductionOrderId);

        ProductionOrderItem mapping;
        if (existingForOrder != null)
        {
            existingForOrder.ItemId = dto.ItemId;
            existingForOrder.PlannedQuantity = dto.PlannedQuantity;
            existingForOrder.Status = GeneralItemStatus.Planned;
            existingForOrder.ErrorMessage = null;
            existingForOrder.ProducedQuantity = null;
            existingForOrder.AbsoluteEntry = null;
            existingForOrder.ProcessedAt = null;
            mapping = existingForOrder;
        }
        else
        {
            mapping = new ProductionOrderItem
            {
                Status = GeneralItemStatus.Planned,
                ProductionOrderId = dto.ProductionOrderId,
                ItemId = dto.ItemId,
                CreatedAt = DateTime.UtcNow,
                PlannedQuantity = dto.PlannedQuantity,
            };

            await AddAsync(mapping);
        }

        await SaveChangesAsync();

        var sapId = order.Warehouse?.SapId ?? await sapCache.Get();
        var syncOutcome = await TrySyncComponentLinesFromSapBomAsync(
            mapping.ProductionOrderId,
            mapping.ItemId,
            mapping.PlannedQuantity,
            order.WarehouseId,
            sapId);

        var model = new ProductionOrderItemDTO
        {
            ProductionOrderId = mapping.ProductionOrderId,
            PlannedQuantity = mapping.PlannedQuantity,
            Status = GetEnumString(mapping.Status),
            ItemId = mapping.ItemId,
            ProductionOrderItemId = mapping.ProductionOrderItemId,
        };

        return GeneralResponse<ProductionOrderItemDTO>.SuccessResponse(
            model,
            syncOutcome.Success
                ? "Finished good saved and BOM components synchronized successfully."
                : $"Finished good saved, but no BOM components were synchronized from SAP. {syncOutcome.Reason}");
    }

    public async Task<GeneralResponse<ProductionOrderItemDTO>> UpdateProductionItemAsync(
        int productionItemId,
        bool? isRecevied,
        UpdateProductionOrderItemDTO dto)
    {
        var entity = await _context.ProductionOrderItems
            .Include(x => x.ProductionOrder)
                .ThenInclude(x => x.Warehouse)
            .FirstOrDefaultAsync(e => e.ProductionOrderItemId == productionItemId);

        if (entity == null)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("not found");

        if (dto.ProductionOrderItemId != productionItemId)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("id not equal production order id!");

        if (entity.Status == GeneralItemStatus.Closed || entity.Status == GeneralItemStatus.Failed)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Closed or failed items cannot be edited.");

        if (isRecevied == true && entity.Status != GeneralItemStatus.Released)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Production item must be released before marking it as received.");

        var oldPlannedQuantity = entity.PlannedQuantity;

        entity.PlannedQuantity = dto.PlannedQuantity;
        entity.ProducedQuantity = dto.ProducedQuantity;

        if (isRecevied == true)
            entity.Status = GeneralItemStatus.Received;

        await _context.SaveChangesAsync();

        var sapId = entity.ProductionOrder?.Warehouse?.SapId ?? await sapCache.Get();
        var syncOutcome = await TrySyncComponentLinesFromSapBomAsync(
            entity.ProductionOrderId,
            entity.ItemId,
            entity.PlannedQuantity,
            entity.ProductionOrder.WarehouseId,
            sapId);

        if (!syncOutcome.Success && oldPlannedQuantity > 0 && oldPlannedQuantity != entity.PlannedQuantity)
        {
            await ScaleExistingComponentLinesAsync(
                entity.ProductionOrderId,
                oldPlannedQuantity,
                entity.PlannedQuantity);
        }

        var result = new ProductionOrderItemDTO
        {
            ItemId = entity.ItemId,
            AbsoluteEntry = entity.AbsoluteEntry,
            Status = GetEnumString(entity.Status),
            ErrorMessage = entity.ErrorMessage,
            PlannedQuantity = entity.PlannedQuantity,
            ProducedQuantity = entity.ProducedQuantity ?? 0,
            ProductionOrderItemId = entity.ProductionOrderItemId,
            ProductionOrderId = entity.ProductionOrderId,
            ProcessedAt = entity.ProcessedAt,
        };

        return GeneralResponse<ProductionOrderItemDTO>.SuccessResponse(
            result,
            syncOutcome.Success
                ? "Finished good updated and BOM components synchronized successfully."
                : $"Finished good updated, but no BOM components were synchronized from SAP. {syncOutcome.Reason}");
    }

    private async Task ScaleExistingComponentLinesAsync(int productionOrderId, decimal oldPlannedQty, decimal newPlannedQty)
    {
        if (oldPlannedQty <= 0 || newPlannedQty <= 0)
            return;

        var lines = await _context.ProductionComponentLines
            .Where(x => x.ProductionOrderId == productionOrderId)
            .ToListAsync();

        if (lines.Count == 0)
            return;

        var ratio = newPlannedQty / oldPlannedQty;
        foreach (var line in lines)
        {
            line.RequiredQuantity = Math.Round(line.RequiredQuantity * ratio, 6);

            if (line.IssuedQuantity.HasValue && line.IssuedQuantity > line.RequiredQuantity)
                line.IssuedQuantity = line.RequiredQuantity;
        }

        await _context.SaveChangesAsync();
    }

    private async Task<(bool Success, string Reason)> TrySyncComponentLinesFromSapBomAsync(
        int productionOrderId,
        int finishedGoodItemId,
        decimal plannedQuantity,
        int warehouseId,
        int? sapId)
    {
        if (plannedQuantity <= 0)
            return (false, "Planned quantity must be greater than zero.");

        if (!sapId.HasValue || sapId.Value <= 0)
        {
            var warehouseSapId = await _context.Warehouses
                .AsNoTracking()
                .Where(x => x.WarehouseId == warehouseId)
                .Select(x => (int?)x.SapId)
                .FirstOrDefaultAsync();

            if (warehouseSapId.HasValue && warehouseSapId.Value > 0)
                sapId = warehouseSapId.Value;
        }

        if (!sapId.HasValue || sapId.Value <= 0)
        {
            var currentSapId = await sapCache.Get();
            if (currentSapId.HasValue && currentSapId.Value > 0)
                sapId = currentSapId.Value;
        }

        if (!sapId.HasValue || sapId.Value <= 0)
        {
            var anyActiveSapId = await _context.Saps
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.SapId)
                .Select(x => (int?)x.SapId)
                .FirstOrDefaultAsync();

            if (anyActiveSapId.HasValue && anyActiveSapId.Value > 0)
                sapId = anyActiveSapId.Value;
        }

        if (!sapId.HasValue || sapId.Value <= 0)
            return (false, "No active SAP configuration is available.");

        var finishedGoodCode = await _context.Items
            .AsNoTracking()
            .Where(x => x.ItemId == finishedGoodItemId)
            .Select(x => x.ItemCode)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(finishedGoodCode))
            return (false, "Finished good item code was not found locally.");

        finishedGoodCode = finishedGoodCode.Trim();

        var (bomLines, bomReadError) = await GetBomLinesFromSapAsync(sapId.Value, finishedGoodCode);
        if (bomLines.Count == 0)
            return (false, bomReadError);

        var normalizedLines = bomLines
            .Where(x => !string.IsNullOrWhiteSpace(x.ItemCode)
                        && x.Quantity > 0
                        && !string.Equals(x.ItemCode, finishedGoodCode, StringComparison.OrdinalIgnoreCase))
            .GroupBy(x => new { Code = x.ItemCode.Trim(), IssueType = NormalizeIssueType(x.IssueType) })
            .Select(g => new SapBomLine
            {
                ItemCode = g.Key.Code,
                Quantity = g.Sum(x => x.Quantity),
                IssueType = g.Key.IssueType
            })
            .ToList();

        if (normalizedLines.Count == 0)
            return (false, "BOM response did not include valid component lines.");

        var componentCodes = normalizedLines
            .Select(x => x.ItemCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var items = await _context.Items
            .AsNoTracking()
            .Where(x => componentCodes.Contains(x.ItemCode))
            .Select(x => new { x.ItemId, x.ItemCode })
            .ToListAsync();

        if (items.Count == 0)
            return (false, "BOM components were returned from SAP but do not exist in local Items sync.");

        var itemByCode = items
            .GroupBy(x => x.ItemCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var componentItemIds = items.Select(x => x.ItemId).Distinct().ToList();
        var warehouseItems = await _context.WarehouseItems
            .AsNoTracking()
            .Where(x => x.WarehouseId == warehouseId && componentItemIds.Contains(x.ItemId))
            .ToDictionaryAsync(x => x.ItemId);

        var existingLineIds = await _context.ProductionComponentLines
            .Where(x => x.ProductionOrderId == productionOrderId)
            .Select(x => x.ProductionComponentLineId)
            .ToListAsync();

        if (existingLineIds.Count > 0)
        {
            var oldBatches = _context.ProductionComponentBatches
                .Where(x => existingLineIds.Contains(x.ProductionComponentLineId));
            _context.ProductionComponentBatches.RemoveRange(oldBatches);

            var oldLines = _context.ProductionComponentLines
                .Where(x => x.ProductionOrderId == productionOrderId);
            _context.ProductionComponentLines.RemoveRange(oldLines);
        }

        var linesToAdd = normalizedLines
            .Where(x => itemByCode.ContainsKey(x.ItemCode))
            .Select(x =>
            {
                var item = itemByCode[x.ItemCode];
                warehouseItems.TryGetValue(item.ItemId, out var whItem);

                var requiredQty = Math.Round(x.Quantity * plannedQuantity, 6);
                var inWhsQty = Convert.ToDecimal(whItem?.InStock ?? 0);

                return new ProductionComponentLine
                {
                    ProductionOrderId = productionOrderId,
                    ItemId = item.ItemId,
                    WarehouseId = warehouseId,
                    RequiredQuantity = requiredQty,
                    IssuedQuantity = null,
                    InWhsQuantity = inWhsQty,
                    IssueType = NormalizeIssueType(x.IssueType),
                    CreatedAt = DateTime.UtcNow
                };
            })
            .Where(x => x.RequiredQuantity > 0)
            .ToList();

        if (linesToAdd.Count == 0)
            return (false, "No component lines were generated after local warehouse/item matching.");

        await _context.ProductionComponentLines.AddRangeAsync(linesToAdd);
        await _context.SaveChangesAsync();
        return (true, "Synchronized from SAP.");
    }

    private async Task<(List<SapBomLine> Lines, string Error)> GetBomLinesFromSapAsync(int sapId, string finishedGoodItemCode)
    {
        var sap = await _context.Saps
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SapId == sapId && x.IsActive);

        if (sap == null || string.IsNullOrWhiteSpace(sap.SapUrl))
            return (new List<SapBomLine>(), "SAP settings are missing or inactive.");

        var client = httpClientFactory.CreateClient("SAP");
        client.BaseAddress = new Uri(EnsureTrailingSlash(sap.SapUrl));

        var loginPayload = new
        {
            CompanyDB = sap.CompanyDB,
            UserName = sap.UserName,
            Password = sap.Password
        };

        using var loginResponse = await client.PostAsJsonAsync("Login", loginPayload);
        if (!loginResponse.IsSuccessStatusCode)
            return (new List<SapBomLine>(), $"SAP login failed with HTTP {(int)loginResponse.StatusCode}.");

        var endpointErrors = new List<string>();
        var hadSuccessResponseWithNoLines = false;

        foreach (var endpoint in BuildProductTreesEndpoints(finishedGoodItemCode))
        {
            using var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                endpointErrors.Add($"{endpoint} -> {(int)response.StatusCode}");
                continue;
            }

            var body = await response.Content.ReadAsStringAsync();
            var parsed = ParseBomLines(body);
            if (parsed.Count > 0)
                return (parsed, string.Empty);

            hadSuccessResponseWithNoLines = true;
        }

        if (hadSuccessResponseWithNoLines)
            return (new List<SapBomLine>(), "SAP ProductTrees returned data but no parsable component lines were found.");

        if (endpointErrors.Count > 0)
            return (new List<SapBomLine>(), $"SAP ProductTrees requests failed: {string.Join(" | ", endpointErrors)}");

        return (new List<SapBomLine>(), "SAP ProductTrees did not return BOM data.");
    }

    private static string[] BuildProductTreesEndpoints(string itemCode)
    {
        var escapedODataLiteral = itemCode.Replace("'", "''");
        var encodedFilter = Uri.EscapeDataString($"TreeCode eq '{escapedODataLiteral}'");

        return new[]
        {
            $"ProductTrees('{escapedODataLiteral}')?$expand=ProductTreeLines",
            $"ProductTrees('{escapedODataLiteral}')?$select=TreeCode&$expand=ProductTreeLines",
            $"ProductTrees('{escapedODataLiteral}')",
            $"ProductTrees?$filter={encodedFilter}&$expand=ProductTreeLines&$top=1",
            $"ProductTrees?$filter={encodedFilter}&$top=1",
            $"ProductTrees?$filter={encodedFilter}&$expand=Items&$top=1"
        };
    }

    private static List<SapBomLine> ParseBomLines(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<SapBomLine>();

        try
        {
            using var doc = JsonDocument.Parse(json);
            var lines = new List<SapBomLine>();
            ExtractBomLinesRecursively(doc.RootElement, lines);

            return lines
                .Where(x => !string.IsNullOrWhiteSpace(x.ItemCode) && x.Quantity > 0)
                .ToList();
        }
        catch
        {
            return new List<SapBomLine>();
        }
    }

    private static void ExtractBomLinesRecursively(JsonElement element, List<SapBomLine> lines)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
            {
                TryAddLineIfPossible(element, lines);

                foreach (var property in element.EnumerateObject())
                    ExtractBomLinesRecursively(property.Value, lines);

                break;
            }
            case JsonValueKind.Array:
                foreach (var child in element.EnumerateArray())
                    ExtractBomLinesRecursively(child, lines);
                break;
        }
    }

    private static void TryAddLineIfPossible(JsonElement element, List<SapBomLine> lines)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return;

        var itemCode = ReadFirstString(element, "ItemCode", "itemCode", "Code", "CodeItem", "ChildItemCode");
        if (string.IsNullOrWhiteSpace(itemCode))
            return;

        var quantity = ReadFirstDecimal(element, "Quantity", "quantity", "BaseQuantity", "PlannedQuantity", "IssueQuantity");
        if (quantity <= 0)
            return;

        var issueType = ReadFirstString(element, "IssueMethod", "IssueType", "issueMethod", "issueType");

        lines.Add(new SapBomLine
        {
            ItemCode = itemCode.Trim(),
            Quantity = quantity,
            IssueType = issueType
        });
    }

    private static string NormalizeIssueType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Backflush";

        if (value.Contains("manual", StringComparison.OrdinalIgnoreCase))
            return "Manual";

        return "Backflush";
    }

    private static string EnsureTrailingSlash(string url)
        => url.EndsWith("/", StringComparison.Ordinal) ? url : $"{url}/";

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static string? ReadFirstString(JsonElement element, params string[] propertyNames)
    {
        foreach (var name in propertyNames)
        {
            if (TryGetPropertyIgnoreCase(element, name, out var value)
                && value.ValueKind == JsonValueKind.String)
            {
                var text = value.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                    return text;
            }
        }

        return null;
    }

    private static decimal ReadFirstDecimal(JsonElement element, params string[] propertyNames)
    {
        foreach (var name in propertyNames)
        {
            if (!TryGetPropertyIgnoreCase(element, name, out var value))
                continue;

            if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var fromNumber))
                return fromNumber;

            if (value.ValueKind == JsonValueKind.String
                && decimal.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var fromString))
            {
                return fromString;
            }
        }

        return 0m;
    }

    private sealed class SapBomLine
    {
        public string ItemCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string? IssueType { get; set; }
    }

    private string GetEnumString(GeneralItemStatus status)
    {
        switch (status)
        {
            case GeneralItemStatus.Draft:
                return "Draft";
            case GeneralItemStatus.Planned:
                return "Planned";
            case GeneralItemStatus.Released:
                return "Released";
            case GeneralItemStatus.Closed:
                return "Closed";
            case GeneralItemStatus.Failed:
                return "Failed";
            default:
                return "Unknown";
        }
    }
}
