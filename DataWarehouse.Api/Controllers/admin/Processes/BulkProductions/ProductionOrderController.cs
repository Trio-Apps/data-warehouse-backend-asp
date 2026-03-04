using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DataWarehouse.Api.Controllers.admin.Processes.BulkProductions;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProductionOrderController : ControllerBase
{
    private readonly IProductionOrderRepository _repository;
    private readonly ILogger<ProductionOrderController> _logger;

    public ProductionOrderController(
        IProductionOrderRepository repository,
        ILogger<ProductionOrderController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<ActionResult<IEnumerable<ProductionOrder>>> GetAll()
    {
        var productionOrders = await _repository.GetAllAsync();
        return Ok(productionOrders);
    }

    [HttpGet("warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<ActionResult<IEnumerable<ProductionOrder>>> GetByWarehouseId(int warehouseId)
    {
        var productionOrders = await _repository.GetByWarehouseIdAsync(warehouseId);
        return Ok(productionOrders);
    }

    [HttpGet("warehouse/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<IActionResult> GetByWarehouseIdWithPagination(int warehouseId, int skip, int pageSize)
    {
        var res = await _repository.GetByWarehouseIdWithPaginationAsync(warehouseId, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<ActionResult<ProductionOrder>> GetById(int id)
    {
        var productionOrder = await _repository.GetByIdAsync(id);
        if (productionOrder == null)
            return NotFound($"ProductionOrder with ID {id} not found.");

        return Ok(productionOrder);
    }

    [HttpGet("status")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<IActionResult> Status()
    {
        var productionOrder = await _repository.GetProductionOrderStatus();
        return Ok(productionOrder);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Create}")]
    public async Task<ActionResult<ProductionOrder>> Create([FromBody] AddProductionOrderDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddProductionOrderByWarehouseIdAsync(userId, dto);
        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductionOrderDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await _repository.UpdateProductionOrderAsync(userId, id, dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPost("{id}/submit")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Edit}")]
    public async Task<IActionResult> Submit(int id, [FromBody] SubmitProductionOrderRequest? request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var res = await _repository.SubmitProductionOrderAsync(userId, id, request?.Note);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var productionOrder = await _repository.GetByIdAsync(id);
        if (productionOrder == null)
            return NotFound($"ProductionOrder with ID {id} not found.");

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("processing")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<ActionResult<IEnumerable<ProductionOrder>>> GetPendingOrders()
    {
        var productionOrders = await _repository.GetPendingOrdersAsync();
        return Ok(productionOrders);
    }

    // commented endpoints left as-is
}

public sealed class SubmitProductionOrderRequest
{
    public string? Note { get; set; }
}
