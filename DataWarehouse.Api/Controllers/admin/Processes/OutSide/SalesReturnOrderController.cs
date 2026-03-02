using DataWarehouse.Core.DTOs.Processes.OutSide;
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
public class SalesReturnOrderController : ControllerBase
{
    private readonly ISalesReturnOrderRepository _repository;
    private readonly ILogger<SalesReturnOrderController> _logger;

    public SalesReturnOrderController(
        ISalesReturnOrderRepository repository,
        ILogger<SalesReturnOrderController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]
    public async Task<ActionResult<IEnumerable<SalesReturnOrder>>> GetAll()
    {
        var salesReturnOrders = await _repository.GetAllAsync();
        return Ok(salesReturnOrders);
    }
    
     
    [HttpGet("warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]
    public async Task<ActionResult<IEnumerable<SalesReturnOrder>>> GetByWarehouseId(int warehouseId)
    {
        var salesReturnOrders = await _repository.GetByWarehouseIdAsync(warehouseId);
        return Ok(salesReturnOrders);
    }


    [HttpGet("warehouse/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]
    public async Task<IActionResult> GetByWarehouseIdWithPagination(int warehouseId, int skip, int pageSize)
    {
        var res = await _repository.GetByWarehouseIdWithPaginationAsync(warehouseId, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }


    [HttpGet("dashboard/warehouse/status/posting-date/due-date/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]
    public async Task<IActionResult> GetByWarehouseIdWithPagination(int warehouseId, int? customerId, string? status, DateTime? postingDate, DateTime? dueDate, int skip, int pageSize)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await _repository.GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(warehouseId, userId, customerId, postingDate, dueDate, status, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }



    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]
    public async Task<IActionResult> GetSalesReturnById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await _repository.GetSalesReturnOrderByIdAsync(userId, id);
        if (!res.Success)
            return NotFound(res);

        return Ok(res);
    }

    [HttpGet("delivery-note-order/{deliveryNoteOrderId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<IActionResult> GetBydeliveryNoteOrderId(int deliveryNoteOrderId)
    {
        var salesReturnOrder = await _repository.GetByDeliveryNoteOrderIdAsync(deliveryNoteOrderId);
        if (!salesReturnOrder.Success)
            return NotFound($"SalesReturnOrder for SalesOrder ID {deliveryNoteOrderId} not found.");

        return Ok(salesReturnOrder);
    }
    [HttpGet("by-delivery-note-order/{deliveryNoteOrderId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<IActionResult> GetWithCustomerBydeliveryNoteOrderId(int deliveryNoteOrderId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var salesReturnOrder = await _repository.GetWithCustomerAsync(deliveryNoteOrderId,userId);
        if (!salesReturnOrder.Success)
            return NotFound($"SalesReturnOrder for SalesOrder ID {deliveryNoteOrderId} not found.");

        return Ok(salesReturnOrder);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<ActionResult<IEnumerable<SalesReturnOrder>>> GetByUserId(string userId)
    {
        var salesReturnOrders = await _repository.GetByUserIdAsync(userId);
        return Ok(salesReturnOrders);
    }

    [HttpGet("{id}/with-items")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<ActionResult<SalesReturnOrder>> GetWithItems(int id)
    {
        var salesReturnOrder = await _repository.GetWithItemsAsync(id);
        if (salesReturnOrder == null)
            return NotFound($"SalesReturnOrder with ID {id} not found.");

        return Ok(salesReturnOrder);
    }

    [HttpGet("{id}/with-items-batches")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<IActionResult> GetWithItemsAndBatches(int id)
    {
        var res = await _repository.GetWithItemsAndBatchesAsync(id);
        if (!res.Success)
            return NotFound(res);

        return Ok(res);
    }

    [HttpGet("{id}/with-delivery-note-order")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<ActionResult<SalesReturnOrder>> GetWithSalesOrder(int id)
    {
        var salesReturnOrder = await _repository.GetWithDeliveryNoteOrderAsync(id);
        if (salesReturnOrder == null)
            return NotFound($"SalesReturnOrder with ID {id} not found.");

        return Ok(salesReturnOrder);
    }

    [HttpGet("{id}/with-warehouse")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]
    public async Task<ActionResult<SalesReturnOrder>> GetWithWarehouse(int id)
    {
        var salesReturnOrder = await _repository.GetWithWarehouseAsync(id);
        if (salesReturnOrder == null)
            return NotFound($"SalesReturnOrder with ID {id} not found.");

        return Ok(salesReturnOrder);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Create}")]

    public async Task<ActionResult<SalesReturnOrder>> Create(AddSalesReturnOrderDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddSalesReturnOrderAsync(userId, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPost("without-reference")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Create}")]
    public async Task<ActionResult<SalesReturnOrder>> CreateWithoutReference(AddSalesReturnOrderWithoutRefDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddSalesReturnOrderWithoutRefAsync(userId, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPost("with-default-items")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Create}")]
    public async Task<ActionResult<SalesReturnOrder>> CreateWithDefaultItems(AddSalesReturnOrderDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddSalesReturnOrderAndItemsByDeliveryNoteOrderIdAsync(userId, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSalesReturnOrderDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await _repository.UpdateSalesReturnOrderAsync(userId, id, dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _repository.DeleteSalesReturnOrderAsync(id);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

}

