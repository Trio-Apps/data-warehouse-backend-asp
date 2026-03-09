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
public class CountStockBatchController : ControllerBase
{
    private readonly ICountStockBatchRepository _repository;
    private readonly ILogger<CountStockBatchController> _logger;

    public CountStockBatchController(
        ICountStockBatchRepository repository,
        ILogger<CountStockBatchController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<IEnumerable<CountStockBatch>>> GetAll()
    {
        var batches = await _repository.GetAllAsync();
        return Ok(batches);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<CountStockBatch>> GetById(int id)
    {
        var batch = await _repository.GetByIdAsync(id);
        if (batch == null)
            return NotFound($"CountStockBatch with ID {id} not found.");

        return Ok(batch);
    }

    [HttpGet("count-stock-item/{countStockItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<IEnumerable<CountStockBatch>>> GetByCountStockItemId(int countStockItemId)
    {
        var res = await _repository.GetByCountStockItemIdAsync(countStockItemId);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("count-stock-item/{countStockItemId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<IEnumerable<CountStockBatch>>> GetByCountStockItemIdWithPagination(
        int countStockItemId, int skip, int pageSize)
    {
        var res = await _repository.GetByCountStockItemIdWithPaginationAsync(
            countStockItemId, skip, pageSize);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPost("count-stock-item/{countStockItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Create}")]
    public async Task<ActionResult<CountStockBatch>> Create(int countStockItemId, AddCountStockBatchDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _repository.AddByCountStockItemIdAsync(countStockItemId, dto);

        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCountStockBatchDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        dto.CountStockBatchId = id;
        var res = await _repository.UpdateCountStockBatchAsync(id, dto);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var batch = await _repository.GetByIdAsync(id);
        if (batch == null)
            return NotFound($"CountStockBatch with ID {id} not found.");

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return NoContent();
    }
}

