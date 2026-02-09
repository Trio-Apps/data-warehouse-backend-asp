using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Domain.Entities.BarCode;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataWarehouse.Api.Controllers.admin.BarCode
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarCodeSettingController : ControllerBase
{
    private readonly IBarCodeSettingRepository _barCodeSettingRepository;
    private readonly ILogger<BarCodeController> _logger;

    public BarCodeSettingController(
        IBarCodeSettingRepository barCodeSettingRepository,
        ILogger<BarCodeController> logger)
    {
        _barCodeSettingRepository = barCodeSettingRepository;
        _logger = logger;
    }



    [HttpGet("settings/{skip}/{pageSize}")]
    public async Task<IActionResult> GetAllBarCodeSettings(int skip,int pageSize)
    {
        var settings = await _barCodeSettingRepository.GetBarCodeSettingAsync(skip,pageSize);

        return Ok(settings);
    }

    [HttpGet("settings/{id}")]
    public async Task<ActionResult<BarCodeSetting>> GetBarCodeSettingById(int id)
    {
        var setting = await _barCodeSettingRepository.GetByIdAsync(id);
        if (setting == null)
            return NotFound($"BarCodeSetting with ID {id} not found.");

        return Ok(setting);
    }

  

    [HttpPost("settings")]
    public async Task<ActionResult<BarCodeSetting>> CreateBarCodeSetting([FromBody] AddBarCodeSettingDto barCodeSetting)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _barCodeSettingRepository.AddBarCodeSettingAsync(barCodeSetting);
            if (!res.Success)
            {
                return BadRequest("The Adding is Failed");
            }
            return Ok(res);
    }

        [HttpPut("settings")]
        public async Task<ActionResult<BarCodeSetting>> UpdateBarCodeSetting(UpdateBarCodeSettingDto barCodeSetting)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);



            var existing = await _barCodeSettingRepository.GetByIdAsync(barCodeSetting.BarCodeSettingId);
            if (existing == null)
                return NotFound($"BarCodeSetting with ID {barCodeSetting.BarCodeSettingId} not found.");

            var res = await _barCodeSettingRepository.UpdateBarCodeSettingAsync(barCodeSetting);
            if (!res.Success)
            {
                return BadRequest("The Updating is Failed");
            }
            return Ok(res);
        }

        [HttpDelete("settings/{id}")]
    public async Task<ActionResult> DeleteBarCodeSetting(int id)
    {
        var res = await _barCodeSettingRepository.DeleteBarCodeSettingAsync(id);
        if (!res.Success )
            return NotFound(res.Message);

        return Ok(res);
    }


}


}
