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
public class ReceivedItemController : ControllerBase
{
    private readonly IReceivedItemRepository _repository;
    private readonly ILogger<ReceivedItemController> _logger;

    public ReceivedItemController(
        IReceivedItemRepository repository,
        ILogger<ReceivedItemController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<IEnumerable<ReceivedItem>>> GetAll()
    {
        var receivedItems = await _repository.GetAllAsync();
        return Ok(receivedItems);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<ReceivedItem>> GetById(int id)
    {
        var receivedItem = await _repository.GetByIdAsync(id);
        if (receivedItem == null)
            return NotFound($"ReceivedItem with ID {id} not found.");

        return Ok(receivedItem);
    }

    [HttpGet("received-stock/{ReceivedStockId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<IEnumerable<ReceivedItemDTO>>> GetByReceivedStockId(int ReceivedStockId)
    {
        var receivedItems = await _repository.GetByReceivedItemByReceivedStockIdAsync(ReceivedStockId);
        return Ok(receivedItems);
    }

    [HttpGet("received-stock/{ReceivedStockId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<IEnumerable<ReceivedItemDTO>>> GetByReceivedStockIdWithPagination(
        int ReceivedStockId, int skip, int pageSize)
    {
        var res = await _repository.GetByReceivedItemByReceivedStockIdWithPaginationAsync(
            ReceivedStockId, skip, pageSize);

        return Ok(res);
    }

    [HttpPost("transferred-stock/{TransferredStockId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Create}")]
    public async Task<ActionResult<ReceivedItem>> Create(int TransferredStockId, AddReceivedItemDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddReceivedItemByTransferredItemIdAsync(userId, TransferredStockId, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Edit}")]
    public async Task<IActionResult> Update(int id, UpdateReceivedItemDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _repository.UpdateReceivedItemAsync(id, dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var receivedItem = await _repository.GetByIdAsync(id);
        if (receivedItem == null)
            return NotFound($"ReceivedItem with ID {id} not found.");

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return Ok("deleted");
    }

    [HttpGet("{id}/with-batches")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<ReceivedItem>> GetWithBatches(int id)
    {
        var receivedItem = await _repository.GetWithBatchesAsync(id);
        if (receivedItem == null)
            return NotFound($"ReceivedItem with ID {id} not found.");

        return Ok(receivedItem);
    }

    [HttpGet("{id}/with-transferred-item")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<ReceivedItem>> GetWithTransferredItem(int id)
    {
        var receivedItem = await _repository.GetWithTransferredItemAsync(id);
        if (receivedItem == null)
            return NotFound($"ReceivedItem with ID {id} not found.");

        return Ok(receivedItem);
    }
}
