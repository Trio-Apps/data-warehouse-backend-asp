using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface ITransferredRequestItemRepository : IBaseRepository<TransferredRequestItem>
{
    Task<GeneralResponse<IEnumerable<TransferredRequestItemDTO>>> GetByTransferredRequestItemByTransferredRequestIdAsync(int transferredRequestId);
    Task<GeneralResponse<PagedResult<TransferredRequestItemDTO>>> GetByTransferredRequestItemByTransferredRequestIdWithPaginationAsync(int transferredRequestId, string? status, int pageNumber, int pageSize);
    Task<GeneralResponse<TransferredRequestItemDTO>> AddTransferredRequestItemByTransferredRequestIdAsync(
        int transferredRequestId,
        bool isBarcode,
        DynamicBarcodesDto? dynamicDto,
        AddGeneralItemDto? dto);

    Task<GeneralResponse<TransferredRequestItemDTO>> UpdateTransferredRequestItemAsync(
        int transferredRequestItemId,
        UpdateGeneralItemDto dto);

    Task<GeneralResponse<TransferredRequestItemDTO>> DeleteTransferredRequestItemAsync(int transferredRequestItemId);
}
