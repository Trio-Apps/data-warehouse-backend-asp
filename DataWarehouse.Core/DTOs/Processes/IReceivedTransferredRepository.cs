using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using System.Threading.Tasks;

namespace DataWarehouse.Core.DTOs.Processes;

public interface IReceivedTransferredRepository
{
    Task<GeneralResponse<ProcessItemIsProgressDto>> UpdateReceivedQuantitiesAsync(string userId, ReceiveTransferredStockDTO dto);
    Task<GeneralResponse<ProcessItemIsProgressDto>> CompleteReceivingStatusIfDraftAsync(string userId, int transferredStockId);
}
