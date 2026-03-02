using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.Processes.BulkProductions;

[Route("api/production-component-batches")]
[ApiController]
[Authorize]
public class ProductionComponentBatchController : ControllerBase
{
    private readonly IProductionComponentBatchRepository _repository;

    public ProductionComponentBatchController(IProductionComponentBatchRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Create}")]
    public async Task<IActionResult> Create([FromBody] AddProductionComponentBatchDTO dto)
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
        [FromQuery] int productionComponentLineId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        if (productionComponentLineId <= 0)
            return BadRequest("productionComponentLineId is required.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _repository.GetListAsync(userId!, productionComponentLineId, pageNumber, pageSize);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("available/{productionComponentLineId:int}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Get}")]
    public async Task<IActionResult> GetAvailableBatches(int productionComponentLineId)
    {
        if (productionComponentLineId <= 0)
            return BadRequest("productionComponentLineId is required.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _repository.GetAvailableBatchesAsync(userId!, productionComponentLineId);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Productions_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductionComponentBatchDTO dto)
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
        var result = await _repository.DeleteAsync(userId!, id);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
