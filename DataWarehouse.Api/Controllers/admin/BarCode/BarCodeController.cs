using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Domain.Entities.BarCode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin.BarCode;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BarCodeController : ControllerBase
{
    private readonly IItemBarCodeRepository _itemBarCodeRepository;
    private readonly IBarCodeSettingRepository _barCodeSettingRepository;
    private readonly ILogger<BarCodeController> _logger;

    public BarCodeController(
        IItemBarCodeRepository itemBarCodeRepository,
        IBarCodeSettingRepository barCodeSettingRepository,
        ILogger<BarCodeController> logger)
    {
        _itemBarCodeRepository = itemBarCodeRepository;
        _barCodeSettingRepository = barCodeSettingRepository;
        _logger = logger; 
    }

    #region ItemBarCode Endpoints

    [HttpGet("item-barcodes")]
    public async Task<IActionResult> GetAllItemBarCodes()
    {
        var itemBarCodes = await _itemBarCodeRepository.GetAllAsync();
        return Ok(itemBarCodes);
    }


    [HttpGet("item-barcode/{id}")]
    public async Task<IActionResult> GetItemBarCodeById(int id)
    {
        var res = await _itemBarCodeRepository.GetBarCodeByBarCodeIdAsync(id);
        if (!res.Success)
            return NotFound(res.Message);

        return Ok(res);
    }

    [HttpGet("item-uom-group/{itemId}")]
    public async Task<IActionResult> GetItemUoMGroups(int itemId)

    {
        var res = await _itemBarCodeRepository.GetItemUomGroupAsync(itemId);
        if (!res.Success)
            return NotFound(res.Message);

        return Ok(res);
    }



    [HttpGet("item-barcodes/by-item/{itemId}/{skip}/{pageSize}")]
    public async Task<IActionResult> GetItemBarCodesByItemId(int itemId,int skip, int pageSize,string? barCode)

    {
        var itemBarCodes = await _itemBarCodeRepository.GetByItemIdAsync(itemId,skip,pageSize, barCode);
        if(!itemBarCodes.Success)
            return NotFound(itemBarCodes.Message);

        return Ok(itemBarCodes);
    }


    [HttpGet("item-barcodes/by-item-id-or-no/{skip}/{pageSize}")]
    public async Task<IActionResult> GetItemBarCodesByItemId(int? itemId, int skip, int pageSize, string? barCode)

    {
        var itemBarCodes = await _itemBarCodeRepository.GetByItemIdOrNoAsync(skip, pageSize, itemId, barCode);
        if (!itemBarCodes.Success)
            return NotFound(itemBarCodes.Message);

        return Ok(itemBarCodes);
    }




    //


    [HttpGet("item-barcodes/{id}/with-item")]
    public async Task<IActionResult> GetItemBarCodeWithItem(int id)
    {
        var res = await _itemBarCodeRepository.GetWithItemAsync(id);
        if (!res.Success)
            return NotFound(res.Message);

        return Ok(res);
    }



    //
    [HttpPost("item-barcodes/{itemId}")]
    public async Task<ActionResult<ItemBarCode>> CreateItemBarCode(int itemId, AddBarCodeDto barCode)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _itemBarCodeRepository.AddBarCodeForItem(itemId, barCode);

        if (!res.Success)
        {
            return BadRequest(res);
        }

        return Ok(res);
    }

    [HttpPut("item-barcodes/{id}")]
    public async Task<IActionResult> UpdateItemBarCode(int id, [FromBody] UpdateBarCodeDto barCode)
    {
        var res = await _itemBarCodeRepository.UpdateBarCodeAsync(id,barCode);
        if(!res.Success)
            return BadRequest(res.Message);
        return Ok(res);
    }


    [HttpDelete("item-barcodes/{id}")]
    public async Task<IActionResult> DeleteItemBarCode(int id)
    {
        
        var res = await _itemBarCodeRepository.DeleteBarCodeAsync(id);
        if (!res.Success)
            return NotFound($"ItemBarCode with ID {id} not found.");

        return Ok(res);
    }

    
    #endregion

}

