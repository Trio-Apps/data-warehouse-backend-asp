using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.Processes.BulkProductions;

[Route("api/production-order-items")]
[ApiController]
[Authorize]
public class ProductionOrderItemController : ControllerBase
{
    private readonly IProductionOrderItemRepository _repository;

    public ProductionOrderItemController(IProductionOrderItemRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Create}")]
    public async Task<IActionResult> Create([FromBody] AddProductionOrderItemDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _repository.CreateAsync(userId!, dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _repository.GetByIdDetailsAsync(userId!, id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<IActionResult> GetList(
        [FromQuery] int productionOrderId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        if (productionOrderId <= 0)
            return BadRequest("productionOrderId is required.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _repository.GetListAsync(userId!, productionOrderId, pageNumber, pageSize);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductionOrderItemDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _repository.UpdateProductionItemAsync(userId!, id, dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _repository.DeleteProductionItemAsync(userId!, id);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
