using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Core.Interfaces.Queue;
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
    private readonly ISapJobQueuer jobQueuer;
    private readonly IProductionOrderRepository _repository;
    private readonly ILogger<ProductionOrderController> _logger;

    public ProductionOrderController(
        ISapJobQueuer jobQueuer,
        IProductionOrderRepository repository,
        ILogger<ProductionOrderController> logger)
    {
        this.jobQueuer = jobQueuer;
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

    [HttpPost("bulk")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Create}")]
    public async Task<ActionResult<IEnumerable<ProductionOrder>>> CreateBulk([FromBody] List<AddProductionOrderDTO> dtos)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (dtos == null || !dtos.Any())
            return BadRequest("Request must contain at least one production order.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddProductionOrdersByWarehouseIdAsync(userId, dtos);
        return Ok(created);
    }

    [HttpGet("search")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<IActionResult> Search(
        [FromQuery] string? query,
        [FromQuery] int warehouseId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await _repository.SearchProductionOrdersAsync(userId, query, warehouseId, pageNumber, pageSize);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
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
