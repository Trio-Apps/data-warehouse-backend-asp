using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
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
public class ProductionReceiptController : ControllerBase
{
    private readonly IProductionReceiptRepository _repository;
    private readonly ILogger<ProductionReceiptController> _logger;

    public ProductionReceiptController(
        IProductionReceiptRepository repository,
        ILogger<ProductionReceiptController> logger)
    {
        _repository = repository;
        _logger = logger;
    }


    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<ActionResult<IEnumerable<ProductionReceipt>>> GetAll()
    {
        var productionReceipts = await _repository.GetAllAsync();
        return Ok(productionReceipts);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<ActionResult<ProductionReceipt>> GetById(int id)
    {
        var productionReceipt = await _repository.GetByIdAsync(id);
        if (productionReceipt == null)
            return NotFound($"ProductionReceipt with ID {id} not found.");

        return Ok(productionReceipt);
    }

    [HttpGet("production-order-item/{productionOrderItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<ActionResult<IEnumerable<ProductionReceipt>>> GetByProductionOrderItemId(int productionOrderItemId)
    {
        var res = await _repository.GetByProductionOrderItemIdAsync(productionOrderItemId);
        return Ok(res);
    }

    [HttpPost("production-order-item/{productionOrderItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Create}")]
    public async Task<ActionResult<ProductionReceipt>> Create(int productionOrderItemId, AddProductionReceiptDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _repository.AddByProductionItemIdAsync(productionOrderItemId, dto);

        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductionReceiptDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _repository.UpdateProductionReceiptAsync(id, dto);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var productionReceipt = await _repository.GetByIdAsync(id);
        if (productionReceipt == null)
            return NotFound($"ProductionReceipt with ID {id} not found.");

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return NoContent();
    }
}

