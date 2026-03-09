using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Sync;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Sync;

public interface ISapSyncResetRepository
{
    Task<GeneralResponse<bool>> ResetItemSyncAsync(int sapId);
    Task<GeneralResponse<bool>> ResetWarehouseSyncAsync(int sapId);
    Task<GeneralResponse<bool>> ResetPurchaseSyncAsync(int sapId);
    Task<GeneralResponse<bool>> ResetCountSyncAsync(int sapId);
    Task<GeneralResponse<bool>> ResetBusinessPartnersSyncAsync(int sapId);
    Task<GeneralResponse<bool>> ResetItemUomGroupSyncAsync(int sapId);
    Task<GeneralResponse<bool>> ResetSalesSyncAsync(int sapId);
    Task<GeneralResponse<List<SapSyncStateDto>>> GetCompanySyncStateAsync();
}
