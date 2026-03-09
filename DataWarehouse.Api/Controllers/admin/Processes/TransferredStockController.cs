using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.Processes;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TransferredStockController : ControllerBase
{
    private readonly ITransferredStockRepository _repository;
    private readonly ISapJobQueuer jobQueuer;
    private readonly ILogger<TransferredStockController> _logger;

    public TransferredStockController(
        ITransferredStockRepository repository,
                ISapJobQueuer jobQueuer,
        ILogger<TransferredStockController> logger)
    {
        _repository = repository;
        this.jobQueuer = jobQueuer;
        _logger = logger;
    }



    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<ActionResult<IEnumerable<TransferredStock>>> GetAll()
    {
        var data = await _repository.GetAllAsync();
        return Ok(data);
    }


    [HttpGet("warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<ActionResult<IEnumerable<TransferredStock>>> GetByWarehouseId(int warehouseId)
    {
        var data = await _repository.GetByWarehouseIdAsync(warehouseId);
        return Ok(data);
    }


    [HttpGet("warehouse/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<IActionResult> GetByWarehouseIdWithPagination(int warehouseId, int skip, int pageSize)
    {
        var res = await _repository.GetByWarehouseIdWithPaginationAsync(warehouseId, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }



    [HttpGet("dashboard/warehouse/status/posting-date/due-date/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Get}")]
    public async Task<IActionResult> GetByWarehouseIdForDashboard(
        int warehouseId,
        int? destinationWarehouseId,
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


        var res = await _repository.GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(
            warehouseId,
            userId,
            destinationWarehouseId,
            postingDate,
            dueDate,
            status,
            skip,
            pageSize);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }


    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<IActionResult> GetById(int id)
    {

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");


        var data = await _repository.GetTransferredStockByIdAsync(userId, id);
        if (!data.Success)
            return NotFound(data);


        return Ok(data);
    }

   
    [HttpGet("{id}/with-items")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<ActionResult<TransferredStock>> GetWithItems(int id)
    {
        var data = await _repository.GetWithItemsAsync(id);
        if (data == null)
            return NotFound($"TransferredStock with ID {id} not found.");

        return Ok(data);
    }

    [HttpGet("{id}/with-warehouses")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<ActionResult<TransferredStock>> GetWithWarehouses(int id)
    {
        var data = await _repository.GetWithWarehousesAsync(id);
        if (data == null)
            return NotFound($"TransferredStock with ID {id} not found.");

        return Ok(data);
    }

    [HttpGet("status")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<IActionResult> Status()
    {
        var statuses = await _repository.GetTransferredStockStatus();

        if (!statuses.Success)
            return NotFound(statuses);

        return Ok(statuses);
    }

    [HttpGet("status/{status}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<IActionResult> Status(string status)
    {
        var data = await _repository.GetByStatusAsync(status);

        if (!data.Success)
            return NotFound(data);

        return Ok(data);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Create}")]
    public async Task<ActionResult<TransferredStock>> Create(AddTransferredStockWithoutRefDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");

        var created = await _repository.AddTransferredStockByWarehouseIdWithoutRefAsync(userId, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPost("transferred-request/{transferredRequestId}/with-default-items")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Create}")]
    public async Task<ActionResult<TransferredStock>> CreateWithDefaultItems(int transferredRequestId, AddTransferredStockDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");

        dto.TransferredRequestId = transferredRequestId;
        var created = await _repository.AddTransferredStockAndItemsByTransferredRequestIdAsync(userId, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTransferredStockDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");

        dto.TransferredStockId = id;
        var res = await _repository.UpdateTransferredStockAsync(userId, id, dto);
        if (!res.Success) return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _repository.DeleteTransferredStockAsync(id);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPatch("{id}/revert-partially-failed")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Edit}")]
    public async Task<IActionResult> RevertPartiallyFailedStatus(int id)
    {
        var res = await _repository.RevertPartiallyFailedStatusToProcessingAsync(id);

        await jobQueuer.DistributionOrders(res.Data);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }


    [HttpGet("pending")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<ActionResult<IEnumerable<TransferredStock>>> GetPendingTransfers()
    {
        var data = await _repository.GetPendingTransfersAsync();
        return Ok(data);
    }
}

