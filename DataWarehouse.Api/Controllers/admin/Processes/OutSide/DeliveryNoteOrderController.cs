using DataWarehouse.Api;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Queue;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class DeliveryNoteOrderController : ControllerBase
{
    private readonly ISapJobQueuer jobQueuer;
    private readonly IDeliveryNoteOrderRepository _repository;
    private readonly ILogger<DeliveryNoteOrderController> _logger;


    public DeliveryNoteOrderController(
        ISapJobQueuer jobQueuer,
        IDeliveryNoteOrderRepository repository,
        ILogger<DeliveryNoteOrderController> logger)
    {
        this.jobQueuer = jobQueuer;
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
    public async Task<ActionResult<IEnumerable<DeliveryNoteOrder>>> GetAll()
    {
        var deliveryNotes = await _repository.GetAllAsync();
        return Ok(deliveryNotes);
    }

    [HttpGet("warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
    public async Task<ActionResult<IEnumerable<DeliveryNoteOrder>>> GetByWarehouseId(int warehouseId)
    {
        var deliveryNotes = await _repository.GetByWarehouseIdAsync(warehouseId);
        return Ok(deliveryNotes);
    }

    [HttpGet("warehouse/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
    public async Task<IActionResult> GetByWarehouseIdWithPagination(int warehouseId, int skip, int pageSize)
    {
        var res = await _repository.GetByWarehouseIdWithPaginationAsync(warehouseId, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("dashboard/warehouse/status/posting-date/due-date/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
    public async Task<IActionResult> GetByWarehouseIdForDashboard(
        int warehouseId,
        int? customerId,
        string? status,
        DateTime? postingDate,
        DateTime? dueDate,
        int skip,
        int pageSize)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await _repository.GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(
            warehouseId,
            userId!,
            customerId,
            postingDate,
            dueDate,
            status,
            skip,
            pageSize);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
    public async Task<IActionResult> GetDeliveryNoteById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await _repository.GetDeliveryNoteOrderByIdAsync(userId!, id);
        if (!res.Success)
            return NotFound(res);

        return Ok(res);
    }

    // Get Delivery Note by SalesOrderId (Parent)
    [HttpGet("sales-order/{salesOrderId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
    public async Task<IActionResult> GetBySalesOrderId(int salesOrderId)
    {
        var deliveryNote = await _repository.GetBySalesOrderIdAsync(salesOrderId);
        if (!deliveryNote.Success)
            return NotFound($"DeliveryNoteOrder for SalesOrder ID {salesOrderId} not found.");

        return Ok(deliveryNote);
    }

    // not used - same style as SalesReturn
    [HttpGet("by-delivery-note-order/{deliveryNoteOrderId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
    public async Task<IActionResult> GetWithCustomer(int deliveryNoteOrderId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await _repository.GetWithCustomerAsync(deliveryNoteOrderId, userId!);
        if (!res.Success)
            return NotFound($"DeliveryNoteOrder ID {deliveryNoteOrderId} not found.");

        return Ok(res);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
    public async Task<ActionResult<IEnumerable<DeliveryNoteOrder>>> GetByUserId(string userId)
    {
        var deliveryNotes = await _repository.GetByUserIdAsync(userId);
        return Ok(deliveryNotes);
    }

    [HttpGet("{id}/with-items")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
    public async Task<ActionResult<DeliveryNoteOrder>> GetWithItems(int id)
    {
        var deliveryNote = await _repository.GetWithItemsAsync(id);
        if (deliveryNote == null)
            return NotFound($"DeliveryNoteOrder with ID {id} not found.");

        return Ok(deliveryNote);
    }

    [HttpGet("{id}/with-items-batches")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
    public async Task<IActionResult> GetWithItemsAndBatches(int id)
    {
        var res = await _repository.GetWithItemsAndBatchesAsync(id);
        if (!res.Success)
            return NotFound(res);

        return Ok(res);
    }

    [HttpGet("{id}/with-sales-order")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
    public async Task<ActionResult<DeliveryNoteOrder>> GetWithSalesOrder(int id)
    {
        var deliveryNote = await _repository.GetWithSalesOrderAsync(id);
        if (deliveryNote == null)
            return NotFound($"DeliveryNoteOrder with ID {id} not found.");

        return Ok(deliveryNote);
    }

    [HttpGet("{id}/with-warehouse")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
    public async Task<ActionResult<DeliveryNoteOrder>> GetWithWarehouse(int id)
    {
        var deliveryNote = await _repository.GetWithWarehouseAsync(id);
        if (deliveryNote == null)
            return NotFound($"DeliveryNoteOrder with ID {id} not found.");

        return Ok(deliveryNote);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Create}")]
    public async Task<ActionResult<DeliveryNoteOrder>> Create(AddDeliveryNoteOrderDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddDeliveryNoteOrderAsync(userId!, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPost("without-reference")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Create}")]
    public async Task<ActionResult<DeliveryNoteOrder>> CreateWithoutReference(AddDeliveryNoteOrderWithoutRefDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddDeliveryNoteOrderWithoutRefAsync(userId!, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    // Create DeliveryNote from SalesOrder + default items copied
    [HttpPost("with-default-items")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Create}")]
    public async Task<ActionResult<DeliveryNoteOrder>> CreateWithDefaultItems(AddDeliveryNoteOrderDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddDeliveryNoteOrderAndItemsBySalesOrderIdAsync(userId!, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDeliveryNoteOrderDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await _repository.UpdateDeliveryNoteOrderAsync(userId!, id, dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPatch("{id}/revert-partially-failed")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Edit}")]
    public async Task<IActionResult> RevertPartiallyFailedStatus(int id)
    {
        var res = await _repository.RevertPartiallyFailedStatusToProcessingAsync(id);

        await jobQueuer.DistributionOrders(res.Data);

        if (!res.Success)
            return BadRequest(res);


        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _repository.DeleteDeliveryNoteOrderAsync(id);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }
}
