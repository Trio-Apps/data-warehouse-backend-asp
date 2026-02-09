using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.PurchaseOrders;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes;

public class CountStockRepository : BaseRepository<CountStock>, ICountStockRepository
{
    public CountStockRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CountStock>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query().Where(cs => cs.WarehouseId == warehouseId).ToListAsync();
    }

    public async Task<GeneralResponse<PagedResult<CountStockDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.Warehouses.Where(e => e.WarehouseId == warehouseId)
            .AsNoTracking()
            .SelectMany(e => e.CountStocks);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(cs => new CountStockDTO
            {
                CountStockId = cs.CountStockId,
                Status = cs.Status.ToString(),
              
                UserId = cs.UserId,
                WarehouseId = warehouseId,
                Comment = cs.Comment,
                CreatedAt = cs.CreatedAt,
                PostingDate = cs.PostingDate,
           
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<CountStockDTO>>.SuccessResponse(
            new PagedResult<CountStockDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<List<NameStatus>>> GetCountStockStatus()
    {
        var statuses = new List<NameStatus>
        {
            new NameStatus { Id = 1, Name = "Draft" },
            new NameStatus { Id = 2, Name = "Pending" },
            new NameStatus { Id = 3, Name = "Completed" },
            new NameStatus { Id = 4, Name = "Approval" }
        };

        return await Task.FromResult(new GeneralResponse<List<NameStatus>>
        {
            Success = true,
            Message = "Count stock statuses retrieved successfully",
            Data = statuses
        });
    }

    public async Task<GeneralResponse<CountStockDTO>> AddCountStockByWarehouseIdAsync(string userId, AddCountStockDTO dto)
    {
        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.WarehouseId == dto.WarehouseId);
        if (warehouse == null)
            return GeneralResponse<CountStockDTO>.FailResponse("Warehouse is not found");

        var mapping = new CountStock
        {
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
           
            UserId = userId,
            WarehouseId = dto.WarehouseId,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow,
            PostingDate = dto.PostingDate,
         
        };

        var res = await AddAsync(mapping);
        await SaveChangesAsync();

        var model = new CountStockDTO
        {
            CountStockId = res.CountStockId,
            Status = res.Status.ToString(),
          
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            Comment = res.Comment,
            CreatedAt = res.CreatedAt,
            PostingDate = res.PostingDate,
         
        };

        return GeneralResponse<CountStockDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<CountStockDTO>> UpdateCountStockAsync(string userId, int countStockId, UpdateCountStockDTO dto)
    {
        var entity = await _context.CountStocks.FirstOrDefaultAsync(e => e.CountStockId == dto.CountStockId);

        if (entity.CountStockId != countStockId)
        {
            return GeneralResponse<CountStockDTO>.FailResponse("id not equal count stock id!");
        }
        if (entity == null)
        {
            return GeneralResponse<CountStockDTO>.FailResponse("not found");
        }

        entity.UserId = userId;
        entity.PostingDate = dto.PostingDate;
        entity.Comment = dto.Comment;
        await _context.SaveChangesAsync();

        var result = new CountStockDTO
        {
            CountStockId = entity.CountStockId,
           
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            PostingDate = entity.PostingDate,
          
        };

        return GeneralResponse<CountStockDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<IEnumerable<CountStockDTO>>> GetByStatusAsync(string status)
    {

        if (Enum.TryParse<GeneralStatus>(status, out var statusEnum))
        {
            var query = await Query().Where(cs => cs.Status == statusEnum)
             .Select(cs => new CountStockDTO
             {
                 CountStockId = cs.CountStockId,
                 Status = cs.Status.ToString(),
                 
                 UserId = cs.UserId,
                 WarehouseId = cs.WarehouseId,
                 Comment = cs.Comment,
                 CreatedAt = cs.CreatedAt,
                 PostingDate = cs.PostingDate,
                
             }).ToListAsync();

            return GeneralResponse<IEnumerable<CountStockDTO>>.SuccessResponse(query);
        }
        return GeneralResponse<IEnumerable<CountStockDTO>>.FailResponse("Not found any count to this status now!");
    }

    public async Task<IEnumerable<CountStock>> GetByUserIdAsync(string userId)
    {
        return await Query().Where(cs => cs.UserId == userId).ToListAsync();
    }

    public async Task<CountStock?> GetWithItemsAsync(int countStockId)
    {
        return await QueryIncluding(false, cs => cs.CountStockItem)
            .FirstOrDefaultAsync(cs => cs.CountStockId == countStockId);
    }

    public async Task<CountStock?> GetWithWarehouseAsync(int countStockId)
    {
        return await QueryIncluding(false, cs => cs.Warehouse)
            .FirstOrDefaultAsync(cs => cs.CountStockId == countStockId);
    }

    public async Task<IEnumerable<CountStock>> GetPendingCountsAsync()
    {
        return await Query().Where(cs => cs.Status == GeneralStatus.Draft || cs.Status == GeneralStatus.Processing).ToListAsync();
    }

    public async Task<IEnumerable<CountStock>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Query().Where(cs => cs.CreatedAt >= startDate && cs.CreatedAt <= endDate).ToListAsync();
    }
}
