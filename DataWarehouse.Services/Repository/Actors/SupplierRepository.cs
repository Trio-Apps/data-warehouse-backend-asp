using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.PurchaseOrders;
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

public class SupplierRepository : BaseRepository<Supplier>, ISupplierRepository
{
    private readonly ICompanyCache companyCache;
    private readonly ISapCache sapCache;

    public SupplierRepository(ICompanyCache companyCache, ISapCache sapCache, DataWarehouseDbContext context) : base(context)
    {
        this.companyCache = companyCache;
        this.sapCache = sapCache;
    }

    public async Task<GeneralResponse<PagedResult<Supplier>>> GetSuppliersAsync(
       string? supplierCode,
       string? supplierName,
       int pageNumber = 1,
       int pageSize = 20)
    {
        // Guard for pagination
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 200 ? 200 : pageSize; // optional cap

        // 1) Resolve sapId (same logic)
        var sapId = await sapCache.Get();

        if (sapId == null)
        {
            var companyId = await companyCache.Get();
            var sap = await _context.Saps.Where(c => c.CompanyId == companyId).FirstOrDefaultAsync();

            if (sap == null)
                return GeneralResponse<PagedResult<Supplier>>.FailResponse("Not Found Any Suppliers In This Company");

            await sapCache.UpdateSapUserClaimAsync(sap.SapId.ToString());
            sapId = sap.SapId;
        }

        // 2) Build query
        IQueryable<Supplier> query = _context.Suppliers
            .AsNoTracking()
            .Where(s => s.SapId == sapId);

        if (!string.IsNullOrWhiteSpace(supplierCode))
        {
            var code = supplierCode.Trim();
            query = query.Where(s => s.SupplierCode != null && s.SupplierCode.Contains(code));
            // exact match alternative:
            // query = query.Where(s => s.SupplierCode == code);
        }

        if (!string.IsNullOrWhiteSpace(supplierName))
        {
            var name = supplierName.Trim();
            query = query.Where(s => s.SupplierName != null && s.SupplierName.Contains(name));
        }

        // 3) Pagination
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.SupplierId) // ⁄œ¯·Â« Õ”» «·‹ key ⁄‰œﬂ
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<Supplier>
        {
            Data = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalCount,
        };

        return GeneralResponse<PagedResult<Supplier>>.SuccessResponse(result, "Suppliers retrieved successfully");
    }

  
    public async Task<GeneralResponse<Supplier>> GetSupplierByIdAsync(int id)
    {
        var supplier = await GetByIdAsync(id);
        if (supplier == null)
            return GeneralResponse<Supplier>.FailResponse($"Supplier with ID {id} not found.");

        return GeneralResponse<Supplier>.SuccessResponse(supplier, "Supplier retrieved successfully");
    }

    public async Task<GeneralResponse<Supplier>> GetBySupplierCodeAsync(string supplierCode)
    {
        var supplier = await Query().FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);
        if (supplier == null)
            return GeneralResponse<Supplier>.FailResponse($"Supplier with code '{supplierCode}' not found.");

        return GeneralResponse<Supplier>.SuccessResponse(supplier, "Supplier retrieved successfully");
    }

    public async Task<GeneralResponse<Supplier>> GetByNameAsync(string supplierName)
    {
        var supplier = await Query().FirstOrDefaultAsync(s => s.SupplierName == supplierName);
        if (supplier == null)
            return GeneralResponse<Supplier>.FailResponse($"Supplier with name '{supplierName}' not found.");

        return GeneralResponse<Supplier>.SuccessResponse(supplier, "Supplier retrieved successfully");
    }

    public async Task<GeneralResponse<IEnumerable<Supplier>>> GetActiveSuppliersAsync()
    {
        var suppliers = await Query().Where(s => s.IsActive).ToListAsync();
        return GeneralResponse<IEnumerable<Supplier>>.SuccessResponse(suppliers, "Active suppliers retrieved successfully");
    }

    public async Task<GeneralResponse<IEnumerable<Supplier>>> SearchByNameAsync(string searchTerm)
    {
        var suppliers = await Query().Where(s => s.SupplierName.Contains(searchTerm)).ToListAsync();
        return GeneralResponse<IEnumerable<Supplier>>.SuccessResponse(suppliers, "Suppliers retrieved successfully");
    }

    public async Task<GeneralResponse<Supplier>> GetWithSupplierItemsAsync(int supplierId)
    {
        var supplier = await QueryIncluding(false, s => s.SupplierItems)
            .FirstOrDefaultAsync(s => s.SupplierId == supplierId);
        
        if (supplier == null)
            return GeneralResponse<Supplier>.FailResponse($"Supplier with ID {supplierId} not found.");

        return GeneralResponse<Supplier>.SuccessResponse(supplier, "Supplier with items retrieved successfully");
    }

    public async Task<GeneralResponse<SupplierDTO>> GetWithPurchaseOrdersAsync(int supplierId)
    {
        var supplierDto = await QueryIncluding(false, s => s.PurchaseOrders)
            .Where(s => s.SupplierId == supplierId)
            .Select(s => new SupplierDTO
            {
                SupplierId = s.SupplierId,
                SupplierCode = s.SupplierCode,
                SupplierName = s.SupplierName,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                IsActive = s.IsActive,

                PurchaseOrders = s.PurchaseOrders.Select(iw => new PurchaseOrderDTO
                {
                    DueDate = iw.DueDate,
                    PostingDate = iw.PostingDate,
                    PurchaseOrderId = iw.PurchaseOrderId,
                    Status = iw.Status.ToString(),
                    // string = enum
                    UserId = iw.UserId,
                    WarehouseId = iw.WarehouseId,

                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (supplierDto == null)
            return GeneralResponse<SupplierDTO>
                .FailResponse($"Supplier with ID {supplierId} not found.");

        return GeneralResponse<SupplierDTO>
            .SuccessResponse(supplierDto, "Supplier with purchase orders retrieved successfully");
    }

    public async Task<GeneralResponse<Supplier>> AddSupplierAsync(SupplierDTO dto)
    {
        if (await ExistsAsync(s => s.SupplierCode == dto.SupplierCode))
            return GeneralResponse<Supplier>.FailResponse($"Supplier with code '{dto.SupplierCode}' already exists.");
        if(await ExistsAsync(e => e.SupplierName == dto.SupplierName))
            return GeneralResponse<Supplier>.FailResponse($"Supplier with Name '{dto.SupplierName}' already exists.");

        var supplier = new Supplier
        {
            SupplierCode = dto.SupplierCode,
            SupplierName = dto.SupplierName,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            IsActive = dto.IsActive
        };

        var created = await AddAsync(supplier);
        await SaveChangesAsync();

        return GeneralResponse<Supplier>.SuccessResponse(created, "Supplier created successfully");
    }

    public async Task<GeneralResponse<Supplier>> UpdateSupplierAsync(int id, SupplierDTO dto)
    {
        var supplier = await GetByIdAsync(id);
        if (supplier == null)
            return GeneralResponse<Supplier>.FailResponse($"Supplier with ID {id} not found.");

        if (supplier.SupplierCode != dto.SupplierCode && await ExistsAsync(s => s.SupplierCode == dto.SupplierCode))
            return GeneralResponse<Supplier>.FailResponse($"Supplier with code '{dto.SupplierCode}' already exists.");

        supplier.SupplierCode = dto.SupplierCode;
        supplier.SupplierName = dto.SupplierName;
        supplier.Phone = dto.Phone;
        supplier.Email = dto.Email;
        supplier.Address = dto.Address;
        supplier.IsActive = dto.IsActive;

        var updated = await UpdateAsync(supplier);
        await SaveChangesAsync();

        return GeneralResponse<Supplier>.SuccessResponse(updated, "Supplier updated successfully");
    }

    public async Task<GeneralResponse<bool>> DeleteSupplierAsync(int id)
    {
        var supplier = await GetByIdAsync(id);
        if (supplier == null)
            return GeneralResponse<bool>.FailResponse($"Supplier with ID {id} not found.");

        await DeleteAsync(id);
        await SaveChangesAsync();

        return GeneralResponse<bool>.SuccessResponse(true, "Supplier deleted successfully");
    }

}
