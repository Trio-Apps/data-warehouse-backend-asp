using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Domain.Entities.Actors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin.Actors;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly ICustomerRepository _repository;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ICustomerRepository repository, ILogger<CustomerController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<GeneralResponse<IEnumerable<Customer>>>> GetAll()
    {
        var res = await _repository.GetAllCustomersAsync();
        if (!res.Success)
            return BadRequest(res);
        return Ok(res);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GeneralResponse<Customer>>> GetById(int id)
    {
        var res = await _repository.GetCustomerByIdAsync(id);
        if (!res.Success)
            return NotFound(res);
        return Ok(res);
    }

    [HttpGet("name/{customerName}")]
    public async Task<ActionResult<GeneralResponse<Customer>>> GetByName(string customerName)
    {
        var res = await _repository.GetByNameAsync(customerName);
        if (!res.Success)
            return NotFound(res);
        return Ok(res);
    }

    [HttpGet("active")]
    public async Task<ActionResult<GeneralResponse<IEnumerable<Customer>>>> GetActiveCustomers()
    {
        var res = await _repository.GetActiveCustomersAsync();
        return Ok(res);
    }

    [HttpGet("search/{searchTerm}")]
    public async Task<ActionResult<GeneralResponse<IEnumerable<Customer>>>> SearchByName(string searchTerm)
    {
        var res = await _repository.SearchByNameAsync(searchTerm);
        return Ok(res);
    }

    [HttpGet("{id}/with-sales-orders")]
    public async Task<ActionResult<GeneralResponse<Customer>>> GetWithSalesOrders(int id)
    {
        var res = await _repository.GetWithSalesOrdersAsync(id);
        if (!res.Success)
            return NotFound(res);
        return Ok(res);
    }

    [HttpPost]
    public async Task<ActionResult<GeneralResponse<Customer>>> Create(CustomerDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(GeneralResponse<Customer>.FailResponse("Validation failed", errors));
        }

        var res = await _repository.AddCustomerAsync(dto);
        if (!res.Success)
            return Conflict(res);
        return CreatedAtAction(nameof(GetById), new { id = res.Data!.CustomerId }, res);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<GeneralResponse<Customer>>> Update(int id, [FromBody] CustomerDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(GeneralResponse<Customer>.FailResponse("Validation failed", errors));
        }

        var res = await _repository.UpdateCustomerAsync(id, dto);
        if (!res.Success)
            return NotFound(res);
        return Ok(res);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<GeneralResponse<bool>>> Delete(int id)
    {
        var res = await _repository.DeleteCustomerAsync(id);
        if (!res.Success)
            return NotFound(res);
        return Ok(res);
    }
}

