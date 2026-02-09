using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.Interfaces.Company;
using DataWarehouse.Domain.Entities.AllinAll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.Company
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyRepository company;

        public CompanyController(ICompanyRepository company)
        {
            this.company = company;
        }

        [HttpPost("select-company/{companyId}")]
        public async Task<IActionResult> Selectcompany(int companyId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var res = await company.PutCompanyInCache(companyId, userId);

            if (!res.Success)
            {
                return BadRequest("You Can't connect with this company");
            }

            return Ok(new { message = "company selected successfully" });
        }



        [HttpPost]
        public async Task<IActionResult> AddCompany([FromBody] AddCompanyDto dto)
        {
            var result = await company.AddCompanyAuthasync(dto);
            return Ok(result);
        }



        [HttpPut]
        public async Task<IActionResult> Updatecompany([FromBody] UpdateCompanyDto dto)
        {
            var result = await company.UpdateCompanyAuthasync(dto);
            return Ok(result);
        }



        [HttpGet("{skip}/{pageSize}")]
    public async Task<IActionResult> GetAll(int skip,int pageSize)
    {
        var result = await company.GetCompaniesAsync(skip,pageSize);
        return Ok(result);
    }



    [HttpGet("company-setting")]
    public async Task<IActionResult> GetcompanySetting()
    {
        var result = await company.GetCompanySettingAsync();
        return Ok(result);
    }
        [HttpGet("company-selected")]
        public async Task<IActionResult> GetSelectedCompany()
        {
            var result = await company.GetCurruntCompany();
            return Ok(result);
        }



        [HttpDelete("{companyId:int}")]
    public async Task<IActionResult> Delete(int companyId)
    {
        var result = await company.DeleteCompanyAuthasync(companyId);
        return Ok(result);
    }

        [HttpPatch("{companyId:int}")]
        public async Task<IActionResult> ChangeActive(int companyId)
        {
            var result = await company.ChangeActiveCompanyAuthasync(companyId);
            return Ok(result);
        }


    }
}
