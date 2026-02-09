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
public class ReceivedStockBatchController : ControllerBase
{
    private readonly IReceivedStockBatchRepository _repository;
    private readonly ILogger<ReceivedStockBatchController> _logger;

    public ReceivedStockBatchController(
        IReceivedStockBatchRepository repository,
        ILogger<ReceivedStockBatchController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<IEnumerable<ReceivedStockBatch>>> GetAll()
    {
        var batches = await _repository.GetAllAsync();
        return Ok(batches);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<ReceivedStockBatch>> GetById(int id)
    {
        var batch = await _repository.GetByIdAsync(id);
        if (batch == null)
            return NotFound($"ReceivedStockBatch with ID {id} not found.");

        return Ok(batch);
    }

    [HttpGet("received-item/{receivedItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<IEnumerable<ReceivedStockBatch>>> GetByReceivedItemId(int receivedItemId)
    {
        var res = await _repository.GetByReceivedItemIdAsync(receivedItemId);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("received-item/{receivedItemId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<IEnumerable<ReceivedStockBatch>>> GetByReceivedItemIdWithPagination(
        int receivedItemId, int skip, int pageSize)
    {
        var res = await _repository.GetByReceivedItemIdWithPaginationAsync(receivedItemId, skip, pageSize);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPost("received-item/{receivedItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Create}")]
    public async Task<ActionResult<ReceivedStockBatch>> Create(int receivedItemId, AddReceivedStockBatchDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _repository.AddByReceivedItemIdAsync(receivedItemId, dto);

        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReceivedStockBatchDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _repository.UpdateReceivedStockBatchAsync(id, dto);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var batch = await _repository.GetByIdAsync(id);
        if (batch == null)
            return NotFound($"ReceivedStockBatch with ID {id} not found.");

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return NoContent();
    }
}
