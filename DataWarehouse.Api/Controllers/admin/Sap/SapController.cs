using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Auth;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Domain.Entities.AllinAll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DataWarehouse.Api.Controllers.admin.Sap
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SapController : ControllerBase
    {
        private readonly ISapSettingsRepository sap;

        public SapController(ISapSettingsRepository sap)
        {
            this.sap = sap;
        }

        [HttpPost("select-sap/{sapId}")]
        public async Task<IActionResult> SelectSap(int sapId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var res = await sap.PutSapInCache(sapId, userId);

            if (!res.Success)
            {
                return BadRequest("You Can't connect with this sap");
            }

            return Ok(new { message = "SAP selected successfully" });
        }
        [HttpGet("sap-selected")]
        public async Task<IActionResult> GetSelectedCompany()
        {
            var result = await sap.GetCurruntCompany();
            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> AddSap([FromBody] AddSapDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await sap.AddSapAuthasync(userId,dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

      

        [HttpPut]
        public async Task<IActionResult> UpdateSap([FromBody] UpdateSapDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await sap.UpdateSapAuthasync(userId, dto);
            return Ok(result);
        }

      

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roles = User.Claims
           .Where(c => c.Type == ClaimTypes.Role)
           .Select(c => c.Value)
           .ToList();

            var result = await sap.GetAllSapAsync(userId, roles);
            return Ok(result);
        }
        [HttpGet("{Skip}/{pageSize}")]
        public async Task<IActionResult> GetSaps(int skip,int pageSize,int? companyId,
      string? sapName,
     string? userName)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roles = User.Claims
           .Where(c => c.Type == ClaimTypes.Role)
           .Select(c => c.Value)
           .ToList();
            var result = await sap.GetSapsAsync(userId,roles,skip,pageSize,companyId,userName,sapName);
            return Ok(result);
        }



        [HttpGet("sap-setting")]
        public async Task<IActionResult> GetSapSetting()
        {
            var result = await sap.GetSapSettingAsync();
            return Ok(result);
        }



        [HttpGet("saps-by-company-id/{companyId:int}")]
        public async Task<IActionResult> GetSapsByCompanyId(int companyId)
        {
            var result = await sap.GetSapsByCompanyId(companyId);
            return Ok(result);
        }



        [HttpDelete("{sapId:int}")]
        public async Task<IActionResult> Delete(int sapId)
        {
            var result = await sap.ChangeActiveCompanyAuthasync(sapId);
            return Ok(result);
        }




    }
}
