using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Domain.Entities.BarCode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin.BarCode
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DynamicBarCodeController : ControllerBase
    {
        private readonly IDynamicBarCodeRepository dynamicBarCodeRepository;
        private readonly IBarCodeSettingRepository _barCodeSettingRepository;
        private readonly ILogger<DynamicBarCodeController> _logger;

        public DynamicBarCodeController(
            IDynamicBarCodeRepository dynamicBarCodeRepository,
            IBarCodeSettingRepository barCodeSettingRepository,
            ILogger<DynamicBarCodeController> logger)
        {
            this.dynamicBarCodeRepository = dynamicBarCodeRepository;
            _barCodeSettingRepository = barCodeSettingRepository;
            _logger = logger;
        }

        #region ItemBarCode Endpoints



        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemBarCodeById(int id)
        {
            var res = await dynamicBarCodeRepository.GetDynamicBarCodeById(id);

            if (!res.Success)
                return NotFound(res.Message);

            return Ok(res);
        }

   

        [HttpGet("dynamic-barcodes/by-barcode/{barcodeId}/{skip}/{pageSize}")]
        public async Task<IActionResult> GetDynamicCodesBybarCodeId(int barcodeId, int skip, int pageSize)
        {
            var itemBarCodes = await dynamicBarCodeRepository.GetDynamicBarCodeByBarCodeId(barcodeId, skip, pageSize);
          
            if (!itemBarCodes.Success)
                return NotFound(itemBarCodes.Message);

            return Ok(itemBarCodes);
        }

        //

        //
        [HttpPost("item-barcode-id/{barcodeId}")]
        public async Task<ActionResult<ItemBarCode>> CreateDynamicBarCode(int barcodeId, [FromBody] AddDynamicBarCodeDto barCode)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var res = await dynamicBarCodeRepository.AddDynamicBarCodeByBarCodeId(barcodeId, barCode);

            if (!res.Success)
            {
                return BadRequest(res);
            }

            return Ok(res);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateDynamicBarCode(UpdateDynamicBarCodeDto barCode)
        {
            var res = await dynamicBarCodeRepository.UpdateDynamicBarCode(barCode);
          
            if (!res.Success)
                return BadRequest(res);


            return Ok(res);
        }


        [HttpDelete("{barcodeId}")]
        public async Task<IActionResult> DeleteItemBarCode(int barcodeId)
        {

            var res = await dynamicBarCodeRepository.DeleteDynamicBarCode(barcodeId);
            if (!res.Success)
                return NotFound($"ItemBarCode with ID {barcodeId} not found.");

            return Ok(res);
        }


        #endregion
    }
}
