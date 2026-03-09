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
public class CountStockController : ControllerBase
{
    private readonly ICountStockRepository _repository;
    private readonly ILogger<CountStockController> _logger;

    public CountStockController(
        ICountStockRepository repository,
        ILogger<CountStockController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<IEnumerable<CountStock>>> GetAll()
    {
        var countStocks = await _repository.GetAllAsync();
        return Ok(countStocks);
    }

    [HttpGet("warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<IEnumerable<CountStock>>> GetByWarehouseId(int warehouseId)
    {
        var countStocks = await _repository.GetByWarehouseIdAsync(warehouseId);
        return Ok(countStocks);
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

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<CountStock>> GetById(int id)
    {
        var countStock = await _repository.GetByIdAsync(id);
        if (countStock == null)
            return NotFound($"CountStock with ID {id} not found.");

        return Ok(countStock);
    }

    [HttpGet("status")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<IActionResult> Status()
    {
        var statuses = await _repository.GetCountStockStatus();

        if (!statuses.Success)
            return NotFound(statuses);

        return Ok(statuses);
    }

    [HttpGet("status/{status}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<IActionResult> Status(string status)
    {
        var countStocks = await _repository.GetByStatusAsync(status);

        if (!countStocks.Success)
            return NotFound(countStocks);

        return Ok(countStocks);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Create}")]
    public async Task<ActionResult<CountStock>> Create(AddCountStockDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");

        var created = await _repository.AddCountStockByWarehouseIdAsync(userId, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCountStockDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");

        dto.CountStockId = id;
        var res = await _repository.UpdateCountStockAsync(userId, id, dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var countStock = await _repository.GetByIdAsync(id);
        if (countStock == null)
            return NotFound($"CountStock with ID {id} not found.");

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("pending")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<IEnumerable<CountStock>>> GetPendingCounts()
    {
        var countStocks = await _repository.GetPendingCountsAsync();
        return Ok(countStocks);
    }
}

