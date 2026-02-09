using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Processes.OutSide;

public interface ISalesOrderItemService : IBaseService<SalesOrderItem>
{
    Task<IEnumerable<SalesOrderItem>> GetBySalesOrderIdAsync(int salesOrderId);
    Task<IEnumerable<SalesOrderItem>> GetByItemIdAsync(int itemId);
    Task<SalesOrderItem?> GetWithSalesOrderAsync(int salesOrderItemId);
    Task<SalesOrderItem?> GetWithItemAsync(int salesOrderItemId);
}
