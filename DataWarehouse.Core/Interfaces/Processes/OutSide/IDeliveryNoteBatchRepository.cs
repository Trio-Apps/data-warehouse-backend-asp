using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes.OutSide
{
    public interface IDeliveryNoteBatchRepository : IBaseRepository<DeliveryNoteBatch>
    {
        Task<GeneralResponse<IEnumerable<DeliveryNoteBatchDTO>>> GetByDeliveryNoteItemIdAsync(int deliveryNoteItemId);

        Task<GeneralResponse<PagedResult<DeliveryNoteBatchDTO>>> GetByDeliveryNoteItemIdWithPaginationAsync(
            int deliveryNoteItemId, int pageNumber, int pageSize);

        Task<GeneralResponse<DeliveryNoteBatchDTO>> AddByDeliveryNoteItemIdAsync(
            int deliveryNoteItemId, GeneralBatchDto dto);

        Task<GeneralResponse<DeliveryNoteBatchDTO>> UpdateDeliveryNoteBatchAsync(
            int deliveryNoteBatchId, UpdateGeneralBatchDto dto);

        Task<GeneralResponse<DeliveryNoteBatchDTO>> DeleteDeliveryNoteBatchAsync(int deliveryNoteBatchId);
    }

}
