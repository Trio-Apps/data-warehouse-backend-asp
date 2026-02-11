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
public class SalesOrderBatchController : ControllerBase
{
    private readonly ISalesOrderBatchRepository _repository;
    private readonly ILogger<SalesOrderBatchController> _logger;

    public SalesOrderBatchController(
        ISalesOrderBatchRepository repository,
        ILogger<SalesOrderBatchController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]

    public async Task<ActionResult<IEnumerable<SalesOrderBatch>>> GetAll()
    {
        var batches = await _repository.GetAllAsync();
        return Ok(batches);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]

    public async Task<ActionResult<SalesOrderBatch>> GetById(int id)
    {
        var batch = await _repository.GetByIdAsync(id);
        if (batch == null)
            return NotFound($"SalesOrderBatch with ID {id} not found.");

        return Ok(batch);
    }

    [HttpGet("sales-order-item/{salesOrderItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]

    public async Task<ActionResult<IEnumerable<SalesOrderBatch>>> GetBySalesOrderItemId(int salesOrderItemId)
    {
        var res = await _repository.GetBySalesOrderItemIdAsync(salesOrderItemId);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("sales-order-item/{salesOrderItemId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]

    public async Task<ActionResult<IEnumerable<SalesOrderBatch>>> GetBySalesOrderItemIdWithPagination(int salesOrderItemId, int skip, int pageSize)
    {
        var res = await _repository.GetBySalesOrderItemIdWithPaginationAsync(salesOrderItemId, skip, pageSize);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPost("sales-order-item/{salesOrderItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Create}")]

    public async Task<IActionResult> Create(int salesOrderItemId, GeneralBatchDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _repository.AddBySalesOrderItemIdAsync(salesOrderItemId, dto);

        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Edit}")]

    public async Task<IActionResult> Update(int id, [FromBody] UpdateGeneralBatchDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _repository.UpdateSalesOrderBatchAsync(id, dto);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Delete}")]

    public async Task<IActionResult> Delete(int id)
    {
      
      var res =  await _repository.DeleteSalesOrderBatchAsync(id);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }
}

