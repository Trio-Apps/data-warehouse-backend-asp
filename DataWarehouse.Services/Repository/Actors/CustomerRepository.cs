using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Core.Interfaces.Company;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Services.Repository.Based;
using DataWarehouse.Services.Repository.CompanyRepo;
using DataWarehouse.Services.Repository.SapRepo;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Actors;

public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
{
    private readonly ISapCache sapCache;
    private readonly ICompanyCache companyCache;

    public CustomerRepository(ISapCache sapCache,ICompanyCache companyCache, DataWarehouseDbContext context) : base(context)
    {
        this.sapCache = sapCache;
        this.companyCache = companyCache;
    }

    //public async Task<GeneralResponse<IEnumerable<Customer>>> GetustomersAsync()
    //{

    //    var sapId = await sapCache.Get();

    //    if (sapId == null)
    //    {
    //        var companyId = await companyCache.Get();
    //        var sap = await _context.Saps.Where(c => c.CompanyId == companyId).FirstOrDefaultAsync();
    //        if (sap == null)
    //            return GeneralResponse<IEnumerable<Customer>>.FailResponse("Not Found Any Customers In This Company");

    //        await sapCache.UpdateSapUserClaimAsync(sap.SapId.ToString());
    //        sapId = sap.SapId;
    //    }



    //    var customers = await _context.Customers.Where(s => s.SapId == sapId).ToListAsync();
       
    //    return GeneralResponse<IEnumerable<Customer>>.SuccessResponse(customers, "Customers retrieved successfully");
    //}

    public async Task<GeneralResponse<PagedResult<Customer>>> GetCustomersAsync(
       string? customerCode,
       string? customerName,
       int pageNumber = 1,
       int pageSize = 20)
    {
        // Guard for pagination
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 200 ? 200 : pageSize; // optional cap

        // 1) Resolve sapId (same logic you had)
        var sapId = await sapCache.Get();

        if (sapId == null)
        {
            var companyId = await companyCache.Get();
            var sap = await _context.Saps.Where(c => c.CompanyId == companyId).FirstOrDefaultAsync();

            if (sap == null)
                return GeneralResponse<PagedResult<Customer>>.FailResponse("Not Found Any Customers In This Company");

            await sapCache.UpdateSapUserClaimAsync(sap.SapId.ToString());
            sapId = sap.SapId;
        }

        // 2) Build query
        IQueryable<Customer> query = _context.Customers
            .AsNoTracking()
            .Where(c => c.SapId == sapId);

        if (!string.IsNullOrWhiteSpace(customerCode))
        {
            var code = customerCode.Trim();
            query = query.Where(c => c.CustomerCode != null && c.CustomerCode.Contains(code));
            // ·Ê ⁄«Ì“Â« exact match »œ· contains:
            // query = query.Where(c => c.CustomerCode == code);
        }

        if (!string.IsNullOrWhiteSpace(customerName))
        {
            var name = customerName.Trim();
            query = query.Where(c => c.CustomerName != null && c.CustomerName.Contains(name));
        }

        // 3) Pagination
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CustomerId) // ⁄œ¯·Â« Õ”» «·‹ key ⁄‰œﬂ („À·« CustomerCode)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<Customer>
        {
            Data = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalCount
        };

        return GeneralResponse<PagedResult<Customer>>.SuccessResponse(result, "Customers retrieved successfully");
    }

    // »”Ìÿ… ÊŒ›Ì›… ··‹ pagination response



    public async Task<GeneralResponse<Customer>> GetCustomerByIdAsync(int id)
    {
        var customer = await GetByIdAsync(id);
        if (customer == null)
            return GeneralResponse<Customer>.FailResponse($"Customer with ID {id} not found.");

        return GeneralResponse<Customer>.SuccessResponse(customer, "Customer retrieved successfully");
    }

    public async Task<GeneralResponse<Customer>> GetByNameAsync(string customerName)
    {
        var customer = await Query().FirstOrDefaultAsync(c => c.CustomerName == customerName);
        if (customer == null)
            return GeneralResponse<Customer>.FailResponse($"Customer with name '{customerName}' not found.");

        return GeneralResponse<Customer>.SuccessResponse(customer, "Customer retrieved successfully");
    }

    public async Task<GeneralResponse<IEnumerable<Customer>>> GetActiveCustomersAsync()
    {
        var customers = await Query().Where(c => c.IsActive).ToListAsync();
        return GeneralResponse<IEnumerable<Customer>>.SuccessResponse(customers, "Active customers retrieved successfully");
    }

    public async Task<GeneralResponse<IEnumerable<Customer>>> SearchByNameAsync(string searchTerm)
    {
        var customers = await Query().Where(c => c.CustomerName.Contains(searchTerm)).ToListAsync();
        return GeneralResponse<IEnumerable<Customer>>.SuccessResponse(customers, "Customers retrieved successfully");
    }

    public async Task<GeneralResponse<CustomerDTO>> GetWithSalesOrdersAsync(int customerId)
    {
        var customerDto = await QueryIncluding(false, c => c.SalesOrders)
            .Where(c => c.CustomerId == customerId)
            .Select(c => new CustomerDTO
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                IsActive = c.IsActive,

                SalesOrders = c.SalesOrders.Select(so => new SalesOrderDTO
                {
                    SalesOrderId = so.SalesOrderId,
                    WarehouseId = so.WarehouseId,
                    CustomerId = so.CustomerId,
                    Status = so.Status.ToString(),
                    UserId = so.UserId
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (customerDto == null)
            return GeneralResponse<CustomerDTO>
                .FailResponse($"Customer with ID {customerId} not found.");

        return GeneralResponse<CustomerDTO>
            .SuccessResponse(customerDto, "Customer with sales orders retrieved successfully");
    }


    public async Task<GeneralResponse<Customer>> AddCustomerAsync(CustomerDTO dto)
    {
        if (await ExistsAsync(c => c.CustomerName == dto.CustomerName))
            return GeneralResponse<Customer>.FailResponse($"Customer with name '{dto.CustomerName}' already exists.");

        var customer = new Customer
        {
            CustomerName = dto.CustomerName,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            IsActive = dto.IsActive
        };

        var created = await AddAsync(customer);
        await SaveChangesAsync();

        return GeneralResponse<Customer>.SuccessResponse(created, "Customer created successfully");
    }

    public async Task<GeneralResponse<Customer>> UpdateCustomerAsync(int id, CustomerDTO dto)
    {
        var customer = await GetByIdAsync(id);
        if (customer == null)
            return GeneralResponse<Customer>.FailResponse($"Customer with ID {id} not found.");

        if (customer.CustomerName != dto.CustomerName && await ExistsAsync(c => c.CustomerName == dto.CustomerName))
            return GeneralResponse<Customer>.FailResponse($"Customer with name '{dto.CustomerName}' already exists.");

        customer.CustomerName = dto.CustomerName;
        customer.Phone = dto.Phone;
        customer.Email = dto.Email;
        customer.Address = dto.Address;
        customer.IsActive = dto.IsActive;

        var updated = await UpdateAsync(customer);
        await SaveChangesAsync();

        return GeneralResponse<Customer>.SuccessResponse(updated, "Customer updated successfully");
    }

    public async Task<GeneralResponse<bool>> DeleteCustomerAsync(int id)
    {
        var customer = await GetByIdAsync(id);
        if (customer == null)
            return GeneralResponse<bool>.FailResponse($"Customer with ID {id} not found.");

        await DeleteAsync(id);
        await SaveChangesAsync();

        return GeneralResponse<bool>.SuccessResponse(true, "Customer deleted successfully");
    }

   
}
