using DataWarehouse.Api.Controllers.admin.Processes.BulkProductions;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.DTOs.Processes.PurchaseOrders;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Core.IServices.Processes.OutSide;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.Processes.OutSide;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PurchaseOrderController : ControllerBase
{
    private readonly IPurchaseOrderRepository _repository;
    private readonly ILogger<PurchaseOrderController> _logger;

    public PurchaseOrderController(
        IPurchaseOrderRepository repository,
        ILogger<PurchaseOrderController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]

    public async Task<ActionResult<IEnumerable<ProductionOrder>>> GetAll()
    {
        var productionOrders = await _repository.GetAllAsync();
        return Ok(productionOrders);
    }
    [HttpGet("warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]

    public async Task<ActionResult<IEnumerable<ProductionOrder>>> GetByWarehouseId(int warehouseId)
    {
        var productionOrders = await _repository.GetByWarehouseIdAsync(warehouseId);
        return Ok(productionOrders);
    }

    [HttpGet("warehouse/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]

    public async Task<IActionResult> GetByWarehouseIdWithPagination(int warehouseId, int skip, int pageSize)
    {
        var res = await _repository.GetByWarehouseIdWithPaginationAsync(warehouseId, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    //[HttpGet("warehouse/status/posting-date/due-date/{skip}/{pageSize}")]
    //[Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]

    //public async Task<IActionResult> GetByWarehouseIdWithPagination(int? warehouseId,string? status,DateTime? postingDate,DateTime? dueDate, int skip, int pageSize)
    //{
    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //    var res = await _repository.GetByWarehouseIdAndStatusAndDateWithPaginationAsync(warehouseId,userId,postingDate,dueDate,status, skip, pageSize);
    //    if (!res.Success)
    //        return BadRequest(res);

    //    return Ok(res);
    //}
   
    [HttpGet("dashboard/warehouse/status/posting-date/due-date/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]
    public async Task<IActionResult> GetByWarehouseIdWithPagination(int warehouseId, string? status,string? liveStatus, DateTime? postingDate, DateTime? dueDate, int skip, int pageSize)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var res = await _repository.GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(warehouseId, userId, postingDate, dueDate,liveStatus, status, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);
        return Ok(res);
    }
    [HttpGet("with-supplier/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]

    public async Task<ActionResult<ProductionOrder>> GetByIdWithSupplier(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await _repository.GetWithSupplierAsync(userId, id);
        if (!res.Success)
            return NotFound(res);

        return Ok(res);
    }


    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]

    public async Task<ActionResult<ProductionOrder>> GetById(int id)
    {
        var productionOrder = await _repository.GetByIdAsync(id);
        if (productionOrder == null)
            return NotFound($"ProductionOrder with ID {id} not found.");

        return Ok(productionOrder);
    }




    [HttpGet("status")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]

    public async Task<IActionResult> Status()
    {
        var productionOrder = await _repository.GetPurchaseOrderStatus();

        if (!productionOrder.Success)
            return NotFound(productionOrder);

        return Ok(productionOrder);
    }

    [HttpGet("status/{status}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]

    public async Task<IActionResult> Status(string status)
    {
        var productionOrder = await _repository.GetByStatusAsync(status);

        if (!productionOrder.Success)
            return NotFound(productionOrder);

        return Ok(productionOrder);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Create}")]

    public async Task<ActionResult<ProductionOrder>> Create(AddPurchaseOrderDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);


        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await _repository.AddPurchaseOrderByWarehouseIdAsync(userId, dto);


        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Edit}")]

    public async Task<IActionResult> Update(int id, [FromBody] UpdatePurchaseOrderDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


        var res = await _repository.UpdatePurchaseOrderAsync(userId, id, dto);
        if (!res.Success) return BadRequest(res);


        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Delete}")]

    public async Task<IActionResult> Delete(int id)
    {
       
      var res =  await _repository.DeletePurchaseOrderAsync(id);

        if (!res.Success) return BadRequest(res);

        return Ok(res);
    }


    [HttpGet("processing")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]

    public async Task<ActionResult<IEnumerable<ProductionOrder>>> GetPendingOrders()
    {
        var productionOrders = await _repository.GetPendingOrdersAsync();
        return Ok(productionOrders);
    }


    //[HttpGet("status/{status}")]
    //public async Task<ActionResult<IEnumerable<ProductionOrder>>> GetByStatus(int status)
    //{
    //    var productionOrders = await _repository.GetByStatusAsync(status.ToString());
    //    return Ok(productionOrders);
    //}

    //[HttpGet("user/{userId}")]
    //public async Task<ActionResult<IEnumerable<ProductionOrder>>> GetByUserId(string userId)
    //{
    //    var productionOrders = await _repository.GetByUserIdAsync(userId);
    //    return Ok(productionOrders);
    //}

}

