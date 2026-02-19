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
public class ReceiptPurchaseOrderController : ControllerBase
{
    private readonly IReceiptPurchaseOrderRepository _repository;
    private readonly ILogger<ReceiptPurchaseOrderController> _logger;

    public ReceiptPurchaseOrderController(
        IReceiptPurchaseOrderRepository repository,
        ILogger<ReceiptPurchaseOrderController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]

    public async Task<ActionResult<IEnumerable<ReceiptPurchaseOrder>>> GetAll()
    {
        var receiptPurchaseOrders = await _repository.GetAllAsync();
        return Ok(receiptPurchaseOrders);
    }

    [HttpGet("warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]
    public async Task<ActionResult<IEnumerable<ReceiptPurchaseOrder>>> GetByWarehouseId(int warehouseId)
    {
        var receiptPurchaseOrders = await _repository.GetByWarehouseIdAsync(warehouseId);
        return Ok(receiptPurchaseOrders);
    }

    [HttpGet("warehouse/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]
    public async Task<IActionResult> GetByWarehouseIdWithPagination(int warehouseId, int skip, int pageSize)
    {
        var res = await _repository.GetByWarehouseIdWithPaginationAsync(warehouseId, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }
    [HttpGet("dashboard/warehouse/status/posting-date/due-date/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]
    public async Task<IActionResult> GetByWarehouseIdWithPagination(int warehouseId, int? supplierId, string? status, string? liveStatus, DateTime? postingDate, DateTime? dueDate, int skip, int pageSize)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await _repository.GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(warehouseId, userId, supplierId, postingDate, dueDate, liveStatus, status, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    //[HttpGet("{id}")]
    //[Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]

    //public async Task<ActionResult<ReceiptPurchaseOrder>> GetById(int id)
    //{
    //    var receiptPurchaseOrder = await _repository.GetByIdAsync(id);
    //    if (receiptPurchaseOrder == null)
    //        return NotFound($"ReceiptPurchaseOrder with ID {id} not found.");

    //    return Ok(receiptPurchaseOrder);
    //}


    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]

    public async Task<ActionResult<ReceiptPurchaseOrder>> GetReceiptById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await _repository.GetReceiptOrderByIdAsync(userId, id);
        if (!res.Success)
            return NotFound(res);

        return Ok(res);
    }
    [HttpGet("purchase-order/{purchaseOrderId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]

    public async Task<ActionResult<ReceiptPurchaseOrder>> GetByPurchaseOrderId(int purchaseOrderId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var receiptPurchaseOrder = await _repository.GetByPurchaseOrderIdAsync(userId, purchaseOrderId);
        if (!receiptPurchaseOrder.Success)
            return NotFound($"ReceiptPurchaseOrder for PurchaseOrder ID {purchaseOrderId} not found.");

        return Ok(receiptPurchaseOrder);
    }

    [HttpGet("status/{status}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]

    public async Task<ActionResult<IEnumerable<ReceiptPurchaseOrder>>> GetByStatus(string status)
    {
        var receiptPurchaseOrders = await _repository.GetByStatusAsync(status);
        return Ok(receiptPurchaseOrders);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]

    public async Task<ActionResult<IEnumerable<ReceiptPurchaseOrder>>> GetByUserId(string userId)
    {
        var receiptPurchaseOrders = await _repository.GetByUserIdAsync(userId);
        return Ok(receiptPurchaseOrders);
    }

    [HttpGet("{id}/with-items")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]

    public async Task<ActionResult<ReceiptPurchaseOrder>> GetWithItems(int id)
    {
        var receiptPurchaseOrder = await _repository.GetWithItemsAsync(id);
        if (receiptPurchaseOrder == null)
            return NotFound($"ReceiptPurchaseOrder with ID {id} not found.");

        return Ok(receiptPurchaseOrder);
    }

    [HttpGet("{id}/with-purchase-order")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]
    public async Task<ActionResult<ReceiptPurchaseOrder>> GetWithPurchaseOrder(int id)
    {
        var receiptPurchaseOrder = await _repository.GetWithPurchaseOrderAsync(id);
        if (receiptPurchaseOrder == null)
            return NotFound($"ReceiptPurchaseOrder with ID {id} not found.");

        return Ok(receiptPurchaseOrder);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Create}")]
    public async Task<ActionResult<ReceiptPurchaseOrder>> Create(AddReceiptPurchaseOrderDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddReceiptPurchaseOrderByPurchaseOrderIdAsync(userId, dto);

        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }
    [HttpPost("with-default-items")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Create}")]
    public async Task<ActionResult<ReceiptPurchaseOrder>> CreateWithDefaultItems(AddReceiptPurchaseOrderDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddReceiptPurchaseOrderAndItemsByPurchaseOrderIdAsync(userId, dto);

        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }


    [HttpPost("without-reference")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Create}")]
    public async Task<ActionResult<ReceiptPurchaseOrder>> CreateWithoutReference(AddReceiptPurchaseOrderWithoutRefDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddReceiptPurchaseOrderAsync(userId, dto);

        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }
    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Edit}")]

    public async Task<IActionResult> Update(int id, [FromBody] UpdateReceiptPurchaseOrderDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await _repository.UpdateReceiptPurchaseOrderAsync(userId, id, dto);
        if (!res.Success) return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Delete}")]

    public async Task<IActionResult> Delete(int id)
    {
        var res =  await _repository.DeleteReceiptOrderAsync(id);

        if (!res.Success)
            return BadRequest(res);
        return Ok(res);
    }

    [HttpGet("processing")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]

    public async Task<ActionResult<IEnumerable<ReceiptPurchaseOrder>>> GetPendingReceipts()
    {
        var receiptPurchaseOrders = await _repository.GetPendingReceiptsAsync();
        return Ok(receiptPurchaseOrders);
    }
}

