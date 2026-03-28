using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Core.IServices.Actors;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Services.Repository.Actors;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;




namespace DataWarehouse.Api.Controllers.admin.Actors;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ItemController : ControllerBase
{
    private readonly IItemRepository _itemRespository;
    private readonly ILogger<ItemController> _logger;

    public ItemController(IItemRepository itemRespository, ILogger<ItemController> logger)
    {
        _itemRespository = itemRespository;
        _logger = logger;
    }


    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Items_Get}")]

    public async Task<ActionResult<IEnumerable<Item>>> GetAll()
    {
        var items = await _itemRespository.GetAllAsync();
        return Ok(items);
    }


    [HttpGet("FilterByUpdateDateAndPagination/{updateDate}/{skip}/{top}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Items_Get}")]

    public async Task<ActionResult<PagedResult<Item>>> FilterByUpdateDateAndPagination(
      string updateDate,
      int skip,
      int top){
        // 1?? ���� �� ��� ModelState ��� parameters
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 2?? ���� ����� ���� ������
        if (!DateTime.TryParseExact(updateDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            return BadRequest("Invalid date format. Expected format: yyyy-MM-dd");
        }

        // 3?? Pagination + filter
        var items = await _itemRespository.PaginationAsync(x => x.UpdateDate >= date, skip, top);

        // 4?? ���� �� ���� ������
        if (items == null || !items.Data.Data.Any())
        {
            return NotFound("No items found for the given date.");
        }

        return Ok(items);
    }

    [HttpGet("GetItemsWithItemCodeAndName/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Items_Get}")]

    public async Task<IActionResult> GetItemsByWarehouseIdWithItemCodeAndName(
    string? itemCode,
    string? itemName,
     int skip,
   int pageSize)
    {
        // 1?? ���� �� ��� ModelState ��� parameters
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 3?? Pagination + filter
        var items = await _itemRespository.GetItemsWithItemCodeAndName(itemCode, itemName, skip, pageSize);

        // 4?? ���� �� ���� ������
        if (items == null || !items.Data.Data.Any())
        {
            return NotFound("No items found for the given date.");
        }

        return Ok(items);
    }



    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Items_Get}")]

    public async Task<ActionResult<Item>> GetById(int id)
    {
        var item = await _itemRespository.GetByIdAsync(id);
        if (item == null)
            return NotFound($"Item with ID {id} not found.");

        return Ok(item);
    }

    [HttpGet("code/{itemCode}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Items_Get}")]

    public async Task<ActionResult<Item>> GetByItemCode(string itemCode)
    {
        var item = await _itemRespository.GetByItemCodeAsync(itemCode);
        if (item == null)
            return NotFound($"Item with code '{itemCode}' not found.");

        return Ok(item);
    }

    [HttpGet("group/{itemGroup}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Items_Get}")]

    public async Task<ActionResult<IEnumerable<Item>>> GetByItemGroup(string itemGroup)
    {
        var items = await _itemRespository.GetByItemGroupAsync(itemGroup);
        return Ok(items);
    }

    [HttpGet("{id}/with-bin-locations")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Items_Get}")]

    public async Task<ActionResult<Item>> GetWithBinLocations(int id)
    {
        var item = await _itemRespository.GetWithBinLocationsAsync(id);
        if (item == null)
            return NotFound($"Item with ID {id} not found.");

        return Ok(item);
    }

    [HttpGet("{id}/with-supplier-items")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Items_Get}")]

    public async Task<ActionResult<Item>> GetWithSupplierItems(int id)
    {
        var item = await _itemRespository.GetWithSupplierItemsAsync(id);
        if (item == null)
            return NotFound($"Item with ID {id} not found.");

        return Ok(item);
    }

  
   

    [HttpGet("{id}/prices-with-uoms")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Items_Get}")]
    public async Task<ActionResult<List<ItemPriceWithUomResponse>>> GetItemPricesWithUoms(int id)
    {
        var prices = await _itemRespository.GetItemPricesWithUomsAsync(id);
        return Ok(prices);
    }
}

