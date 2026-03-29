using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Core.Interfaces.Queue;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.Processes;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TransferredRequestOrderController : ControllerBase
{
    private readonly ISapJobQueuer jobQueuer;
    private readonly ITransferredRequestOrderRepository _repository;
    private readonly ILogger<TransferredRequestOrderController> _logger;


    public TransferredRequestOrderController(
        ISapJobQueuer jobQueuer,
        ITransferredRequestOrderRepository repository,
        ILogger<TransferredRequestOrderController> logger)
    {
        this.jobQueuer = jobQueuer;
        _repository = repository;
        _logger = logger;
    }


    [HttpGet("search-pagination/warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Items_Get}")]
    public async Task<IActionResult> GetByWarehouseId(
    int warehouseId,
    [FromQuery] string? itemCodeOrItemName,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20)
    {
        var result = await _repository.GetByWarehouseIdAsync(
            warehouseId,
            itemCodeOrItemName,
            pageNumber,
            pageSize);

        return Ok(result);
    }


    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Get}")]
    public async Task<ActionResult<IEnumerable<TransferredRequest>>> GetAll()
    {
        var orders = await _repository.GetAllAsync();
        return Ok(orders);
    }


    [HttpGet("warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Get}")]
    public async Task<ActionResult<IEnumerable<TransferredRequest>>> GetByWarehouseId(int warehouseId)
    {
        var orders = await _repository.GetByWarehouseIdAsync(warehouseId);
        return Ok(orders);
    }


    [HttpGet("warehouse/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Get}")]
    public async Task<IActionResult> GetByWarehouseIdWithPagination(int warehouseId, int skip, int pageSize)
    {
        var res = await _repository.GetByWarehouseIdWithPaginationAsync(warehouseId, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("items-in-warehouse/warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Items_Get}")]
    public async Task<IActionResult> GetItemsByWarehouseId(int warehouseId)
    {
        var items = await _repository.GetItemsByWarehouseIdAsync(warehouseId);
        if (!items.Success)
            return BadRequest(items);

        return Ok(items);
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
            liveStatus,
            status,
            skip,
            pageSize);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Get}")]
    public async Task<ActionResult<TransferredRequest>> GetById(int id)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null)
            return NotFound($"TransferredRequest with ID {id} not found.");

        return Ok(order);
    }

    [HttpGet("with-warehouses/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Get}")]
    public async Task<IActionResult> GetWithWarehousesAndApproval(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");

        var res = await _repository.GetWithWarehousesAndApprovalAsync(id, userId);
        if (!res.Success)
            return NotFound(res);

        return Ok(res);
    }

    [HttpGet("status")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Get}")]
    public async Task<IActionResult> Status()
    {
        var statuses = await _repository.GetTransferredRequestStatus();
        if (!statuses.Success)
            return NotFound(statuses);

        return Ok(statuses);
    }

    [HttpGet("status/{status}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Get}")]
    public async Task<IActionResult> Status(string status)
    {
        var orders = await _repository.GetByStatusAsync(status);
        if (!orders.Success)
            return NotFound(orders);

        return Ok(orders);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Create}")]
    public async Task<ActionResult<TransferredRequest>> Create(AddTransferredRequestDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");

        var created = await _repository.AddTransferredRequestByWarehouseIdAsync(userId, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPost("{id}/duplicate")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Create}")]
    public async Task<IActionResult> Duplicate(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");
        var res = await _repository.DuplicateTransferredRequestAsync(userId, id);
        if (!res.Success)
            return BadRequest(res);
        return Ok(res);
    }    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTransferredRequestDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");

        dto.TransferredRequestId = id;
        var res = await _repository.UpdateTransferredRequestAsync(userId, id, dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _repository.DeleteTransferredRequestAsync(id);
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
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Get}")]
    public async Task<ActionResult<IEnumerable<TransferredRequest>>> GetPendingRequests()
    {
        var requests = await _repository.GetPendingRequestsAsync();
        return Ok(requests);
    }
}
