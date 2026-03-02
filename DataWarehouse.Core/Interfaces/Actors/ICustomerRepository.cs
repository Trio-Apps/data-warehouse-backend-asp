using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Actors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Actors;

public interface ICustomerRepository : IBaseRepository<Customer>
{
    Task<GeneralResponse<PagedResult<Customer>>> GetCustomersAsync(
           string? customerCode,
           string? customerName,
           int pageNumber = 1,
           int pageSize = 20);
    Task<GeneralResponse<Customer>> GetCustomerByIdAsync(int id);
    Task<GeneralResponse<Customer>> GetByNameAsync(string customerName);
    Task<GeneralResponse<IEnumerable<Customer>>> GetActiveCustomersAsync();
    Task<GeneralResponse<IEnumerable<Customer>>> SearchByNameAsync(string searchTerm);
    Task<GeneralResponse<CustomerDTO>> GetWithSalesOrdersAsync(int customerId);
    Task<GeneralResponse<Customer>> AddCustomerAsync(CustomerDTO dto);
    Task<GeneralResponse<Customer>> UpdateCustomerAsync(int id, CustomerDTO dto);
    Task<GeneralResponse<bool>> DeleteCustomerAsync(int id);
}
