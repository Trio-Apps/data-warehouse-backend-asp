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
public class QuantityAdjustmentStockItemController : ControllerBase
{
    private readonly IQuantityAdjustmentStockItemRepository _repository;
    private readonly ILogger<QuantityAdjustmentStockItemController> _logger;

    public QuantityAdjustmentStockItemController(
        IQuantityAdjustmentStockItemRepository repository,
        ILogger<QuantityAdjustmentStockItemController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<IEnumerable<QuantityAdjustmentStockItem>>> GetAll()
    {
        var result = await _repository.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<QuantityAdjustmentStockItem>> GetById(int id)
    {
        var result = await _repository.GetByIdAsync(id);
        if (result == null)
            return NotFound($"QuantityAdjustmentStockItem with ID {id} not found.");

        return Ok(result);
    }

    [HttpGet("quantity-adjustment-stock/{quantityAdjustmentStockId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<IEnumerable<QuantityAdjustmentStockItemDTO>>> GetByQuantityAdjustmentStockId(int quantityAdjustmentStockId)
    {
        var res = await _repository.GetByQuantityAdjustmentStockItemByQuantityAdjustmentStockIdAsync(quantityAdjustmentStockId);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("status/quantity-adjustment-stock/{quantityAdjustmentStockId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<IEnumerable<QuantityAdjustmentStockItemDTO>>> GetByQuantityAdjustmentStockIdWithPagination(
        int quantityAdjustmentStockId, string? status, int skip, int pageSize)
    {
        var res = await _repository.GetByQuantityAdjustmentStockItemByQuantityAdjustmentStockIdWithPaginationAsync(
            quantityAdjustmentStockId, status, skip, pageSize);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPost("quantity-adjustment-stock/{quantityAdjustmentStockId}/add-barcode-or-no/{isBarcode:bool}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Create}")]
    public async Task<ActionResult<QuantityAdjustmentStockItem>> Create(
        int quantityAdjustmentStockId,
        bool isBarcode,
        AddQuantityAdjustmentStockItemCreateRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (isBarcode && dto.Barcode == null)
            return BadRequest(" barcode should not be sent with static type");

        var created = await _repository.AddQuantityAdjustmentStockItemByQuantityAdjustmentStockIdAsync(
            quantityAdjustmentStockId, isBarcode, dto.Barcode, dto.Item);

        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("quantity-adjustment-stock-item/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Edit}")]
    public async Task<IActionResult> Update(int id, UpdateGeneralItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _repository.UpdateQuantityAdjustmentStockItemAsync(id, dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("quantity-adjustment-stock-item/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _repository.DeleteQuantityAdjustmentStockItemAsync(id);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }
}
