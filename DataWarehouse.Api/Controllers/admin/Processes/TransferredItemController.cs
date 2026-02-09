using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin.Processes;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TransferredItemController : ControllerBase
{
    private readonly ITransferredItemRepository _repository;
    private readonly ILogger<TransferredItemController> _logger;

    public TransferredItemController(
        ITransferredItemRepository repository,
        ILogger<TransferredItemController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<ActionResult<IEnumerable<TransferredItem>>> GetAll()
    {
        var transferredItems = await _repository.GetAllAsync();
        return Ok(transferredItems);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<ActionResult<TransferredItem>> GetById(int id)
    {
        var transferredItem = await _repository.GetByIdAsync(id);
        if (transferredItem == null)
            return NotFound($"TransferredItem with ID {id} not found.");

        return Ok(transferredItem);
    }

    [HttpGet("transferred-stock/{TransferredStockId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<ActionResult<IEnumerable<TransferredItemDTO>>> GetByTransferredStockId(int TransferredStockId)
    {
        var transferredItems = await _repository.GetByTransferredItemByTransferredStockIdAsync(TransferredStockId);
        return Ok(transferredItems);
    }

    [HttpGet("status/transferred-stock/{TransferredStockId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Get}")]
    public async Task<ActionResult<IEnumerable<TransferredItemDTO>>>
        GetByTransferredStockIdWithPagination(int TransferredStockId, string? status, int skip, int pageSize)
    {
        var res = await _repository.GetByTransferredItemByTransferredStockIdWithPaginationAsync(
            TransferredStockId, status, skip, pageSize);

        return Ok(res);
    }

    [HttpPost("Transferred-stock/{TransferredStockId}/add-barcode-or-no/{isBarcode:bool}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Create}")]
    public async Task<ActionResult<TransferredItem>> Create(
        int TransferredStockId,
        bool isBarcode,
        AddTransferredItemCreateRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Case 1: Barcode = true
        if (isBarcode)
        {
            if (dto.Barcode == null)
                return BadRequest(" barcode should not be sent with static type");
        }

        var created = await _repository.AddTransferredItemByTransferredStockIdAsync(
            TransferredStockId,
            isBarcode,
            dto.Barcode,
            dto.Item);

        return Ok(created);
    }

    [HttpPut("Transferred-item-order/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Edit}")]
    public async Task<IActionResult> Update(int id, UpdateTransferredItemDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _repository.UpdateTransferredItemAsync(id, dto);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("Transferred-item-order/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Transferred_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var transferredItem = await _repository.GetByIdAsync(id);
        if (transferredItem == null)
            return NotFound($"TransferredItem with ID {id} not found.");

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return Ok("delete");
    }
}

