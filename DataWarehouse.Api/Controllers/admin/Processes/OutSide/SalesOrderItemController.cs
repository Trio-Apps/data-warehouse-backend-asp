using DataWarehouse.Core.DTOs.Processes;
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
public class SalesOrderItemController : ControllerBase
{
    private readonly ISalesOrderItemRepository _repository;
    private readonly ILogger<SalesOrderItemController> _logger;

    public SalesOrderItemController(
        ISalesOrderItemRepository repository,
        ILogger<SalesOrderItemController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]

    public async Task<ActionResult<IEnumerable<SalesOrderItem>>> GetAll()
    {
        var SalesOrderItems = await _repository.GetAllAsync();
        return Ok(SalesOrderItems);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]

    public async Task<ActionResult<SalesOrderItem>> GetById(int id)
    {
        var SalesOrderItem = await _repository.GetByIdAsync(id);
        if (SalesOrderItem == null)
            return NotFound($"SalesOrderItem with ID {id} not found.");

        return Ok(SalesOrderItem);
    }

    [HttpGet("sales-order/{SalesOrderId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]

    public async Task<ActionResult<IEnumerable<SalesOrderItem>>> GetBySalesOrderId(int SalesOrderId)
    {
        var SalesOrderItems = await _repository.GetBySalesItemBySalesOrderIdAsync(SalesOrderId);
        return Ok(SalesOrderItems);
    }

    [HttpGet("status/sales-order/{SalesOrderId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]

    public async Task<ActionResult<IEnumerable<SalesOrderItem>>>
        GetBySalesOrderIdWithPagination(int SalesOrderId, string? status, int skip, int pageSize)
    {
        var res = await _repository.GetBySalesItemBySalesOrderIdWithPaginationAsync(SalesOrderId, status, skip, pageSize);
        return Ok(res);
    }

    [HttpPost("Sales-order/{SalesOrderId}/add-barcode-or-no/{isBarcode:bool}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Create}")]

    public async Task<ActionResult<SalesOrderItem>> Create(
    int SalesOrderId,
    bool isBarcode,
    AddSalesOrderItemCreateRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Case 1: Barcode = true
        if (isBarcode)
        {
            if (dto.Barcode == null)
                return BadRequest(" barcode should not be sent with static type");
        }

        var created = await _repository.AddSalesItemBySalesOrderIdAsync(SalesOrderId,
            isBarcode,
            dto.Barcode,
            dto.Item);

        return Ok(created);
    }

    [HttpPut("Sales-item-order/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Edit}")]

    public async Task<IActionResult> Update(
     int id,
       UpdateGeneralItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _repository.UpdateSalesItemAsync(id, dto);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("Sales-item-order/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Delete}")]

    public async Task<IActionResult> Delete(int id)
    {
      
      var res =  await _repository.DeleteSalesItemAsync(id);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }
}

