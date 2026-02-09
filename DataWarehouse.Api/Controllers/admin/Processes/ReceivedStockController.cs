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
public class ReceivedStockController : ControllerBase
{
    private readonly IReceivedStockRepository _repository;
    private readonly ILogger<ReceivedStockController> _logger;

    public ReceivedStockController(
        IReceivedStockRepository repository,
        ILogger<ReceivedStockController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<IEnumerable<ReceivedStock>>> GetAll()
    {
        var receivedStocks = await _repository.GetAllAsync();
        return Ok(receivedStocks);
    }

    [HttpGet("warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<IEnumerable<ReceivedStock>>> GetByWarehouseId(int warehouseId)
    {
        var receivedStocks = await _repository.GetByWarehouseIdAsync(warehouseId);
        return Ok(receivedStocks);
    }

    [HttpGet("warehouse/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<IActionResult> GetByWarehouseIdWithPagination(int warehouseId, int skip, int pageSize)
    {
        var res = await _repository.GetByWarehouseIdWithPaginationAsync(warehouseId, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<ReceivedStock>> GetById(int id)
    {
        var receivedStock = await _repository.GetByIdAsync(id);
        if (receivedStock == null)
            return NotFound($"ReceivedStock with ID {id} not found.");

        return Ok(receivedStock);
    }

    [HttpGet("transferred-stock/{transferredStockId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<IActionResult> GetByTransferredStockId(int transferredStockId)
    {
        var receivedStock = await _repository.GetByTransferredStockIdAsync(transferredStockId);
        if (!receivedStock.Success)
            return NotFound($"ReceivedStock for TransferredStock ID {transferredStockId} not found.");

        return Ok(receivedStock);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<IEnumerable<ReceivedStock>>> GetByUserId(string userId)
    {
        var receivedStocks = await _repository.GetByUserIdAsync(userId);
        return Ok(receivedStocks);
    }

    [HttpGet("{id}/with-items")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<ReceivedStock>> GetWithItems(int id)
    {
        var receivedStock = await _repository.GetWithItemsAsync(id);
        if (receivedStock == null)
            return NotFound($"ReceivedStock with ID {id} not found.");

        return Ok(receivedStock);
    }

    [HttpGet("{id}/with-items-batches")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<IActionResult> GetWithItemsAndBatches(int id)
    {
        var res = await _repository.GetWithItemsAndBatchesAsync(id);
        if (!res.Success)
            return NotFound(res);

        return Ok(res);
    }

    [HttpGet("{id}/with-transferred-stock")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<ReceivedStock>> GetWithTransferredStock(int id)
    {
        var receivedStock = await _repository.GetWithTransferredStockAsync(id);
        if (receivedStock == null)
            return NotFound($"ReceivedStock with ID {id} not found.");

        return Ok(receivedStock);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Create}")]
    public async Task<ActionResult<ReceivedStock>> Create(AddReceivedStockDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddReceivedStockByTransferredStockIdAsync(userId, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReceivedStockDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await _repository.UpdateReceivedStockAsync(userId, id, dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var receivedStock = await _repository.GetByIdAsync(id);
        if (receivedStock == null)
            return NotFound($"ReceivedStock with ID {id} not found.");

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return NoContent();
    }
}
