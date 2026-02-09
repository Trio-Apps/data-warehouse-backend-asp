using DataWarehouse.Domain.Context;
using DataWarehouse.SAP.Interfaces.BarCode;
using DataWarehouse.SAP.Interfaces.Based;
using DataWarehouse.SAP.Models.BarCode;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Repositories.BarCode
{
    public class SapDynamicBarCodeService : ISapDynamicBarCodeService
    {
        private readonly IBaseSap<ICollection<DynamicBarCodeFromWmsDto>> sap;
        private readonly ILogger<SapBarCodeService> logger;
        private readonly DataWarehouseDbContext _context;

        public SapDynamicBarCodeService(IBaseSap<ICollection<DynamicBarCodeFromWmsDto>> sap,
            DataWarehouseDbContext context,
            ILogger<SapBarCodeService> logger)
        {
            this.sap = sap;
            this.logger = logger;
            _context = context;
        }

        public async Task<string> SyncDynamicBarcodeAsync(int sapId)
        {
            var barCodes = await GetDynamicBarCodeFromWmsAsync(sapId);


            if (!barCodes.Any())
                return "No barcodes to sync";

            int successCount = 0;
            int failCount = 0;

            foreach (var barCode in barCodes)
            {
                var url = $"Items('{barCode.ItemCode}')";

                try
                { 
                    await sap.AddPatchSapAsync(sapId, url, barCode.ItemDynamicBarCodeCollection);

                    // ✅ Update immediately
                    await DynamicBarCodeDone(barCode.DynamicBarCodeId);

                    successCount++;

                    logger.LogInformation(
                        "Barcode synced & updated. Id: {Id}",
                        barCode.ItemBarCodeId
                    );
                }
                catch (Exception ex)
                {
                    failCount++;

                    logger.LogError(
                        ex,
                        "Failed to sync barcode. Id: {Id}",
                        barCode.ItemBarCodeId
                    );
                }
            }

            return $"Sync completed. Success: {successCount}, Failed: {failCount}";
        }

        public async Task<string> SyncDeleteDynamicBarCodeAsync(int sapId)
        {
            var absEntries = await GetDynamicBarCodeFromWmsIsNotActiveAsync(sapId);


            if (!absEntries.Any())
                return "No barcodes to sync";

            int successCount = 0;
            int failCount = 0;

            foreach (var abs in absEntries)
            {
                var url = $"BarCodes({abs.AbsEntry})";

                try
                {
                    await sap.DeleteSap(sapId, url);

                    // ✅ Update immediately
                    await DynamicBarCodeDelete(abs.BarCodeId);

                    successCount++;

                    logger.LogInformation(
                        "Barcode synced & delete. Id: {BarCodeId}",
                        abs.BarCodeId
                    );
                }
                catch (Exception ex)
                {
                    failCount++;

                    logger.LogError(
                        ex,
                        "Failed to sync barcode. Id: {Id}",
                       abs.BarCodeId
                    );
                }
            }

            return $"Sync completed. Success: {successCount}, Failed: {failCount}";
        }
        private async Task<string> DynamicBarCodeDone(int id)
        {

            var affectedRows = await _context.DynamicBarCodes
                .Where(x => x.DynamicBarCodeId == id)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(x => x.SapFlag, true)
                );


            return $"{affectedRows} barcode(s) marked as sent to SAP";
        }

        private async Task<IReadOnlyList<DynamicBarCodeFromWmsDtoRequest>> GetDynamicBarCodeFromWmsAsync(int sapId)
        {
            return await _context.DynamicBarCodes
    .AsNoTracking()
    .Where(x => !x.SapFlag && x.IsActive && x.SapId == sapId)
    .Select(x => new DynamicBarCodeFromWmsDtoRequest
    {
        DynamicBarCodeId = x.DynamicBarCodeId,
        ItemCode = x.ItemBarCode.Item.ItemCode,

        ItemDynamicBarCodeCollection = new List<DynamicBarCodeFromWmsDto>
        {
            new DynamicBarCodeFromWmsDto
            {
                Barcode = x.BarCode,
                 UoMEntry = x.ItemBarCode.UoMEntry,
                 FreeText = x.ItemBarCode.FreeText,
            }
        }
    })
    .ToListAsync();

        }
        private async Task<IReadOnlyList<DeleteDynamicBarCodeFromWmsDto>> GetDynamicBarCodeFromWmsIsNotActiveAsync(int sapId)
        {
            return await _context.DynamicBarCodes
    .AsNoTracking()
    .Where(x => !x.IsActive && x.SapId == sapId)
    .Select(x => new DeleteDynamicBarCodeFromWmsDto { BarCodeId = x.DynamicBarCodeId, AbsEntry = x.AbsEntry })
    .ToListAsync();

        }

        private async Task<string> DynamicBarCodeDelete(int barCodeId)
        {
            var row = await _context.DynamicBarCodes.FindAsync(barCodeId);
            _context.DynamicBarCodes.Remove(row);

            await _context.SaveChangesAsync();
            return string.Empty;

        }

    }
}
