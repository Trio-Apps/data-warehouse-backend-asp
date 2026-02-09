using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Processes.OutSide;

public interface ISalesOrderService : IBaseService<SalesOrder>
{
    Task<IEnumerable<SalesOrder>> GetByWarehouseIdAsync(int warehouseId);
    Task<IEnumerable<SalesOrder>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<SalesOrder>> GetByStatusAsync(string status);
    Task<IEnumerable<SalesOrder>> GetByUserIdAsync(string userId);
    Task<SalesOrder?> GetWithItemsAsync(int salesOrderId);
    Task<SalesOrder?> GetWithCustomerAsync(int salesOrderId);
    Task<SalesOrder?> GetWithWarehouseAsync(int salesOrderId);
    Task<IEnumerable<SalesOrder>> GetPendingOrdersAsync();
    Task<IEnumerable<SalesOrder>> GetDraftOrdersAsync();
    Task<IEnumerable<SalesOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}
