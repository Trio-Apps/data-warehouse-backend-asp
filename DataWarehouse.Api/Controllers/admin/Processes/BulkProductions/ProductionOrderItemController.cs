using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Api.Controllers.admin.Processes.BulkProductions;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProductionOrderItemController : ControllerBase
{
    private readonly IProductionOrderItemRepository _repository;
    private readonly ILogger<ProductionOrderItemController> _logger;

    public ProductionOrderItemController(
        IProductionOrderItemRepository repository,
        ILogger<ProductionOrderItemController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<ActionResult<IEnumerable<ProductionOrderItem>>> GetAll()
    {
        var productionOrderItems = await _repository.GetAllAsync();
        return Ok(productionOrderItems);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<ActionResult<ProductionOrderItem>> GetById(int id)
    {
        var productionOrderItem = await _repository.GetByIdAsync(id);
        if (productionOrderItem == null)
            return NotFound($"ProductionOrderItem with ID {id} not found.");

        return Ok(productionOrderItem);
    }

    [HttpGet("production-order/{productionOrderId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<ActionResult<IEnumerable<ProductionOrderItem>>> GetByProductionOrderId(int productionOrderId)
    {
        var productionOrderItems =
            await _repository.GetByProductionItemByProductionOrderIdAsync(productionOrderId);

        return Ok(productionOrderItems);
    }

    [HttpGet("status/production-order/{productionOrderId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<ActionResult<IEnumerable<ProductionOrderItem>>>
        GetByProductionOrderIdWithPagination(
            int productionOrderId,
            string? status,
            int skip,
            int pageSize)
    {
        var res =
            await _repository.GetByProductionItemByProductionOrderIdWithPaginationAsync(
                productionOrderId, status, skip, pageSize);

        return Ok(res);
    }

    [HttpPost("production-order/{productionOrderId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Create}")]
    public async Task<ActionResult<ProductionOrderItem>> Create(
        int productionOrderId,
        AddProductionOrderItemDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created =
            await _repository.AddProductionItemByProductionOrderIdAsync(productionOrderId, dto);

        return Ok(created);
    }

    [HttpPut("production-item-order/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Edit}")]
    public async Task<IActionResult> Update(
        int id,
        bool? isRecevied,
        UpdateProductionOrderItemDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _repository.UpdateProductionItemAsync(id, isRecevied, dto);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("production-item-order/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var productionOrderItem = await _repository.GetByIdAsync(id);
        if (productionOrderItem == null)
            return NotFound($"ProductionOrderItem with ID {id} not found.");

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return Ok("delete");
    }
}
