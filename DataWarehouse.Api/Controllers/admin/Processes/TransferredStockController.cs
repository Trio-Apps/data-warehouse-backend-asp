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
    private readonly ILogger<TransferredStockController> _logger;

    public TransferredStockController(
        ITransferredStockRepository repository,
        ILogger<TransferredStockController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<ActionResult<IEnumerable<TransferredStock>>> GetAll()
    {
        var transferredStocks = await _repository.GetAllAsync();
        return Ok(transferredStocks);
    }

    [HttpGet("warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<ActionResult<IEnumerable<TransferredStock>>> GetByWarehouseId(int warehouseId)
    {
        var transferredStocks = await _repository.GetByWarehouseIdAsync(warehouseId);
        return Ok(transferredStocks);
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

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<ActionResult<TransferredStock>> GetById(int id)
    {
        var transferredStock = await _repository.GetByIdAsync(id);
        if (transferredStock == null)
            return NotFound($"TransferredStock with ID {id} not found.");

        return Ok(transferredStock);
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
        var transferredStocks = await _repository.GetByStatusAsync(status);

        if (!transferredStocks.Success)
            return NotFound(transferredStocks);

        return Ok(transferredStocks);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Create}")]
    public async Task<ActionResult<TransferredStock>> Create(AddTransferredStockDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddTransferredStockByWarehouseIdAsync(userId, dto);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTransferredStockDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await _repository.UpdateTransferredStockAsync(userId, id, dto);
        if (!res.Success) return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var transferredStock = await _repository.GetByIdAsync(id);
        if (transferredStock == null)
            return NotFound($"TransferredStock with ID {id} not found.");

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("pending")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<ActionResult<IEnumerable<TransferredStock>>> GetPendingTransfers()
    {
        var transferredStocks = await _repository.GetPendingTransfersAsync();
        return Ok(transferredStocks);
    }
}

