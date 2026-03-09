using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DataWarehouse.Api.Controllers.admin.Processes;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class QuantityAdjustmentStockController : ControllerBase
{
    private readonly ISapJobQueuer jobQueuer;
    private readonly IQuantityAdjustmentStockRepository _repository;
    private readonly ILogger<QuantityAdjustmentStockController> _logger;

    public QuantityAdjustmentStockController(
        ISapJobQueuer jobQueuer,
        IQuantityAdjustmentStockRepository repository,
        ILogger<QuantityAdjustmentStockController> logger)
    {
        this.jobQueuer = jobQueuer;
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<IEnumerable<QuantityAdjustmentStock>>> GetAll()
    {
        var result = await _repository.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<IEnumerable<QuantityAdjustmentStock>>> GetByWarehouseId(int warehouseId)
    {
        var result = await _repository.GetByWarehouseIdAsync(warehouseId);
        return Ok(result);
    }

    [HttpGet("warehouse/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<IActionResult> GetByWarehouseIdWithPagination(int warehouseId, int skip, int pageSize)
    {
        var res = await _repository.GetByWarehouseIdWithPaginationAsync(warehouseId, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("dashboard/warehouse/status/posting-date/due-date/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<IActionResult> GetByWarehouseIdForDashboard(
        int warehouseId,
        string? status,
        DateTime? postingDate,
        DateTime? dueDate,
        int skip,
        int pageSize)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");

        var res = await _repository.GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(
            warehouseId, userId, postingDate, dueDate, status, skip, pageSize);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("with-warehouse/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<IActionResult> GetByIdWithWarehouse(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");


        var res = await _repository.GetWithWarehouseAsync(id, userId);
        if (!res.Success)
            return NotFound(res);

        return Ok(res);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<QuantityAdjustmentStock>> GetById(int id)
    {
        var result = await _repository.GetByIdAsync(id);
        if (result == null)
            return NotFound($"QuantityAdjustmentStock with ID {id} not found.");

        return Ok(result);
    }

    [HttpGet("status")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<IActionResult> Status()
    {
        var result = await _repository.GetQuantityAdjustmentStockStatus();
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("status/{status}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<IActionResult> Status(string status)
    {
        var result = await _repository.GetByStatusAsync(status);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Create}")]
    public async Task<ActionResult<QuantityAdjustmentStock>> Create(AddQuantityAdjustmentStockDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");

        var created = await _repository.AddQuantityAdjustmentStockByWarehouseIdAsync(userId, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateQuantityAdjustmentStockDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");

        dto.QuantityAdjustmentStockId = id;
        var res = await _repository.UpdateQuantityAdjustmentStockAsync(userId, id, dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPatch("{id}/revert-partially-failed")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Edit}")]
    public async Task<IActionResult> RevertPartiallyFailedStatus(int id)
    {
        var res = await _repository.RevertPartiallyFailedStatusToProcessingAsync(id);

        await jobQueuer.DistributionOrders(res.Data);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _repository.DeleteQuantityAdjustmentStockAsync(id);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }
}
