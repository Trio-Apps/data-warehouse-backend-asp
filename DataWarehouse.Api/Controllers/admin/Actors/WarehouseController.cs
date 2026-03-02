using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Core.IServices.Actors;
using DataWarehouse.Core.IServices.Auth;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Services.Repository.Permissions;
using DataWarehouse.Services.Services.Actors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.Actors;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WarehouseController : ControllerBase
{
    private readonly IRoleServices roleServices;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly ILogger<WarehouseController> _logger;

    public WarehouseController(IRoleServices roleServices,IWarehouseRepository warehouseRepository, ILogger<WarehouseController> logger)
    {
        this.roleServices = roleServices;
        _warehouseRepository = warehouseRepository;
        _logger = logger;
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddWarehouseDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _warehouseRepository.AddWarehouseAsync(dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }
    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Warehouses_Get}")]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var roles = User.Claims
       .Where(c => c.Type == ClaimTypes.Role)
       .Select(c => c.Value)
       .ToList();

        var warehouses = await _warehouseRepository.GetAllWarehouses(userId,roles);
        if (!warehouses.Success)
        {
            return BadRequest(warehouses);
        }

        return Ok(warehouses);
    }
    [HttpGet("sap")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Saps_Get}")]
    public async Task<ActionResult<IEnumerable<Warehouse>>> GetSap()
    {
        var warehouses = await _warehouseRepository.GetSap();

        return Ok(warehouses);
    }
    [HttpGet("sap/{sapId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Saps_Get}")]
    public async Task<ActionResult<IEnumerable<Warehouse>>> GetSapById(int sapId)
    {
        var warehouses = await _warehouseRepository.GetSapByIdAsync(sapId);

        return Ok(warehouses);
    }
   
    [HttpGet("FilterByUpdateDateAndPagination/{updateDate}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Items_Get}")]
    public async Task<ActionResult<PagedResult<Item>>> FilterByUpdateDateAndPagination(
     string updateDate,
     int skip,
     int pageSize)
    {
        // 1??  Õﬁﬁ „‰ «·‹ ModelState ··‹ parameters
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 2?? Õ«Ê·  ÕÊÌ· «·‰’ · «—ÌŒ
        if (!DateTime.TryParseExact(updateDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            return BadRequest("Invalid date format. Expected format: yyyy-MM-dd");
        }

        // 3?? Pagination + filter
        var items = await _warehouseRepository.PaginationAsync(x => x.UpdateDate >= date, skip, pageSize);

        // 4??  Õﬁﬁ ·Ê „›Ì‘ »Ì«‰« 
        if (items == null || !items.Data.Data.Any())
        {
            return NotFound("No items found for the given date.");
        }

        return Ok(items);
    }


    [HttpGet("GetItemsByWarehouseId/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Items_Get}")]
    public async Task<IActionResult> GetItemsByWarehouseId(
    int warehouseId)
    {
        // 1??  Õﬁﬁ „‰ «·‹ ModelState ··‹ parameters
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 3?? Pagination + filter
        var res = await _warehouseRepository.GetItemsOfWarehouseAsync(warehouseId);

        // 4??  Õﬁﬁ ·Ê „›Ì‘ »Ì«‰« 
            if(!res.Success)
            return BadRequest(res);
       

        return Ok(res);
    }

    [HttpGet("GetItemsByWarehouseId/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Items_Get}")]

    public async Task<IActionResult> GetItemsByWarehouseId(
   int warehouseId,
  int skip,
  int pageSize)
    {
        // 1??  Õﬁﬁ „‰ «·‹ ModelState ··‹ parameters
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 3?? Pagination + filter
        var items = await _warehouseRepository.GetItemsOfWarehouseAsync(warehouseId, skip, pageSize);

        // 4??  Õﬁﬁ ·Ê „›Ì‘ »Ì«‰« 
        if (items == null || !items.Data.Data.Any())
        {
            return NotFound("No items found for the given date.");
        }

        return Ok(items);
    }

    [HttpGet("GetItemsByWarehouseIdWithItemCodeAndName/{warehouseId}/items/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Items_Get}")]
    public async Task<IActionResult> GetItemsByWarehouseIdWithItemCodeAndName(
   int warehouseId,
   string? itemCode,
   string? itemName,
    int skip,
  int pageSize)
    {
        // 1??  Õﬁﬁ „‰ «·‹ ModelState ··‹ parameters
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 3?? Pagination + filter
        var items = await _warehouseRepository.GetItemsByWarehouseIdWithItemCodeAndName(warehouseId,itemCode,itemName, skip, pageSize);

        // 4??  Õﬁﬁ ·Ê „›Ì‘ »Ì«‰« 
        if (items == null || !items.Data.Data.Any())
        {
            return NotFound("No items found for the given date.");
        }

        return Ok(items);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Warehouses_Get}")]

    public async Task<ActionResult<Warehouse>> GetById(int id)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        if (warehouse == null)
            return NotFound($"Warehouse with ID {id} not found.");

        return Ok(warehouse);
    }

    [HttpGet("code/{warehouseCode}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Warehouses_Get}")]

    public async Task<ActionResult<Warehouse>> GetByCode(string warehouseCode)
    {
        var warehouse = await _warehouseRepository.GetByNameAsync(warehouseCode);
        if (warehouse == null)
            return NotFound($"Warehouse with code '{warehouseCode}' not found.");

        return Ok(warehouse);
    }

    [HttpGet("{id}/with-user-warehouses")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Warehouses_Get}")]
    public async Task<ActionResult<Warehouse>> GetWithUserWarehouses(int id)
    {
        var warehouse = await _warehouseRepository.GetWithUserWarehousesAsync(id);
        if (warehouse == null)
            return NotFound($"Warehouse with ID {id} not found.");

        return Ok(warehouse);
    }

  
   
}

