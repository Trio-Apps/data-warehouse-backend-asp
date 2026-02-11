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

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<ActionResult<SalesReturnOrder>> GetById(int id)
    {
        var salesReturnOrder = await _repository.GetByIdAsync(id);
        if (salesReturnOrder == null)
            return NotFound($"SalesReturnOrder with ID {id} not found.");

        return Ok(salesReturnOrder);
    }

    [HttpGet("sales-order/{salesOrderId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<IActionResult> GetBySalesOrderId(int salesOrderId)
    {
        var salesReturnOrder = await _repository.GetBySalesOrderIdAsync(salesOrderId);
        if (!salesReturnOrder.Success)
            return NotFound($"SalesReturnOrder for SalesOrder ID {salesOrderId} not found.");

        return Ok(salesReturnOrder);
    }
    [HttpGet("by-sales-order/{salesOrderId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<IActionResult> GetWithCustomerBySalesOrderId(int salesOrderId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var salesReturnOrder = await _repository.GetWithCustomerAsync(salesOrderId,userId);
        if (!salesReturnOrder.Success)
            return NotFound($"SalesReturnOrder for SalesOrder ID {salesOrderId} not found.");

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

    [HttpGet("{id}/with-sales-order")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.SalesReturn_Get}")]

    public async Task<ActionResult<SalesReturnOrder>> GetWithSalesOrder(int id)
    {
        var salesReturnOrder = await _repository.GetWithSalesOrderAsync(id);
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

        var created = await _repository.AddSalesReturnOrderBySalesOrderIdAsync(userId, dto);
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
        var salesReturnOrder = await _repository.GetByIdAsync(id);
        if (salesReturnOrder == null)
            return NotFound($"SalesReturnOrder with ID {id} not found.");

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return NoContent();
    }
}

