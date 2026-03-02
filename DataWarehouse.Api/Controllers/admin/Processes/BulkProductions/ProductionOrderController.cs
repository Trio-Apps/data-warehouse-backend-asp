using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.Processes.BulkProductions;

[Route("api/production-orders")]
[ApiController]
[Authorize]
public class ProductionOrderController : ControllerBase
{
    private readonly IProductionOrderRepository _repository;

    public ProductionOrderController(IProductionOrderRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Create}")]
    public async Task<IActionResult> Create([FromBody] AddProductionOrderDTO dto)
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
        var result = await _repository.GetDetailsAsync(userId!, id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<IActionResult> GetList([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _repository.GetListAsync(userId!, pageNumber, pageSize);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductionOrderDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _repository.UpdateAsync(userId!, id, dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _repository.DeleteProductionOrderAsync(userId!, id);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:int}/submit")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Edit}")]
    public async Task<IActionResult> Submit(int id, [FromBody] SubmitProductionOrderDTO? dto = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _repository.SubmitAsync(userId!, id, dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
