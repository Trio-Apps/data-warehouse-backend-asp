using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Based;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Services.Based;

public class BaseService<T> : IBaseService<T> where T : class
{
    protected readonly IBaseRepository<T> _repository;

    public BaseService(IBaseRepository<T> repository)
    {
        _repository = repository;
    }

    public IBaseRepository<T> Repository => _repository;

    #region Basic Queries
    public IQueryable<T> Query(bool tracking = false)
    {
        return _repository.Query(tracking);
    }

    public IQueryable<T> QueryIncluding(bool tracking = false, params Expression<Func<T, object>>[] includes)
    {
        return _repository.QueryIncluding(tracking, includes);
    }
    #endregion

    #region Pagination
    public async Task<GeneralResponse<PagedResult<T>>> PaginationAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize)
    {

        var  res = await _repository.PaginationAsync(filter, pageNumber, pageSize);


        return res;
    }
    #endregion

    #region Expression Methods
    public async Task<T?> GetByIdAsync(Expression<Func<T, bool>> del)
    {
        return await _repository.GetByIdAsync(del);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> del)
    {
        return await _repository.ExistsAsync(del);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _repository.SaveChangesAsync();
    }
    #endregion

    #region CRUD Operations
    // ➕ Add
    public async Task<T> AddAsync(T entity)
    {
        return await _repository.AddAsync(entity);
    }

    public async Task<IEnumerable<T>> AddRangeAsync(ICollection<T> entities)
    {
        return await _repository.AddRangeAsync(entities);
    }

    // 🔍 GetById
    public async Task<T> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    // 📋 GetAll
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    // ✏️ Update
    public Task<T> UpdateAsync(T entity)
    {
        return _repository.UpdateAsync(entity);
    }

    public async Task<IEnumerable<T>> UpdateRange(ICollection<T> entities)
    {
        return await _repository.UpdateRange(entities);
    }

    // ❌ Delete
    public async Task<T> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<T>> DeleteRange(ICollection<T> entities)
    {
        return await _repository.DeleteRange(entities);
    }
    #endregion
}
