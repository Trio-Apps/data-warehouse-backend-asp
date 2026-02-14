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
public class GoodsReturnOrderController : ControllerBase
{
    private readonly IGoodsReturnOrderRepository _repository;
    private readonly ILogger<GoodsReturnOrderController> _logger;

    public GoodsReturnOrderController(
        IGoodsReturnOrderRepository repository,
        ILogger<GoodsReturnOrderController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]
    public async Task<ActionResult<IEnumerable<GoodsReturnOrder>>> GetAll()
    {
        var goodsReturnOrders = await _repository.GetAllAsync();
        return Ok(goodsReturnOrders);
    }

    [HttpGet("warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]
    public async Task<ActionResult<IEnumerable<GoodsReturnOrder>>> GetByWarehouseId(int warehouseId)
    {
        var goodsReturnOrders = await _repository.GetByWarehouseIdAsync(warehouseId);
        return Ok(goodsReturnOrders);
    }

    [HttpGet("dashboard/warehouse/status/posting-date/due-date/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]

    public async Task<IActionResult> GetByWarehouseIdWithPagination(int warehouseId, string? status, DateTime? postingDate, DateTime? dueDate, int skip, int pageSize)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await _repository.GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(warehouseId, userId, postingDate, dueDate, status, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    //[HttpGet("{id}")]
    //[Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]
    //public async Task<ActionResult<GoodsReturnOrder>> GetById(int id)
    //{
    //    var goodsReturnOrder = await _repository.GetByIdAsync(id);
    //    if (goodsReturnOrder == null)
    //        return NotFound($"GoodsReturnOrder with ID {id} not found.");

    //    return Ok(goodsReturnOrder);
    //}

    [HttpGet("receipt-purchase-order/{receiptPurchaseOrderId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]

    public async Task<IActionResult> GetByReceiptPurchaseOrderId(int receiptPurchaseOrderId)
    {
        var goodsReturnOrder = await _repository.GetByReceiptPurchaseOrderIdAsync(receiptPurchaseOrderId);
        if (!goodsReturnOrder.Success)
            return NotFound($"GoodsReturnOrder for ReceiptPurchaseOrder ID {receiptPurchaseOrderId} not found.");

        return Ok(goodsReturnOrder);
    }
    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]

    public async Task<IActionResult> GetGoodsReturnById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var goodsReturnOrder = await _repository.GetGoodsReturnOrderByIdAsync(userId, id);
        if (!goodsReturnOrder.Success)
            return NotFound(goodsReturnOrder);

        return Ok(goodsReturnOrder);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]

    public async Task<ActionResult<IEnumerable<GoodsReturnOrder>>> GetByUserId(string userId)
    {
        var goodsReturnOrders = await _repository.GetByUserIdAsync(userId);
        return Ok(goodsReturnOrders);
    }

    [HttpGet("{id}/with-items")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]

    public async Task<ActionResult<GoodsReturnOrder>> GetWithItems(int id)
    {
        var goodsReturnOrder = await _repository.GetWithItemsAsync(id);
        if (goodsReturnOrder == null)
            return NotFound($"GoodsReturnOrder with ID {id} not found.");

        return Ok(goodsReturnOrder);
    }

    [HttpGet("{id}/with-items-batches")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]

    public async Task<IActionResult> GetWithItemsAndBatches(int id)
    {
        var res = await _repository.GetWithItemsAndBatchesAsync(id);
        if (!res.Success)
            return NotFound(res);

        return Ok(res);
    }

    [HttpGet("{id}/with-receipt-purchase-order")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]

    public async Task<ActionResult<GoodsReturnOrder>> GetWithReceiptPurchaseOrder(int id)
    {
        var goodsReturnOrder = await _repository.GetWithReceiptPurchaseOrderAsync(id);
        if (goodsReturnOrder == null)
            return NotFound($"GoodsReturnOrder with ID {id} not found.");

        return Ok(goodsReturnOrder);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Create}")]

    public async Task<ActionResult<GoodsReturnOrder>> Create(AddGoodsReturnOrderWithoutRefDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddGoodsReturnOrderAsync(userId, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    //[HttpPost("without-reference")]
    //[Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Create}")]

    //public async Task<ActionResult<GoodsReturnOrder>> CreateWithoutReference(AddGoodsReturnOrderWithoutRefDTO dto)
    //{
    //    if (!ModelState.IsValid)
    //        return BadRequest(ModelState);

    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    //    var created = await _repository.AddGoodsReturnOrderAsync(userId, dto);
    //    if (!created.Success)
    //        return BadRequest(created);

    //    return Ok(created);
    //}

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Edit}")]

    public async Task<IActionResult> Update(int id,UpdateGoodsReturnOrderDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await _repository.UpdateGoodsReturnOrderAsync(userId, id, dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Delete}")]

    public async Task<IActionResult> Delete(int id)
    {
       
      var res =  await _repository.DeleteGoodsReturnOrderAsync(id);

        if(!res.Success)
            return BadRequest(res);

        return Ok(res);
    }
}

