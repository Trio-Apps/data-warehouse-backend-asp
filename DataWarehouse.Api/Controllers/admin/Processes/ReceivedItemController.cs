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
    private readonly IReceivedItemRepository repository;
    private readonly ILogger<ReceivedItemController> logger;

    public ReceivedItemController(
        IReceivedItemRepository repository,
        ILogger<ReceivedItemController> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<IEnumerable<ReceivedItem>>> GetAll()
    {
        var receivedItems = await repository.GetAllAsync();
        return Ok(receivedItems);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<ReceivedItem>> GetById(int id)
    {
        var receivedItem = await repository.GetByIdAsync(id);
        if (receivedItem == null)
            return NotFound($"ReceivedItem with ID {id} not found.");

        return Ok(receivedItem);
    }

    [HttpGet("received-stock/{receivedStockId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<IActionResult> GetByReceivedStockId(int receivedStockId)
    {
        var res = await repository.GetByReceivedStockIdAsync(receivedStockId);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("received-stock/{receivedStockId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<IActionResult> GetByReceivedStockIdWithPagination(
        int receivedStockId,
        int skip,
        int pageSize)
    {
        var res = await repository.GetByReceivedStockIdWithPaginationAsync(receivedStockId, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPost("transferred-stock/{transferredStockId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Create}")]
    public async Task<IActionResult> Create(int transferredStockId, AddReceivedItemDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await repository.AddReceivedItemByTransferredItemIdAsync(userId!, transferredStockId, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPost("witout-reference/received-stock/{receivedStockId}/add-barcode-or-no/{isBarcode:bool}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Create}")]
    public async Task<IActionResult> CreateWithoutReference(
        int receivedStockId,
        bool isBarcode,
        AddGeneralItemByManualOrBarcodeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await repository.AddReceivedItemByReceivedStockIdWithoutRefAsync(
            receivedStockId,
            isBarcode,
            dto.Barcode,
            dto.Item);

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

        var res = await repository.UpdateReceivedItemAsync(id, dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPut("without-reference/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Edit}")]
    public async Task<IActionResult> UpdateWithoutReference(int id, UpdateGeneralItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await repository.UpdateReceivedItemWithoutRefAsync(id, dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await repository.DeleteReceivedItemAsync(id);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("{id}/with-batches")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<ReceivedItem>> GetWithBatches(int id)
    {
        var receivedItem = await repository.GetWithBatchesAsync(id);
        if (receivedItem == null)
            return NotFound($"ReceivedItem with ID {id} not found.");

        return Ok(receivedItem);
    }

    [HttpGet("{id}/with-transferred-item")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<ReceivedItem>> GetWithTransferredItem(int id)
    {
        var receivedItem = await repository.GetWithTransferredItemAsync(id);
        if (receivedItem == null)
            return NotFound($"ReceivedItem with ID {id} not found.");

        return Ok(receivedItem);
    }

    [HttpGet("{id}/with-item")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<ReceivedItem>> GetWithItem(int id)
    {
        var receivedItem = await repository.GetWithItemAsync(id);
        if (receivedItem == null)
            return NotFound($"ReceivedItem with ID {id} not found.");

        return Ok(receivedItem);
    }
}
