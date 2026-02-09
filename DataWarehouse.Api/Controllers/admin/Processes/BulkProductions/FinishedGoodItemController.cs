using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Api.Controllers.admin.Processes.BulkProductions;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FinishedGoodItemController : ControllerBase
{
    private readonly IFinishedGoodItemRepository _repository;
    private readonly ILogger<FinishedGoodItemController> _logger;

    public FinishedGoodItemController(
        IFinishedGoodItemRepository repository,
        ILogger<FinishedGoodItemController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<IActionResult> GetAll()
    {
        var finishedGoodItems = await _repository.GetAllAsync();
        return Ok(finishedGoodItems);
    }

    [HttpGet("GetFinishedGoodBomItemsByWarehouseId/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<IActionResult> GetFinishedGoodBomItemsByWarehouseId(
        int warehouseId, int skip, int pageSize, int? itemId)
    {
        var res = await _repository.GetFinishedGoodBomItemsByWarehouseIdAsync(
            warehouseId, itemId, skip, pageSize);

        return Ok(res);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<IActionResult> GetById(int id)
    {
        var finishedGoodItem = await _repository.GetByIdAsync(id);
        if (finishedGoodItem == null)
            return NotFound($"FinishedGoodItem with ID {id} not found.");

        return Ok(finishedGoodItem);
    }

    [HttpGet("warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<IActionResult> GetByWarehouseId(int warehouseId)
    {
        var finishedGoodItems = await _repository.GetByWarehouseIdAsync(warehouseId);
        return Ok(finishedGoodItems);
    }

    [HttpGet("item/{itemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<IActionResult> GetByItemId(int itemId)
    {
        var finishedGoodItems = await _repository.GetByItemIdAsync(itemId);
        return Ok(finishedGoodItems);
    }

    [HttpGet("item/{itemId}/warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<IActionResult> GetByItemAndWarehouse(int itemId, int warehouseId)
    {
        var finishedGoodItem = await _repository.GetByItemAndWarehouseAsync(itemId, warehouseId);
        if (finishedGoodItem == null)
            return NotFound($"FinishedGoodItem with ItemId {itemId} and WarehouseId {warehouseId} not found.");

        return Ok(finishedGoodItem);
    }

    [HttpGet("{id}/with-item")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<IActionResult> GetWithItem(int id)
    {
        var finishedGoodItem = await _repository.GetWithItemAsync(id);
        if (finishedGoodItem == null)
            return NotFound($"FinishedGoodItem with ID {id} not found.");

        return Ok(finishedGoodItem);
    }

    [HttpGet("{id}/with-warehouse")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<IActionResult> GetWithWarehouse(int id)
    {
        var finishedGoodItem = await _repository.GetWithWarehouseAsync(id);
        if (finishedGoodItem == null)
            return NotFound($"FinishedGoodItem with ID {id} not found.");

        return Ok(finishedGoodItem);
    }

    [HttpGet("active")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<IActionResult> GetActiveItems()
    {
        var finishedGoodItems = await _repository.GetActiveItemsAsync();
        return Ok(finishedGoodItems);
    }
}
