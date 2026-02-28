using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Api.Controllers.admin.Processes.OutSide;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SalesReturnOrderBatchController : ControllerBase
{
    private readonly ISalesReturnOrderBatchRepository _repository;
    private readonly ILogger<SalesReturnOrderBatchController> _logger;

    public SalesReturnOrderBatchController(
        ISalesReturnOrderBatchRepository repository,
        ILogger<SalesReturnOrderBatchController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<ActionResult<IEnumerable<SalesReturnOrderBatch>>> GetAll()
    {
        var batches = await _repository.GetAllAsync();
        return Ok(batches);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<ActionResult<SalesReturnOrderBatch>> GetById(int id)
    {
        var batch = await _repository.GetByIdAsync(id);
        if (batch == null)
            return NotFound($"SalesReturnOrderBatch with ID {id} not found.");

        return Ok(batch);
    }

    [HttpGet("sales-return-order-item/{salesReturnOrderItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<ActionResult<IEnumerable<SalesReturnOrderBatch>>> GetBySalesReturnOrderItemId(int salesReturnOrderItemId)
    {
        var res = await _repository.GetBySalesReturnOrderItemIdAsync(salesReturnOrderItemId);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("sales-return-order-item/{salesReturnOrderItemId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<ActionResult<IEnumerable<SalesReturnOrderBatch>>> GetBySalesReturnOrderItemIdWithPagination(int salesReturnOrderItemId, int skip, int pageSize)
    {
        var res = await _repository.GetBySalesReturnOrderItemIdWithPaginationAsync(salesReturnOrderItemId, skip, pageSize);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPost("sales-return-order-item/{salesReturnOrderItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Create}")]

    public async Task<ActionResult<SalesReturnOrderBatch>> Create(int salesReturnOrderItemId, GeneralBatchDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _repository.AddBySalesReturnOrderItemIdAsync(salesReturnOrderItemId, dto);

        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGeneralBatchDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _repository.UpdateSalesReturnOrderBatchAsync(id, dto);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _repository.DeleteSalesReturnOrderBatchAsync(id);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }


}

