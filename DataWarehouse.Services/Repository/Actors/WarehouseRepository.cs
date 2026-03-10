using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Core.Interfaces.Company;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.IServices.Auth;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Services.Repository.Based;
using DataWarehouse.Services.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Actors;

public class WarehouseRepository : BaseRepository<Warehouse>, IWarehouseRepository
{
    private const string GoodsReceiptType = "Goods Receipt";
    private const string GoodsIssueType = "Goods Issue";
    private const string TransferOutType = "Inventory Transfer Out";
    private const string TransferInType = "Inventory Transfer In";
    private const string CountingType = "Counting Posting";
    private const string ProductionReceiptType = "Production Receipt";
    private const string SalesDeliveryType = "Sales Delivery";
    private const string SalesReturnType = "Sales Return";

    private readonly IAuthServices authServices;
    private readonly IRoleServices roleServices;
    private readonly ISapSettingsRepository sapRepo;
    private readonly ICompanyCache companyCache;
    private readonly ISapCache sapCache;

    public WarehouseRepository(IAuthServices authServices, DataWarehouseDbContext context, IRoleServices roleServices, ISapSettingsRepository sapRepo, ICompanyCache companyCache, ISapCache sapCache) : base(context)
    {
        this.authServices = authServices;
        this.roleServices = roleServices;
        this.sapRepo = sapRepo;
        this.companyCache = companyCache;
        this.sapCache = sapCache;
    }

    public async Task<GeneralResponse<IEnumerable<WarehouseDTO>>> GetAllWarehouses(string userId, IList<string> roleNames)
    {

        var roles = await roleServices.GetUserRolesAsync(userId);


        var checkRole = await authServices.GetLoginContextAsync(userId, roles);

        var sapId = await sapCache.Get();

        List<WarehouseDTO> res;
        if (checkRole.IsGlobal)
        {

            if (sapId == null)
            {
                var companyId = await companyCache.Get();
                var sap = await _context.Saps.Where(c => c.CompanyId == companyId).FirstOrDefaultAsync();
                if (sap == null)
                    return GeneralResponse<IEnumerable<WarehouseDTO>>.FailResponse("Not Found Any Warehouses In This Company");

                await sapCache.UpdateSapUserClaimAsync(sap.SapId.ToString());
                sapId = sap.SapId;
            }

            res = await _context.Warehouses.Where(x => x.SapId == sapId).Select(d =>

            new WarehouseDTO
            {
                SapId = d.SapId,
                WarehouseId = d.WarehouseId,
                WarehouseName = d.WarehouseName,
            }).ToListAsync();
        }
       
        else
        {

            res = await _context.UserWarehouses.AsNoTracking().Where(uw=> uw.UserId == userId && uw.Warehouse.SapId==sapId).Select(d =>

            new WarehouseDTO
            {
                SapId = d.Warehouse.SapId,
                WarehouseId = d.WarehouseId,
                WarehouseName = d.Warehouse.WarehouseName,
            }).ToListAsync();



        }

           

        return  GeneralResponse<IEnumerable<WarehouseDTO>>.SuccessResponse(res);
    }
    public async Task<GeneralResponse<IEnumerable<WarehouseDTO>>> GetAllWarehousesForEmployeeAsync(string userId)
    {
        var sapId = await sapCache.Get();
        var res = _context.UserWarehouses.AsNoTracking().Where(x => x.UserId == userId).Select(d =>
        new WarehouseDTO
        {
            WarehouseId = d.WarehouseId,
            WarehouseName = d.Warehouse.WarehouseName
        });

        return GeneralResponse<IEnumerable<WarehouseDTO>>.SuccessResponse(res);
    }
   
    public async Task<int?> GetSap()
    {
      

        return await sapCache.Get();
    }
    

   public async Task<GeneralResponse<IEnumerable<WarehouseDTO>>> GetSapByIdAsync(int sapId)
    {

        var res = await _context.Warehouses.AsNoTracking().Where(w => w.SapId == sapId)
            .Select(e => new WarehouseDTO
            {
                WarehouseId = e.WarehouseId,
                WarehouseName = e.WarehouseName,
                SapId = e.SapId
            })
            .ToListAsync();

        return GeneralResponse<IEnumerable<WarehouseDTO>>.SuccessResponse(res);
      
    }

    public async Task<Warehouse?> GetByNameAsync(string warehouseCode)
    {
        return await Query().FirstOrDefaultAsync(w => w.WarehouseCode == warehouseCode);
    }


    public async Task<Warehouse?> GetWithUserWarehousesAsync(int warehouseId)
    {
        return await QueryIncluding(false, w => w.UserWarehouses)
            .FirstOrDefaultAsync(w => w.WarehouseId == warehouseId);
    }

    // by warehouse id only 

    public async Task<GeneralResponse<IEnumerable<ItemResponseDTO>>> GetItemsOfWarehouseAsync(
       int warehouseId)
    {

      

        var query = await _context.WarehouseItems
            .AsNoTracking()
            .Where(iw => iw.WarehouseId == warehouseId)
            .Select(iw => new ItemResponseDTO
            {
                ItemId = iw.Item.ItemId,
                ItemCode = iw.Item.ItemCode,
                ItemName = iw.Item.ItemName,
                PurchasePrice = iw.Item.PurchasePrice,
                SalesPrice = iw.Item.SalesPrice,
                UoM = iw.Item.UoM,
                UpdateDate = iw.Item.UpdateDate,
                WarehouseCode = iw.WarehouseCode,
                InStock = iw.InStock
            }).ToListAsync();

        return GeneralResponse<IEnumerable<ItemResponseDTO>>.SuccessResponse(query);

    }



    public async Task<GeneralResponse<PagedResult<ItemResponseDTO>>> GetItemsOfWarehouseAsync(
        int warehouseId,
         int pageNumber,
    int pageSize)
    {

        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
    pageSize   = pageSize   <= 0 ? 10 : pageSize;

    var query = _context.WarehouseItems
        .AsNoTracking()
        .Where(iw => iw.WarehouseId == warehouseId)
        .Select(iw => new ItemResponseDTO
        {
            ItemId        = iw.Item.ItemId,
            ItemCode      = iw.Item.ItemCode,
            ItemName      = iw.Item.ItemName,
            PurchasePrice = iw.Item.PurchasePrice,
            SalesPrice    = iw.Item.SalesPrice,
            UoM           = iw.Item.UoM,
            UpdateDate    = iw.Item.UpdateDate,
            WarehouseCode = iw.WarehouseCode,
            InStock       = iw.InStock
        });

    var totalRecords = await query.CountAsync();

    var data = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

        return GeneralResponse<PagedResult<ItemResponseDTO>>.SuccessResponse(new PagedResult<ItemResponseDTO>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            Data = data
        });

    }


    //GetItemsByWarehouseIdWithItemCodeAndName
    public async Task<GeneralResponse<PagedResult<ItemResponseDTO>>>
      GetItemsByWarehouseIdWithItemCodeAndName(
          int warehouseId,
          string? itemCode,
          string? itemName,
          int pageNumber,
          int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.WarehouseItems
            .AsNoTracking()
            .Where(iw => iw.WarehouseId == warehouseId);

        // 🔹 Filtering
        if (!string.IsNullOrWhiteSpace(itemCode))
        {
            query = query.Where(iw =>
                iw.Item.ItemCode.Contains(itemCode));
        }

        if (!string.IsNullOrWhiteSpace(itemName))
        {
            query = query.Where(iw =>
        iw.Item.ItemName.Contains(itemName));
        }

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderBy(iw => iw.Item.ItemName) // مهم جدًا مع Pagination
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(iw => new ItemResponseDTO
            {
                ItemId = iw.Item.ItemId,
                ItemCode = iw.Item.ItemCode,
                ItemName = iw.Item.ItemName,
                PurchasePrice = iw.Item.PurchasePrice,
                SalesPrice = iw.Item.SalesPrice,
                UoM = iw.Item.UoM,
                UpdateDate = iw.Item.UpdateDate,
                WarehouseCode = iw.WarehouseCode,
                InStock = iw.InStock
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<ItemResponseDTO>>.SuccessResponse(
            new PagedResult<ItemResponseDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<PagedResult<TransactionReportItemDto>>> GetTransactionReportAsync(
        TransactionReportFilterDto filter)
    {
        var warehouseExists = await _context.Warehouses
            .AsNoTracking()
            .AnyAsync(w => w.WarehouseId == filter.WarehouseId);

        if (!warehouseExists)
            return GeneralResponse<PagedResult<TransactionReportItemDto>>.FailResponse("Warehouse not found");

        var pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;

        var allowedTypes = ResolveTransactionTypeFilter(filter.TransactionType);
        if (allowedTypes == null)
            return GeneralResponse<PagedResult<TransactionReportItemDto>>
                .FailResponse("Invalid transaction type filter");

        var goodsReceiptQuery = _context.ReceiptPurchaseOrderItems
            .AsNoTracking()
            .Where(x => x.ReceiptPurchaseOrder.WarehouseId == filter.WarehouseId)
            .Select(x => new TransactionReportItemDto
            {
                Document = "Goods Receipt PO",
                TransactionType = GoodsReceiptType,
                Direction = "In",
                BaseReference = x.ReceiptPurchaseOrder.DocNum ?? x.ReceiptPurchaseOrder.ReceiptPurchaseOrderId,
                TransactionDate = x.ReceiptPurchaseOrder.CreatedAt,
                WarehouseId = x.ReceiptPurchaseOrder.WarehouseId,
                WarehouseCode = x.ReceiptPurchaseOrder.Warehouse.WarehouseCode,
                ItemId = x.ItemId,
                ItemCode = x.Item.ItemCode,
                ItemName = x.Item.ItemName,
                Quantity = x.Quantity
            });

        var goodsIssueQuery = _context.GoodsReturnOrderItems
            .AsNoTracking()
            .Where(x => x.GoodsReturnOrder.WarehouseId == filter.WarehouseId)
            .Select(x => new TransactionReportItemDto
            {
                Document = "Goods Return",
                TransactionType = GoodsIssueType,
                Direction = "Out",
                BaseReference = x.GoodsReturnOrder.DocNum ?? x.GoodsReturnOrder.GoodsReturnOrderId,
                TransactionDate = x.GoodsReturnOrder.CreatedAt,
                WarehouseId = x.GoodsReturnOrder.WarehouseId,
                WarehouseCode = x.GoodsReturnOrder.Warehouse.WarehouseCode,
                ItemId = x.ItemId,
                ItemCode = x.Item.ItemCode,
                ItemName = x.Item.ItemName,
                Quantity = x.Quantity
            });

        var transferOutQuery = _context.TransferredItems
            .AsNoTracking()
            .Where(x => x.TransferredStock.WarehouseId == filter.WarehouseId)
            .Select(x => new TransactionReportItemDto
            {
                Document = "Inventory Transfer",
                TransactionType = TransferOutType,
                Direction = "Out",
                BaseReference = x.TransferredStock.DocNum ?? x.TransferredStock.TransferredStockId,
                TransactionDate = x.TransferredStock.CreatedAt,
                WarehouseId = x.TransferredStock.WarehouseId,
                WarehouseCode = x.TransferredStock.Warehouse.WarehouseCode,
                ItemId = x.ItemId,
                ItemCode = x.Item.ItemCode,
                ItemName = x.Item.ItemName,
                Quantity = x.Quantity
            });

        var transferInQuery = _context.ReceivedItems
            .AsNoTracking()
            .Where(x => x.ReceivedStock.WarehouseId == filter.WarehouseId)
            .Select(x => new TransactionReportItemDto
            {
                Document = "Inventory Transfer",
                TransactionType = TransferInType,
                Direction = "In",
                BaseReference = x.ReceivedStock.DocNum ?? x.ReceivedStock.ReceivedStockId,
                TransactionDate = x.ReceivedStock.CreatedAt,
                WarehouseId = x.ReceivedStock.WarehouseId,
                WarehouseCode = x.ReceivedStock.Warehouse.WarehouseCode,
                ItemId = x.ItemId,
                ItemCode = x.Item.ItemCode,
                ItemName = x.Item.ItemName,
                Quantity = x.Quantity
            });

        var countingQuery = _context.CountStockItems
            .AsNoTracking()
            .Where(x => x.CountStock.WarehouseId == filter.WarehouseId)
            .Select(x => new TransactionReportItemDto
            {
                Document = "Stock Counting",
                TransactionType = CountingType,
                Direction = "Count",
                BaseReference = x.CountStock.DocNum ?? x.CountStock.CountStockId,
                TransactionDate = x.CountStock.CreatedAt,
                WarehouseId = x.CountStock.WarehouseId,
                WarehouseCode = x.CountStock.Warehouse.WarehouseCode,
                ItemId = x.ItemId,
                ItemCode = x.Item.ItemCode,
                ItemName = x.Item.ItemName,
                Quantity = x.Quantity
            });

        var productionReceiptQuery = _context.ProductionReceipts
            .AsNoTracking()
            .Where(x => x.ProductionOrderItem.ProductionOrder.WarehouseId == filter.WarehouseId)
            .Select(x => new TransactionReportItemDto
            {
                Document = "Production Receipt",
                TransactionType = ProductionReceiptType,
                Direction = "In",
                BaseReference = x.ProductionOrderItem.AbsoluteEntry ?? x.ProductionOrderItem.ProductionOrderId,
                TransactionDate = x.CreatedAt,
                WarehouseId = x.ProductionOrderItem.ProductionOrder.WarehouseId,
                WarehouseCode = x.ProductionOrderItem.ProductionOrder.Warehouse.WarehouseCode,
                ItemId = x.ProductionOrderItem.ItemId,
                ItemCode = x.ProductionOrderItem.Item.ItemCode,
                ItemName = x.ProductionOrderItem.Item.ItemName,
                Quantity = x.ProducedQuantity
            });

        var salesDeliveryQuery = _context.DeliveryNoteItems
            .AsNoTracking()
            .Where(x => x.DeliveryNoteOrder.WarehouseId == filter.WarehouseId)
            .Select(x => new TransactionReportItemDto
            {
                Document = "Delivery Note",
                TransactionType = SalesDeliveryType,
                Direction = "Out",
                BaseReference = x.DeliveryNoteOrder.DocNum ?? x.DeliveryNoteOrder.DeliveryNoteOrderId,
                TransactionDate = x.DeliveryNoteOrder.CreatedAt,
                WarehouseId = x.DeliveryNoteOrder.WarehouseId,
                WarehouseCode = x.DeliveryNoteOrder.Warehouse.WarehouseCode,
                ItemId = x.ItemId,
                ItemCode = x.Item.ItemCode,
                ItemName = x.Item.ItemName,
                Quantity = x.Quantity
            });

        var salesReturnQuery = _context.SalesReturnOrderItems
            .AsNoTracking()
            .Where(x => x.SalesReturnOrder.WarehouseId == filter.WarehouseId)
            .Select(x => new TransactionReportItemDto
            {
                Document = "Sales Return",
                TransactionType = SalesReturnType,
                Direction = "In",
                BaseReference = x.SalesReturnOrder.DocNum ?? x.SalesReturnOrder.SalesReturnOrderId,
                TransactionDate = x.SalesReturnOrder.CreatedAt,
                WarehouseId = x.SalesReturnOrder.WarehouseId,
                WarehouseCode = x.SalesReturnOrder.Warehouse.WarehouseCode,
                ItemId = x.ItemId,
                ItemCode = x.Item.ItemCode,
                ItemName = x.Item.ItemName,
                Quantity = x.Quantity
            });

        var allTransactionsQuery = goodsReceiptQuery
            .Concat(goodsIssueQuery)
            .Concat(transferOutQuery)
            .Concat(transferInQuery)
            .Concat(countingQuery)
            .Concat(productionReceiptQuery)
            .Concat(salesDeliveryQuery)
            .Concat(salesReturnQuery);

        if (filter.FromDate.HasValue)
        {
            var fromDate = filter.FromDate.Value.Date;
            allTransactionsQuery = allTransactionsQuery.Where(x => x.TransactionDate >= fromDate);
        }

        if (filter.ToDate.HasValue)
        {
            var toExclusive = filter.ToDate.Value.Date.AddDays(1);
            allTransactionsQuery = allTransactionsQuery.Where(x => x.TransactionDate < toExclusive);
        }

        if (!string.IsNullOrWhiteSpace(filter.ItemCodeOrName))
        {
            var term = filter.ItemCodeOrName.Trim();
            allTransactionsQuery = allTransactionsQuery
                .Where(x => x.ItemCode.Contains(term) || x.ItemName.Contains(term));
        }

        if (allowedTypes.Count > 0)
        {
            allTransactionsQuery = allTransactionsQuery
                .Where(x => allowedTypes.Contains(x.TransactionType));
        }

        var totalRecords = await allTransactionsQuery.CountAsync();

        var data = await allTransactionsQuery
            .OrderByDescending(x => x.TransactionDate)
            .ThenBy(x => x.TransactionType)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return GeneralResponse<PagedResult<TransactionReportItemDto>>.SuccessResponse(new PagedResult<TransactionReportItemDto>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        });
    }

    public async Task<GeneralResponse<PagedResult<InWarehouseReportItemDto>>> GetInWarehouseReportAsync(
        InWarehouseReportFilterDto filter)
    {
        var warehouseExists = await _context.Warehouses
            .AsNoTracking()
            .AnyAsync(w => w.WarehouseId == filter.WarehouseId);

        if (!warehouseExists)
            return GeneralResponse<PagedResult<InWarehouseReportItemDto>>.FailResponse("Warehouse not found");

        var pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;

        var query = _context.WarehouseItems
            .AsNoTracking()
            .Where(iw => iw.WarehouseId == filter.WarehouseId);

        if (!filter.ShowItemsWithNoQuantityInStock)
        {
            query = query.Where(iw => (iw.InStock ?? 0) > 0);
        }

        if (!string.IsNullOrWhiteSpace(filter.ItemCodeOrName))
        {
            var term = filter.ItemCodeOrName.Trim();
            query = query.Where(iw => iw.Item.ItemCode.Contains(term) || iw.Item.ItemName.Contains(term));
        }

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderBy(iw => iw.Item.ItemCode)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(iw => new InWarehouseReportItemDto
            {
                ItemId = iw.ItemId,
                ItemCode = iw.Item.ItemCode,
                ItemName = iw.Item.ItemName,
                UoM = iw.Item.UoM,
                InStock = iw.InStock,
                WarehouseId = iw.WarehouseId,
                WarehouseCode = iw.WarehouseCode
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<InWarehouseReportItemDto>>.SuccessResponse(new PagedResult<InWarehouseReportItemDto>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        });
    }

    public async Task<GeneralResponse<TransactionReportSourcesCountDto>> GetTransactionReportSourcesCountAsync(
        int warehouseId)
    {
        var warehouseExists = await _context.Warehouses
            .AsNoTracking()
            .AnyAsync(w => w.WarehouseId == warehouseId);

        if (!warehouseExists)
            return GeneralResponse<TransactionReportSourcesCountDto>.FailResponse("Warehouse not found");

        // EF Core DbContext does not support concurrent operations on the same instance.
        var goodsReceiptCount = await _context.ReceiptPurchaseOrderItems
            .AsNoTracking()
            .CountAsync(x => x.ReceiptPurchaseOrder.WarehouseId == warehouseId);

        var goodsIssueCount = await _context.GoodsReturnOrderItems
            .AsNoTracking()
            .CountAsync(x => x.GoodsReturnOrder.WarehouseId == warehouseId);

        var transferOutCount = await _context.TransferredItems
            .AsNoTracking()
            .CountAsync(x => x.TransferredStock.WarehouseId == warehouseId);

        var transferInCount = await _context.ReceivedItems
            .AsNoTracking()
            .CountAsync(x => x.ReceivedStock.WarehouseId == warehouseId);

        var countingCount = await _context.CountStockItems
            .AsNoTracking()
            .CountAsync(x => x.CountStock.WarehouseId == warehouseId);

        var productionReceiptCount = await _context.ProductionReceipts
            .AsNoTracking()
            .CountAsync(x => x.ProductionOrderItem.ProductionOrder.WarehouseId == warehouseId);

        var salesDeliveryCount = await _context.DeliveryNoteItems
            .AsNoTracking()
            .CountAsync(x => x.DeliveryNoteOrder.WarehouseId == warehouseId);

        var salesReturnCount = await _context.SalesReturnOrderItems
            .AsNoTracking()
            .CountAsync(x => x.SalesReturnOrder.WarehouseId == warehouseId);

        var dto = new TransactionReportSourcesCountDto
        {
            WarehouseId = warehouseId,
            GoodsReceiptCount = goodsReceiptCount,
            GoodsIssueCount = goodsIssueCount,
            TransferOutCount = transferOutCount,
            TransferInCount = transferInCount,
            CountingCount = countingCount,
            ProductionReceiptCount = productionReceiptCount,
            SalesDeliveryCount = salesDeliveryCount,
            SalesReturnCount = salesReturnCount
        };

        dto.TotalCount =
            dto.GoodsReceiptCount +
            dto.GoodsIssueCount +
            dto.TransferOutCount +
            dto.TransferInCount +
            dto.CountingCount +
            dto.ProductionReceiptCount +
            dto.SalesDeliveryCount +
            dto.SalesReturnCount;

        return GeneralResponse<TransactionReportSourcesCountDto>.SuccessResponse(dto);
    }

    private static IReadOnlyList<string>? ResolveTransactionTypeFilter(string? transactionType)
    {
        if (string.IsNullOrWhiteSpace(transactionType))
            return Array.Empty<string>();

        var normalized = transactionType.Trim().ToLowerInvariant();

        return normalized switch
        {
            "all" => Array.Empty<string>(),
            "goods receipt" or "goods-receipt" or "receipt" => new[] { GoodsReceiptType },
            "goods issue" or "goods-issue" or "issue" => new[] { GoodsIssueType },
            "transfer" or "transfers" or "inventory transfer" => new[] { TransferOutType, TransferInType },
            "inventory transfer out" or "transfer out" => new[] { TransferOutType },
            "inventory transfer in" or "transfer in" => new[] { TransferInType },
            "counting" or "counting posting" or "stock counting" => new[] { CountingType },
            "production" or "production receipt" => new[] { ProductionReceiptType },
            "sales" or "sales-related" or "sales related" => new[] { SalesDeliveryType, SalesReturnType },
            "sales delivery" => new[] { SalesDeliveryType },
            "sales return" => new[] { SalesReturnType },
            _ => null
        };
    }


    public async Task<bool> ExistsByNameAsync(string warehouseName)
    {
        return await ExistsAsync(w => w.WarehouseName == warehouseName);
    }
}
