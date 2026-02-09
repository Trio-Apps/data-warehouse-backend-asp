using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface IProductionReceiptRepository : IBaseRepository<ProductionReceipt>
{
    Task<GeneralResponse<IEnumerable<ProductionReceiptDTO>>> GetByProductionOrderItemIdAsync(int productionOrderItemId);
    Task<bool> CheckItemHasBatchsAsync(int itemId);

    Task<GeneralResponse<PagedResult<ProductionReceiptDTO>>> GetByProductionItemIdWithPaginationAsync(int productionItemId, int skip, int pageSize);
    Task<GeneralResponse<ProductionReceiptDTO>> AddByProductionItemIdAsync(int productionOrderItemId,AddProductionReceiptDTO pr);

    Task<GeneralResponse<ProductionReceiptDTO>> UpdateProductionReceiptAsync(int productionReceiptId, UpdateProductionReceiptDTO dto);

}

