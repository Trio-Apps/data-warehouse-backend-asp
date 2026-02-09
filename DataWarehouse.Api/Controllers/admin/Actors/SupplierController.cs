using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin.Actors;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SupplierController : ControllerBase
{
    private readonly ISupplierRepository _repository;
    private readonly ILogger<SupplierController> _logger;

    public SupplierController(ISupplierRepository repository, ILogger<SupplierController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<GeneralResponse<IEnumerable<Supplier>>>> GetAll()
    {
        var res = await _repository.GetAllSuppliersAsync();
        if (!res.Success)
            return BadRequest(res);
        return Ok(res);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GeneralResponse<Supplier>>> GetById(int id)
    {
        var res = await _repository.GetSupplierByIdAsync(id);
        if (!res.Success)
            return NotFound(res);
        return Ok(res);
    }

    [HttpGet("code/{supplierCode}")]
    public async Task<ActionResult<GeneralResponse<Supplier>>> GetBySupplierCode(string supplierCode)
    {
        var res = await _repository.GetBySupplierCodeAsync(supplierCode);
        if (!res.Success)
            return NotFound(res);
        return Ok(res);
    }

    [HttpGet("name/{supplierName}")]
    public async Task<ActionResult<GeneralResponse<Supplier>>> GetByName(string supplierName)
    {
        var res = await _repository.GetByNameAsync(supplierName);
        if (!res.Success)
            return NotFound(res);
        return Ok(res);
    }

    [HttpGet("active")]
    public async Task<ActionResult<GeneralResponse<IEnumerable<Supplier>>>> GetActiveSuppliers()
    {
        var res = await _repository.GetActiveSuppliersAsync();
        return Ok(res);
    }


    [HttpGet("search/{searchTerm}")]
    public async Task<ActionResult<GeneralResponse<IEnumerable<Supplier>>>> SearchByName(string searchTerm)
    {
        var res = await _repository.SearchByNameAsync(searchTerm);
        return Ok(res);
    }

    [HttpGet("{id}/with-supplier-items")]
    public async Task<ActionResult<GeneralResponse<Supplier>>> GetWithSupplierItems(int id)
    {
        var res = await _repository.GetWithSupplierItemsAsync(id);
        if (!res.Success)
            return NotFound(res);
        return Ok(res);
    }

    [HttpGet("{id}/with-purchase-orders")]
    public async Task<ActionResult<GeneralResponse<Supplier>>> GetWithPurchaseOrders(int id)
    {
        var res = await _repository.GetWithPurchaseOrdersAsync(id);
        if (!res.Success)
            return NotFound(res);
        return Ok(res);
    }

    [HttpPost]
    public async Task<ActionResult<GeneralResponse<Supplier>>> Create([FromBody] SupplierDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(GeneralResponse<Supplier>.FailResponse("Validation failed", errors));
        }

        var res = await _repository.AddSupplierAsync(dto);
        if (!res.Success)
            return Conflict(res);
        return CreatedAtAction(nameof(GetById), new { id = res.Data!.SupplierId }, res);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<GeneralResponse<Supplier>>> Update(int id, [FromBody] SupplierDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(GeneralResponse<Supplier>.FailResponse("Validation failed", errors));
        }

        var res = await _repository.UpdateSupplierAsync(id, dto);
        if (!res.Success)
            return NotFound(res);
        return Ok(res);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<GeneralResponse<bool>>> Delete(int id)
    {
        var res = await _repository.DeleteSupplierAsync(id);
        if (!res.Success)
            return NotFound(res);
        return Ok(res);
    }
}

