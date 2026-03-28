using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Queue;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.Processes;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReceivedTransferredController : ControllerBase
{
    private readonly ISapJobQueuer jobQueuer;
    private readonly IReceivedTransferredRepository repository;
    private readonly ILogger<ReceivedTransferredController> logger;

    public ReceivedTransferredController(

          ISapJobQueuer jobQueuer,
        IReceivedTransferredRepository repository,
        ILogger<ReceivedTransferredController> logger)
    {
        this.jobQueuer = jobQueuer;
        this.repository = repository;
        this.logger = logger;
    }


    [HttpGet("dashboard/warehouse/status/posting-date/due-date/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Get}")]
    public async Task<IActionResult> GetByWarehouseIdForDashboard(
        int warehouseId,
        int? sourceWarehouseId,
        string? status,
        string? liveStatus,
        DateTime? postingDate,
        DateTime? dueDate,
        int skip,
        int pageSize)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");


        var res = await repository.GetByWarehouseIdAsDestinationWarehouseAndStatusAndDateWithPaginationForDashboardAsync(
            warehouseId,
            userId,
            sourceWarehouseId,
            postingDate,
            dueDate,
            status,
            skip,
            pageSize);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }


    [HttpPost("receive-quantities")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Edit}")]
    public async Task<IActionResult> UpdateReceivedQuantities([FromBody] ReceiveTransferredStockDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await repository.UpdateReceivedQuantitiesAsync(userId!, dto);

        if (res.Message == "Draft")
            return Ok(new { Message = "Draft" });

        if (!res.Success)
            return BadRequest(res);

        await jobQueuer.DistributionOrders(res.Data);

        return Ok(res);
    }

    [HttpPatch("transferredStockId{transferredStockId}/complete-receiving")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Edit}")]
    public async Task<IActionResult> CompleteReceivingStatusIfDraft(int transferredStockId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await repository.CompleteReceivingStatusIfDraftAsync(userId!, transferredStockId);
        if (!res.Success)
            return BadRequest(res);

        await jobQueuer.DistributionOrders(res.Data);


        return Ok(res);
    }

 
}
