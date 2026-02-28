using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.Processes.OutSide;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SalesReturnOrderItemController : ControllerBase
{
    private readonly ISalesReturnOrderItemRepository _repository;
    private readonly ILogger<SalesReturnOrderItemController> _logger;

    public SalesReturnOrderItemController(
        ISalesReturnOrderItemRepository repository,
        ILogger<SalesReturnOrderItemController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<ActionResult<IEnumerable<SalesReturnOrderItem>>> GetAll()
    {
        var salesReturnOrderItems = await _repository.GetAllAsync();
        return Ok(salesReturnOrderItems);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<ActionResult<SalesReturnOrderItem>> GetById(int id)
    {
        var salesReturnOrderItem = await _repository.GetByIdAsync(id);
        if (salesReturnOrderItem == null)
            return NotFound($"SalesReturnOrderItem with ID {id} not found.");

        return Ok(salesReturnOrderItem);
    }

    [HttpGet("sales-return-order/{salesReturnOrderId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<ActionResult<IEnumerable<SalesReturnOrderItem>>> GetBySalesReturnOrderId(int salesReturnOrderId)
    {
        var salesReturnOrderItems = await _repository.GetBySalesReturnOrderIdAsync(salesReturnOrderId);
        if (!salesReturnOrderItems.Success)
            return BadRequest(salesReturnOrderItems);

        return Ok(salesReturnOrderItems);
    }

    [HttpGet("sales-return-order/{salesReturnOrderId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<ActionResult<IEnumerable<SalesReturnOrderItem>>> GetBySalesReturnOrderIdWithPagination(int salesReturnOrderId, int skip, int pageSize)
    {
        var res = await _repository.GetBySalesReturnOrderIdWithPaginationAsync(salesReturnOrderId, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPost("sales-order/{salesReturnOrderId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Create}")]

    public async Task<ActionResult<SalesReturnOrderItem>> Create(int salesReturnOrderId, AddSalesReturnOrderItemDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddSalesReturnOrderItemBySalesOrderItemIdAsync(userId, salesReturnOrderId, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Edit}")]

    public async Task<IActionResult> Update(int id, UpdateSalesReturnOrderItemDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _repository.UpdateSalesReturnOrderItemAsync(id, dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPost("witout-reference/sales-return-order/{salesReturnOrderId}/add-barcode-or-no/{isBarcode:bool}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Create}")]
    public async Task<ActionResult<SalesReturnOrderItem>> CreateWithoutReference(
        int salesReturnOrderId,
        bool isBarcode,
        AddGeneralItemByManualOrBarcodeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _repository.AddSalesReturnItemBySalesReturnOrderIdWithoutRefAsync(
            salesReturnOrderId,
            isBarcode,
            dto.Barcode,
            dto.Item);

        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("witout-reference/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Edit}")]
    public async Task<IActionResult> UpdateWithoutReference(int id, UpdateGeneralItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _repository.UpdateSalesReturnItemWithoutRefAsync(id, dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Delete}")]

    public async Task<IActionResult> Delete(int id)
    {
        var salesReturnOrderItem = await _repository.GetByIdAsync(id);
        if (salesReturnOrderItem == null)
            return NotFound($"SalesReturnOrderItem with ID {id} not found.");

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return Ok("deleted");
    }

    [HttpGet("{id}/with-batches")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<ActionResult<SalesReturnOrderItem>> GetWithBatches(int id)
    {
        var salesReturnOrderItem = await _repository.GetWithBatchesAsync(id);
        if (salesReturnOrderItem == null)
            return NotFound($"SalesReturnOrderItem with ID {id} not found.");

        return Ok(salesReturnOrderItem);
    }

    [HttpGet("{id}/with-sales-order-item")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<ActionResult<SalesReturnOrderItem>> GetWithSalesOrderItem(int id)
    {
        var salesReturnOrderItem = await _repository.GetWithSalesOrderItemAsync(id);
        if (salesReturnOrderItem == null)
            return NotFound($"SalesReturnOrderItem with ID {id} not found.");

        return Ok(salesReturnOrderItem);
    }
}

