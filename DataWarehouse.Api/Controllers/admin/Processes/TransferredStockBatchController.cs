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
public class TransferredStockBatchController : ControllerBase
{
    private readonly ITransferredStockBatchRepository _repository;
    private readonly ILogger<TransferredStockBatchController> _logger;

    public TransferredStockBatchController(
        ITransferredStockBatchRepository repository,
        ILogger<TransferredStockBatchController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<ActionResult<IEnumerable<TransferredStockBatch>>> GetAll()
    {
        var batches = await _repository.GetAllAsync();
        return Ok(batches);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<ActionResult<TransferredStockBatch>> GetById(int id)
    {
        var batch = await _repository.GetByIdAsync(id);
        if (batch == null)
            return NotFound($"TransferredStockBatch with ID {id} not found.");

        return Ok(batch);
    }

    [HttpGet("transferred-item/{transferredItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<ActionResult<IEnumerable<TransferredStockBatch>>> GetByTransferredItemId(int transferredItemId)
    {
        var res = await _repository.GetByTransferredItemIdAsync(transferredItemId);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("transferred-item/{transferredItemId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<ActionResult<IEnumerable<TransferredStockBatch>>> GetByTransferredItemIdWithPagination(
        int transferredItemId, int skip, int pageSize)
    {
        var res = await _repository.GetByTransferredItemIdWithPaginationAsync(transferredItemId, skip, pageSize);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPost("transferred-item/{transferredItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Create}")]
    public async Task<ActionResult<TransferredStockBatch>> Create(int transferredItemId, AddTransferredStockBatchDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _repository.AddByTransferredItemIdAsync(transferredItemId, dto);

        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTransferredStockBatchDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _repository.UpdateTransferredStockBatchAsync(id, dto);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var batch = await _repository.GetByIdAsync(id);
        if (batch == null)
            return NotFound($"TransferredStockBatch with ID {id} not found.");

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return NoContent();
    }
}

