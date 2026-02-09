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
public class ReceiptPurchaseOrderBatchController : ControllerBase
{
    private readonly IReceiptPurchaseOrderBatchRepository _repository;
    private readonly ILogger<ReceiptPurchaseOrderBatchController> _logger;

    public ReceiptPurchaseOrderBatchController(
        IReceiptPurchaseOrderBatchRepository repository,
        ILogger<ReceiptPurchaseOrderBatchController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]

    public async Task<ActionResult<IEnumerable<ReceiptPurchaseOrderBatch>>> GetAll()
    {
        var batches = await _repository.GetAllAsync();
        return Ok(batches);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]

    public async Task<ActionResult<ReceiptPurchaseOrderBatch>> GetById(int id)
    {
        var batch = await _repository.GetByIdAsync(id);
        if (batch == null)
            return NotFound($"ReceiptPurchaseOrderBatch with ID {id} not found.");

        return Ok(batch);
    }

    [HttpGet("receipt-purchase-order-item/{receiptPurchaseOrderItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]

    public async Task<ActionResult<IEnumerable<ReceiptPurchaseOrderBatch>>> GetByReceiptPurchaseOrderItemId(int receiptPurchaseOrderItemId)
    {
        var res = await _repository.GetByReceiptPurchaseOrderItemIdAsync(receiptPurchaseOrderItemId);

        if (!res.Success)
            return BadRequest(res);


        return Ok(res);
    }

    [HttpGet("receipt-purchase-order-item/{receiptPurchaseOrderItemId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]

    public async Task<ActionResult<IEnumerable<ReceiptPurchaseOrderBatch>>> GetByReceiptPurchaseOrderItemIdWithPagination(int receiptPurchaseOrderItemId, int skip, int pageSize)
    {
        var res = await _repository.GetByReceiptPurchaseOrderItemIdWithPaginationAsync(receiptPurchaseOrderItemId, skip, pageSize);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPost("receipt-purchase-order-item/{receiptPurchaseOrderItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Create}")]

    public async Task<ActionResult<ReceiptPurchaseOrderBatch>> Create(int receiptPurchaseOrderItemId, AddReceiptPurchaseOrderBatchDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _repository.AddByReceiptPurchaseOrderItemIdAsync(receiptPurchaseOrderItemId, dto);

        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }


    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Edit}")]

    public async Task<IActionResult> Update(int id, [FromBody] UpdateReceiptPurchaseOrderBatchDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _repository.UpdateReceiptPurchaseOrderBatchAsync(id, dto);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Delete}")]

    public async Task<IActionResult> Delete(int id)
    {
        var batch = await _repository.GetByIdAsync(id);
        if (batch == null)
            return NotFound($"ReceiptPurchaseOrderBatch with ID {id} not found.");

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return NoContent();
    }



}

