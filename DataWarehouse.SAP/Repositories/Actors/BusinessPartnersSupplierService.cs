using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.SAP.Enums;
using DataWarehouse.SAP.Interfaces.Actors;
using DataWarehouse.SAP.Interfaces.Based;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class BusinessPartnersSupplierService : IBusinessPartnersSupplierService
{
    private readonly IBaseSap<BusinessPartnersSapResponse> _sap;
    private readonly ISapSyncStatusRepository _syncRepo;
    private readonly ILogger<BusinessPartnersSupplierService> _logger;
    private readonly DataWarehouseDbContext _context;

    public BusinessPartnersSupplierService(
        IBaseSap<BusinessPartnersSapResponse> sap,
        ISapSyncStatusRepository syncRepo,
        DataWarehouseDbContext context,
        ILogger<BusinessPartnersSupplierService> logger)
    {
        _sap = sap;
        _syncRepo = syncRepo;
        _logger = logger;
        _context = context;
    }

    public async Task<string> SyncBusinessPartnersAsync(int sapId)
    {
        var suppliersList = new List<BusinessPartnerDto>();
        var customersList = new List<BusinessPartnerDto>();

        // Pagination state
        var state = await _syncRepo.GetLastSyncPaginationSkipAsync(sapId, EntitiesName.businessPartners.ToString());
        int skip = state;
        bool hasMore = true;

        int insertedSuppliersTotal = 0;
        int insertedCustomersTotal = 0;

        while (hasMore)
        {
            var url =
                $"BusinessPartners?$skip={skip}" +
                $"&$select=CardCode,CardName,CardType,Address,Phone1,EmailAddress" +
                $"&$orderby=CardCode";

            _logger.LogInformation("Fetching BusinessPartners batch. Url: {url}", url);

            var json = await _sap.GetAllSap(sapId, url);

            BusinessPartnersSapResponse? partnersResponse;
            try
            {
                partnersResponse = JsonSerializer.Deserialize<BusinessPartnersSapResponse>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to deserialize SAP BusinessPartners response", ex);
            }

            var batch = partnersResponse?.Value ?? new List<BusinessPartnerDto>();
            if (!batch.Any())
            {
                hasMore = false;
                break;
            }

            // Split batch
            var batchSuppliers = batch.Where(p => IsSupplier(p.CardType)).ToList();
            var batchCustomers = batch.Where(p => IsCustomer(p.CardType)).ToList();

            suppliersList.AddRange(batchSuppliers);
            customersList.AddRange(batchCustomers);

            // Insert new records (mapping included inside methods)
            var insertedSuppliers = await AddNewSuppliersAsync(sapId, batchSuppliers);
            var insertedCustomers = await AddNewCustomersAsync(sapId, batchCustomers);

            insertedSuppliersTotal += insertedSuppliers;
            insertedCustomersTotal += insertedCustomers;

            // Move skip by total returned from SAP (not by filtered lists)
            skip += batch.Count;

            // Save pagination state once per batch
            await _syncRepo.UpdateLastSyncPaginationSkipAsync(
                sapId,
                EntitiesName.businessPartners.ToString(),
                skip);
        }

        if (!suppliersList.Any() && !customersList.Any())
            return "No business partners to sync";

        return $"Synced BusinessPartners: Suppliers={suppliersList.Count} (Inserted New={insertedSuppliersTotal}), " +
               $"Customers={customersList.Count} (Inserted New={insertedCustomersTotal}).";
    }

    private static bool IsSupplier(string? cardType)
    {
        if (string.IsNullOrWhiteSpace(cardType)) return false;

        // SAP B1 Service Layer غالباً: cSupplier / cCustomer
        // بعض الأنظمة بتبعت: S / C
        return cardType.Equals("cSupplier", StringComparison.OrdinalIgnoreCase)
               || cardType.Equals("S", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCustomer(string? cardType)
    {
        if (string.IsNullOrWhiteSpace(cardType)) return false;

        return cardType.Equals("cCustomer", StringComparison.OrdinalIgnoreCase)
               || cardType.Equals("C", StringComparison.OrdinalIgnoreCase);
    }

    // =========================
    // Suppliers
    // =========================
    private async Task<int> AddNewSuppliersAsync(int sapId, List<BusinessPartnerDto> sapSuppliers)
    {
        if (sapSuppliers == null || sapSuppliers.Count == 0)
            return 0;

        // Existing supplier codes
        var existingCodes = await _context.Suppliers
            .Where(s => s.SapId == sapId)               // لو عندك SapId في Supplier
            .Select(s => s.SupplierCode)
            .ToListAsync();

        var now = DateTime.UtcNow;

        // New only + mapping
        var newSuppliers = sapSuppliers
            .Where(bp => !string.IsNullOrWhiteSpace(bp.CardCode))
            .Where(bp => !existingCodes.Contains(bp.CardCode))
            .Select(bp => new Supplier
            {
                SupplierCode = bp.CardCode,            // CardCode -> SupplierCode
                SupplierName = bp.CardName??"Name",            // CardName -> SupplierName
                Phone = bp.Phone1??"03216446544",                     // Phone1 -> Phone
                Email = bp.EmailAddress??"email@gmail.com",               // EmailAddress -> Email
                Address = bp.Address ?? "email@gmail.com",                  // Address -> Address
                IsActive = true,
                CreatedAt = now,

                // لو عندك SapId فعلاً في Supplier:
                SapId = sapId
            })
            .ToList();

        if (!newSuppliers.Any())
            return 0;

        await _context.Suppliers.AddRangeAsync(newSuppliers);
        await _context.SaveChangesAsync();

        return newSuppliers.Count;
    }

    // =========================
    // Customers
    // =========================
    private async Task<int> AddNewCustomersAsync(int sapId, List<BusinessPartnerDto> sapCustomers)
    {
        if (sapCustomers == null || sapCustomers.Count == 0)
            return 0;

        // ⚠️ Customer model اللي بعتّه مفيهوش CustomerCode
        // فهفلتر بالموجود عندك (هنا استخدمت CustomerName). الأفضل يبقى عندك CustomerCode = CardCode.
        var existingNames = await _context.Customers
            .Where(c => c.SapId == sapId)              // لو عندك SapId في Customer
            .Select(c => c.CustomerName)
            .ToListAsync();

        var now = DateTime.UtcNow;

        var newCustomers = sapCustomers
            .Where(bp => !string.IsNullOrWhiteSpace(bp.CardName))
            .Where(bp => !existingNames.Contains(bp.CardName))
            .Select(bp => new Customer
            {
                CustomerName = bp.CardName ?? "Name",            // CardName -> CustomerName
                Phone = bp.Phone1 ?? "03216446544",                     // Phone1 -> Phone
                Email = bp.EmailAddress ?? "email@gmail.com",               // EmailAddress -> Email
                Address = bp.Address ?? "email@gmail.com",                  // Address -> Address
                IsActive = true,
                CreatedAt = now,

                // لو عندك SapId فعلاً في Customer:
                SapId = sapId
            })
            .ToList();

        if (!newCustomers.Any())
            return 0;

        await _context.Customers.AddRangeAsync(newCustomers);
        await _context.SaveChangesAsync();

        return newCustomers.Count;
    }
}

// =========================
// SAP Response/DTO
// =========================
public class BusinessPartnersSapResponse
{
    public List<BusinessPartnerDto> Value { get; set; } = new();
}

public class BusinessPartnerDto
{
    public string CardCode { get; set; } = "";
    public string CardName { get; set; } = "";
    public string CardType { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone1 { get; set; } = "";
    public string EmailAddress { get; set; } = "";
}