using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using Microsoft.EntityFrameworkCore;
using Polly;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes;

public class ReceivedTransferredRepository : IReceivedTransferredRepository
{
    private readonly IProcessItemIsProgressRepository progressRepository;
    private readonly DataWarehouseDbContext dbContext;
    private readonly ITransferredStockRepository transferredStockRepository;

    public ReceivedTransferredRepository(IProcessItemIsProgressRepository progressRepository, DataWarehouseDbContext dbContext, ITransferredStockRepository transferredStockRepository)
    {
        this.progressRepository = progressRepository;
        this.dbContext = dbContext;
        this.transferredStockRepository = transferredStockRepository;
    }

    public async Task<GeneralResponse<ProcessItemIsProgressDto>> UpdateReceivedQuantitiesAsync(string userId, ReceiveTransferredStockDTO dto)
    {
        if (dto == null)
            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Request body is required");

        if (dto.TransferredStockId <= 0)
            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("TransferredStockId must be greater than 0");

        if (dto.Items == null || dto.Items.Count == 0)
            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Items are required");

        var hasDuplicateItems = dto.Items
            .GroupBy(i => i.TransferredItemId)
            .Any(g => g.Count() > 1);

        if (hasDuplicateItems)
            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Duplicate TransferredItemId found in items");

        var transferredStock = await dbContext.TransferredStocks
            .Include(ts => ts.TransferredItems)
                .ThenInclude(i => i.TransferredStockBatches)
            .FirstOrDefaultAsync(ts => ts.TransferredStockId == dto.TransferredStockId);

        if (transferredStock == null)
            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Transferred stock not found");

        foreach (var itemDto in dto.Items)
        {
            if (itemDto.TransferredItemId <= 0)
                return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("TransferredItemId must be greater than 0");

            if (itemDto.Quantity <= 0)
                return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Quantity must be greater than zero");

            var item = transferredStock.TransferredItems
                .FirstOrDefault(i => i.TransferredItemId == itemDto.TransferredItemId);

            if (item == null)
                return GeneralResponse<ProcessItemIsProgressDto>.FailResponse($"Transferred item not found: {itemDto.TransferredItemId}");

            if (itemDto.Quantity > item.Quantity)
                return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Received quantity cannot exceed transferred quantity");

            item.ReceivedQuantity = itemDto.Quantity;

            if (itemDto.Batches != null && itemDto.Batches.Count > 0)
            {
                if (item.TransferredStockBatches == null || item.TransferredStockBatches.Count == 0)
                    return GeneralResponse<ProcessItemIsProgressDto>.FailResponse($"No batches found for item: {itemDto.TransferredItemId}");

                var hasDuplicateBatches = itemDto.Batches
                    .GroupBy(b => b.TransferredStockBatchId)
                    .Any(g => g.Count() > 1);

                if (hasDuplicateBatches)
                    return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Duplicate TransferredStockBatchId found in batches");

                decimal totalBatchQty = 0;

                foreach (var batchDto in itemDto.Batches)
                {
                    if (batchDto.TransferredStockBatchId <= 0)
                        return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("TransferredStockBatchId must be greater than 0");

                    if (batchDto.Quantity <= 0)
                        return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Batch quantity must be greater than zero");

                    var batch = item.TransferredStockBatches
                        .FirstOrDefault(b => b.TransferredStockBatchId == batchDto.TransferredStockBatchId);

                    if (batch == null)
                        return GeneralResponse<ProcessItemIsProgressDto>.FailResponse(
                            $"Transferred stock batch not found: {batchDto.TransferredStockBatchId}");

                    if (batchDto.Quantity > batch.Quantity)
                        return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Received batch quantity cannot exceed transferred batch quantity");

                    batch.ReceivedQuantity = batchDto.Quantity;
                    totalBatchQty += batchDto.Quantity;
                }

                if (totalBatchQty > itemDto.Quantity)
                    return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Total batch quantity cannot exceed item received quantity");
            }
        }

        transferredStock.ReceivingStatus = dto.IsDraft ? ReceivingStatus.Draft : ReceivingStatus.Completed;

        await dbContext.SaveChangesAsync();

        var transferredStockRepo = await transferredStockRepository
      .GetTransferredStockByIdAsync(userId, dto.TransferredStockId);

        if (transferredStockRepo.Data?.ApprovalStatus == ApprovalStatus.Approved.ToString() && !dto.IsDraft)
        {
            var result = await GetProcessItemIsProgressForTransferredStock(dto.TransferredStockId);

            return GeneralResponse<ProcessItemIsProgressDto>.SuccessResponse(
                result,
                "Received quantities updated successfully"
            );
        }

        // ❌ لو الشرط لم يتحقق
        return GeneralResponse<ProcessItemIsProgressDto>.FailResponse(
            "Cannot process request. Approval status must be Approved and not a draft."
        );
    }

    public async Task<GeneralResponse<ProcessItemIsProgressDto>> CompleteReceivingStatusIfDraftAsync(string userId, int transferredStockId)
    {
        if (transferredStockId <= 0)
            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("TransferredStockId must be greater than 0");

        var transferredStockRepo = await transferredStockRepository.GetTransferredStockByIdAsync(userId, transferredStockId);

        if (transferredStockRepo.Data == null)
            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Transferred stock not found");

        if (transferredStockRepo.Data.ReceivingStatus != ReceivingStatus.Draft.ToString())
            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Receiving status is not draft");

        var entity = await dbContext.TransferredStocks
            .FirstOrDefaultAsync(ts => ts.TransferredStockId == transferredStockId);

        if (entity == null)
            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Transferred stock not found");

       
        entity.ReceivingStatus = ReceivingStatus.Completed;
        await dbContext.SaveChangesAsync();


        if (transferredStockRepo.Data.ApprovalStatus == ApprovalStatus.Approved.ToString())
        {
            var result = await GetProcessItemIsProgressForTransferredStock(transferredStockId);

            return GeneralResponse<ProcessItemIsProgressDto>.SuccessResponse(
                result,
                "Received quantities updated successfully"
            );

        }


        // ❌ لو الشرط لم يتحقق
        return GeneralResponse<ProcessItemIsProgressDto>.FailResponse(
            "Cannot process request. Approval status must be Approved and not a draft."
        );
    }

    private async Task<ProcessItemIsProgressDto> GetProcessItemIsProgressForTransferredStock(int transferredStockId)
    {
        var res = await progressRepository.GetProcessItemIsProgressAsync(
            ProcessType.Transferred,
            transferredStockId);

        return res;
    }

}
