using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Actors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Actors;

public interface ICustomerService : IBaseService<Customer>
{
    Task<Customer?> GetByNameAsync(string customerName);
    Task<IEnumerable<Customer>> GetActiveCustomersAsync();
    Task<Customer?> GetWithSalesOrdersAsync(int customerId);
    Task<bool> ExistsByNameAsync(string customerName);
    Task<IEnumerable<Customer>> SearchByNameAsync(string searchTerm);
}
