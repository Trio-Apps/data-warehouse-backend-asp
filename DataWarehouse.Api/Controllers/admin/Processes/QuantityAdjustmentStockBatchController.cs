using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Api.Controllers.admin.Processes;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class QuantityAdjustmentStockBatchController : ControllerBase
{
    private readonly IQuantityAdjustmentStockBatchRepository _repository;
    private readonly ILogger<QuantityAdjustmentStockBatchController> _logger;

    public QuantityAdjustmentStockBatchController(
        IQuantityAdjustmentStockBatchRepository repository,
        ILogger<QuantityAdjustmentStockBatchController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<IEnumerable<QuantityAdjustmentStockBatch>>> GetAll()
    {
        var result = await _repository.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<QuantityAdjustmentStockBatch>> GetById(int id)
    {
        var result = await _repository.GetByIdAsync(id);
        if (result == null)
            return NotFound($"QuantityAdjustmentStockBatch with ID {id} not found.");

        return Ok(result);
    }

    [HttpGet("quantity-adjustment-stock-item/{quantityAdjustmentStockItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<IEnumerable<QuantityAdjustmentStockBatchDTO>>> GetByQuantityAdjustmentStockItemId(int quantityAdjustmentStockItemId)
    {
        var res = await _repository.GetByQuantityAdjustmentStockItemIdAsync(quantityAdjustmentStockItemId);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("quantity-adjustment-stock-item/{quantityAdjustmentStockItemId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<IEnumerable<QuantityAdjustmentStockBatchDTO>>> GetByQuantityAdjustmentStockItemIdWithPagination(
        int quantityAdjustmentStockItemId, int skip, int pageSize)
    {
        var res = await _repository.GetByQuantityAdjustmentStockItemIdWithPaginationAsync(
            quantityAdjustmentStockItemId, skip, pageSize);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPost("quantity-adjustment-stock-item/{quantityAdjustmentStockItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Create}")]
    public async Task<ActionResult<QuantityAdjustmentStockBatch>> Create(int quantityAdjustmentStockItemId, GeneralBatchDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _repository.AddByQuantityAdjustmentStockItemIdAsync(quantityAdjustmentStockItemId, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGeneralBatchDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _repository.UpdateQuantityAdjustmentStockBatchAsync(id, dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _repository.DeleteQuantityAdjustmentStockBatchAsync(id);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }
}
