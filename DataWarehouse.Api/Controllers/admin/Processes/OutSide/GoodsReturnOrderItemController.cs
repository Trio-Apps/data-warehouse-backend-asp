using DataWarehouse.Core.DTOs.Processes;
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
public class GoodsReturnOrderItemController : ControllerBase
{
    private readonly IGoodsReturnOrderItemRepository _repository;
    private readonly ILogger<GoodsReturnOrderItemController> _logger;

    public GoodsReturnOrderItemController(
        IGoodsReturnOrderItemRepository repository,
        ILogger<GoodsReturnOrderItemController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]
    public async Task<ActionResult<IEnumerable<GoodsReturnOrderItem>>> GetAll()
    {
        var goodsReturnOrderItems = await _repository.GetAllAsync();
        return Ok(goodsReturnOrderItems);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]
    public async Task<ActionResult<GoodsReturnOrderItem>> GetById(int id)
    {
        var goodsReturnOrderItem = await _repository.GetByIdAsync(id);
        if (goodsReturnOrderItem == null)
            return NotFound($"GoodsReturnOrderItem with ID {id} not found.");

        return Ok(goodsReturnOrderItem);
    }

    [HttpGet("goods-return-order/{goodsReturnOrderId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]
    public async Task<ActionResult<IEnumerable<GoodsReturnOrderItem>>> GetByGoodsReturnOrderId(int goodsReturnOrderId)
    {
        var goodsReturnOrderItems = await _repository.GetByGoodsReturnOrderIdAsync(goodsReturnOrderId);
        return Ok(goodsReturnOrderItems);
    }

    [HttpGet("goods-return-order/{goodsReturnOrderId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]
    public async Task<ActionResult<IEnumerable<GoodsReturnOrderItem>>> GetByGoodsReturnOrderIdWithPagination(int goodsReturnOrderId, int skip, int pageSize)
    {
        var res = await _repository.GetByGoodsReturnOrderIdWithPaginationAsync(goodsReturnOrderId, skip, pageSize);
        return Ok(res);
    }

    [HttpPost("receipt-purchase-order/{receiptOrderId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Create}")]
    public async Task<ActionResult<GoodsReturnOrderItem>> Create(int receiptOrderId, AddGoodsReturnOrderItemDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddGoodsReturnOrderItemByReceiptPurchaseOrderItemIdAsync(userId, receiptOrderId, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Edit}")]
    public async Task<IActionResult> Update(int id, UpdateGoodsReturnOrderItemDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _repository.UpdateGoodsReturnOrderItemAsync(id, dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPost("witout-reference/goods-return-order/{goodsReturnOrderId}/add-barcode-or-no/{isBarcode:bool}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Create}")]
    public async Task<ActionResult<GoodsReturnOrderItem>> CreateWithoutReference
        (int goodsReturnOrderId, bool isBarcode, AddGeneralItemByManualOrBarcodeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddGoodsReturnItemByGoodsReturnOrderIdWithoutRefAsync(goodsReturnOrderId,isBarcode,
            dto.Barcode,dto.Item);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("witout-reference/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Edit}")]
    public async Task<IActionResult> UpdateWithoutReference(int id, UpdateGeneralItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _repository.UpdateGoodsReturnItemWithoutRefAsync(id, dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var goodsReturnOrderItem = await _repository.GetByIdAsync(id);
        if (goodsReturnOrderItem == null)
            return NotFound($"GoodsReturnOrderItem with ID {id} not found.");

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return Ok("deleted");
    }

    [HttpGet("{id}/with-batches")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]
    public async Task<ActionResult<GoodsReturnOrderItem>> GetWithBatches(int id)
    {
        var goodsReturnOrderItem = await _repository.GetWithBatchesAsync(id);
        if (goodsReturnOrderItem == null)
            return NotFound($"GoodsReturnOrderItem with ID {id} not found.");

        return Ok(goodsReturnOrderItem);
    }

    [HttpGet("{id}/with-receipt-purchase-order-item")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]
    public async Task<ActionResult<GoodsReturnOrderItem>> GetWithReceiptPurchaseOrderItem(int id)
    {
        var goodsReturnOrderItem = await _repository.GetWithReceiptPurchaseOrderItemAsync(id);
        if (goodsReturnOrderItem == null)
            return NotFound($"GoodsReturnOrderItem with ID {id} not found.");

        return Ok(goodsReturnOrderItem);
    }
}
