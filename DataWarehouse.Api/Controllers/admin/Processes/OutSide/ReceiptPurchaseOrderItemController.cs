using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin.Processes.OutSide;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReceiptPurchaseOrderItemController : ControllerBase
{
    private readonly IReceiptPurchaseOrderItemRepository _repository;
    private readonly ILogger<ReceiptPurchaseOrderItemController> _logger;

    public ReceiptPurchaseOrderItemController(
        IReceiptPurchaseOrderItemRepository repository,
        ILogger<ReceiptPurchaseOrderItemController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]

    public async Task<ActionResult<IEnumerable<ReceiptPurchaseOrderItem>>> GetAll()
    {
        var receiptPurchaseOrderItems = await _repository.GetAllAsync();
        return Ok(receiptPurchaseOrderItems);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]

    public async Task<ActionResult<ReceiptPurchaseOrderItem>> GetById(int id)
    {
        var receiptPurchaseOrderItem = await _repository.GetByIdAsync(id);
        if (receiptPurchaseOrderItem == null)
            return NotFound($"ReceiptPurchaseOrderItem with ID {id} not found.");

        return Ok(receiptPurchaseOrderItem);
    }

    [HttpGet("receipt-purchase-order/{ReceiptPurchaseOrderId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]

    public async Task<ActionResult<IEnumerable<ReceiptPurchaseOrderItem>>> GetByReceiptPurchaseOrderId(int ReceiptPurchaseOrderId)
    {
        var receiptPurchaseOrderItems = await _repository.GetByReceiptPurchaseItemByReceiptPurchaseOrderIdAsync(ReceiptPurchaseOrderId);
        return Ok(receiptPurchaseOrderItems);
    }

    [HttpGet("status/receipt-purchase-order/{ReceiptPurchaseOrderId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Get}")]

    public async Task<ActionResult<IEnumerable<ReceiptPurchaseOrderItem>>>
        GetByReceiptPurchaseOrderIdWithPagination(int ReceiptPurchaseOrderId, int skip, int pageSize)
    {
        var res = await _repository.GetByReceiptPurchaseItemByReceiptPurchaseOrderIdWithPaginationAsync(ReceiptPurchaseOrderId, skip, pageSize);
        return Ok(res);
    }

    [HttpPost("Receipt-purchase-order/{ReceiptPurchaseOrderId}/add-barcode-or-no/{isBarcode:bool}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Create}")]

    public async Task<ActionResult<PurchaseOrderItem>> Create(
    int ReceiptPurchaseOrderId,
    bool isBarcode,
    AddReceiptPurchaseOrderItemCreateRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Case 1: Barcode = true
        if (isBarcode)
        {
            if (dto.Barcode == null)
                return BadRequest(" barcode should not be sent with static type");

        }

        var created = await _repository.AddReceiptPurchaseItemByReceiptPurchaseOrderIdAsync(
            ReceiptPurchaseOrderId,
           isBarcode,
            dto.Barcode,
          
            dto.Item);

        if(!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("Receipt-purchase-item-order/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Edit}")]

    public async Task<IActionResult> Update(
     int id,

     UpdateReceiptPurchaseOrderItemDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _repository.UpdateReceiptPurchaseItemAsync(
            id,
             dto);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("Receipt-purchase-item-order/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Delete}")]

    public async Task<IActionResult> Delete(int id)
    {
        var receiptPurchaseOrderItem = await _repository.GetByIdAsync(id);
        if (receiptPurchaseOrderItem == null)
            return NotFound($"ReceiptPurchaseOrderItem with ID {id} not found.");

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return Ok(new { message = "deleted" });
    }
}

