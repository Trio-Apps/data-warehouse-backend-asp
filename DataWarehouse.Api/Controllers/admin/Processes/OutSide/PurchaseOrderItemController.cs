using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.PurchaseOrders;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Core.IServices.Processes.OutSide;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin.Processes.OutSide;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PurchaseOrderItemController : ControllerBase
{
    private readonly IPurchaseOrderItemRepository _repository;
    private readonly ILogger<PurchaseOrderItemController> _logger;

    public PurchaseOrderItemController(
        IPurchaseOrderItemRepository repository,
        ILogger<PurchaseOrderItemController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]
    public async Task<ActionResult<IEnumerable<PurchaseOrderItem>>> GetAll()
    {
        var PurchaseOrderItems = await _repository.GetAllAsync();
        return Ok(PurchaseOrderItems);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]

    public async Task<ActionResult<PurchaseOrderItem>> GetById(int id)
    {
        var PurchaseOrderItem = await _repository.GetByIdAsync(id);
        if (PurchaseOrderItem == null)
            return NotFound($"PurchaseOrderItem with ID {id} not found.");

        return Ok(PurchaseOrderItem);
    }

    [HttpGet("purchase-order/{PurchaseOrderId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]
    public async Task<ActionResult<IEnumerable<PurchaseOrderItem>>> GetByPurchaseOrderId(int PurchaseOrderId)
    {
        var PurchaseOrderItems = await _repository.GetByPurchaseItemByPurchaseOrderIdAsync(PurchaseOrderId);

        return Ok(PurchaseOrderItems);
    }


    [HttpGet("status/purchase-order/{PurchaseOrderId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]
    public async Task<ActionResult<IEnumerable<PurchaseOrderItem>>>
        GetByPurchaseOrderIdWithPagination(int PurchaseOrderId, string? status, int skip, int pageSize)
    {
        var res = await _repository.GetByPurchaseItemByPurchaseOrderIdWithPaginationAsync(PurchaseOrderId, status, skip, pageSize);
        return Ok(res);
    }
    [HttpPost("Purchase-order/{PurchaseOrderId}/add-barcode-or-no/{isBarcode:bool}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Create}")]
    public async Task<ActionResult<PurchaseOrderItem>> Create(
    int PurchaseOrderId,
    bool isBarcode,
    AddPurchaseOrderItemCreateRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Case 1: Barcode = true
        if (isBarcode)
        {
          if (dto.Barcode == null)
             return BadRequest(" barcode should not be sent with static type");
        }
     

        var created = await _repository.AddPurchaseItemByPurchaseOrderIdAsync(PurchaseOrderId,
            isBarcode,
            dto.Barcode,          
            dto.Item);


        return Ok(created);
    }




    [HttpPut("Purchase-item-order/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Edit}")]

    public async Task<IActionResult> Update(
     int id,
       UpdateGeneralItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

       

        var res = await _repository.UpdatePurchaseItemAsync(id, dto);


        if (!res.Success)
            return BadRequest(res);


        return Ok(res);
    }


    [HttpDelete("Purchase-item-order/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {

        var res = await _repository.DeletePurchaseItemAsync(id);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    
}

