using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Based;

public interface IBaseRepository<T>
{
    // Basic Queries
    IQueryable<T> Query(bool tracking = false);

    IQueryable<T> QueryIncluding(
        bool tracking = false,
        params Expression<Func<T, object>>[] includes);

    //Pagination
    Task<GeneralResponse<PagedResult<T>>> PaginationAsync(
    Expression<Func<T, bool>> filter,
    int pageNumber,
    int pageSize);
    Task<PagedResult<T>> PaginationWithoutFilterAsync(
  int pageNumber,
  int pageSize);

    Task<T?> GetByIdAsync(Expression<Func<T, bool>> del);

    Task<bool> ExistsAsync(Expression<Func<T, bool>> del);

    Task<int> SaveChangesAsync();
    // Add
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>>  AddRangeAsync(ICollection<T> entity);

    // Get all By Id
    Task<T> GetByIdAsync(int id);

    // Get all
    Task<IEnumerable<T>> GetAllAsync();

    // Update
    Task<T> UpdateAsync(T entity);
    Task<IEnumerable<T>> UpdateRange(ICollection<T> entity);

    // Delete
    Task<T> DeleteAsync(int id);
    Task<IEnumerable<T>> DeleteRange(ICollection<T> entities);
}