using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.SapRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Based;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
  
    protected readonly DataWarehouseDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(DataWarehouseDbContext context)
    {
       
        _context = context;
        _dbSet = _context.Set<T>();
    }

    #region CRUD Operations
    // ➕ Add
    public async Task<T> AddAsync(T entity)
    {

        await _dbSet.AddAsync(entity);
        return entity;
    }
    public async Task<IEnumerable<T>> AddRangeAsync(ICollection<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        return entities;
    }


    // 🔍 GetAllById
    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    // 📋 GetAll
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }



    // ✏️ Update
    public Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.FromResult(entity);
    }
    // ✏️ Update All
    public async Task<IEnumerable<T>> UpdateRange(ICollection<T> entities)
    {

        _dbSet.UpdateRange(entities);
        return entities;

    }

    // ❌ Delete
    public async Task<T?> DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null)
            return null;

        _dbSet.Remove(entity);
        return entity;
    }

    public async Task<IEnumerable<T>> DeleteRange(ICollection<T> entities)
    {
        _dbSet.RemoveRange(entities);
        return entities;
    }



    #endregion

    #region Pagination
    public async Task<GeneralResponse<PagedResult<T>>> PaginationAsync(
    Expression<Func<T, bool>> filter,
    int pageNumber,
    int pageSize)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = _dbSet.Where(filter);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return GeneralResponse<PagedResult<T>>.SuccessResponse(new PagedResult<T>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        }
        );
    }

    public async Task<PagedResult<T>> PaginationWithIncludeAndFilterAsync(
  Expression<Func<T, bool>> filter,
  int pageNumber,
  int pageSize, params Expression<Func<T, object>>[] includes)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;
        IQueryable<T> query = _dbSet;
        if (includes != null && includes.Any())
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        // ============================
        // Apply Filter
        // ============================
        if (filter != null)
        {
            query = query.Where(filter);
        }

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<T>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        };
    }
    public async Task<PagedResult<T>> PaginationWithFilterAsync(
  Expression<Func<T, bool>> filter,
  int pageNumber,
  int pageSize)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = _dbSet.Where(filter);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<T>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        };
    }

    public async Task<PagedResult<T>> PaginationWithoutFilterAsync(
  int pageNumber,
  int pageSize)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = _dbSet;

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<T>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        };
    }
    #endregion

    #region Expression Methods
    public async Task<T?> GetByIdAsync(Expression<Func<T, bool>> del)
    {
        return await _dbSet.FirstOrDefaultAsync(del);
    }



    public IQueryable<T> Query(bool tracking = false)
    {
        IQueryable<T> query = _dbSet;

        if (!tracking)
            query = query.AsNoTracking();

        return query;
    }

   
    public IQueryable<T> QueryIncluding(
    bool tracking = false,
    params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        if (!tracking)
            query = query.AsNoTracking();
        foreach (var include in includes)
            query = query.Include(include);

        return query;
    }






    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> del)
    {
        return await _dbSet.AnyAsync(del);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    #endregion

    #region processes

    public async Task<ProcessItemIsProgress> GetProcessItem(int OrderId, ProcessType type, CancellationToken cancellationToken = default)
    {
        return await _context.ProcessItemIsProgresses
 .AsNoTracking()
 .Where(p =>
     p.ProcessType == type &&
     p.ReferenceId == OrderId)
 .OrderByDescending(p => p.ProcessItemIsProgressId) // آخر حالة
 .FirstOrDefaultAsync(cancellationToken);
    }


  


   
    #endregion
}
