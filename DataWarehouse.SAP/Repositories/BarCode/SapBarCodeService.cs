using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.BarCode;
using DataWarehouse.SAP.Enums;
using DataWarehouse.SAP.Interfaces.BarCode;
using DataWarehouse.SAP.Interfaces.Based;
using DataWarehouse.SAP.Models.BarCode;
using DataWarehouse.SAP.Repositories.Actors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static DataWarehouse.SAP.Models.Actors.ItemSapModel;
using static DataWarehouse.SAP.Models.Actors.WarehouseSapModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataWarehouse.SAP.Repositories.BarCode
{
    public class SapBarCodeService : ISapBarCodeService
    {
        private readonly IBaseSap<ICollection<BarCodeFromWmsDto>> sap;
        private readonly ILogger<SapBarCodeService> logger;
        private readonly DataWarehouseDbContext _context;

        public SapBarCodeService(IBaseSap<ICollection<BarCodeFromWmsDto>> sap,
            DataWarehouseDbContext context,
            ILogger<SapBarCodeService> logger)
        {
            this.sap = sap;
            this.logger = logger;
            _context = context;
        }

        #region barcode

        public async Task<string> SyncBarCodeAsync(int sapId)
        {
            var barCodes = await GetBarCodeFromWmsAsync(sapId);


            if (!barCodes.Any())
                return "No barcodes to sync";

            int successCount = 0;
            int failCount = 0;

            foreach (var barCode in barCodes)
            {
                var url = $"Items('{barCode.ItemCode}')";

                try
                {

                    await sap.AddPatchSapAsync(sapId, url, barCode.ItemBarCodeCollection);

                    // ✅ Update immediately
                    await BarCodeDone(barCode.ItemBarCodeId);

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

        public async Task<string> SyncDeleteBarCodeAsync(int sapId)
        {
            var absEntries = await GetBarCodeFromWmsIsNotActiveAsync(sapId);


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
                    await BarCodeDelete(abs.BarCodeId);

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



        public async Task<string> SyncItemUomGroupAsync(int sapId)
        {
            var itemCodes = await GetItemCodeWithoutUomGroupAsync(sapId);


            if (!itemCodes.Any())
            {
                logger.LogInformation("Uom is not found: {itemCodes}", itemCodes.Count);
                return "No barcodes to sync";

            }

            int successCount = 0;
            int failCount = 0;
            var uomGroupList = new List<SapUomGroupDto>();


            foreach (var itemCode in itemCodes)
            {
                var url = $"SQLQueries('GetUOM')/List?ItemCode='{itemCode}'";
                var json = await sap.GetAllSap(sapId, url);
                SapUomGroupDto? uomGroup;

                try
                {
                    uomGroup = JsonSerializer.Deserialize<SapUomGroupDto>(json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (uomGroup?.Value != null && uomGroup.Value.Any())
                    {
                        uomGroup.ItemCode = itemCode;
                        uomGroupList.Add(uomGroup); // ضيف العناصر اللي رجع
                                                    //  logger.LogInformation("Fetched {count} items. Total so far: {total}", items.Value.Count, itemList.Count);

                        logger.LogInformation("Url is: {url}", url);

                        logger.LogInformation("Mapping uomGroup: {uomGroup}", JsonSerializer.Serialize(uomGroup.Value, new JsonSerializerOptions { WriteIndented = true }));



                    }


                }
                catch (JsonException ex)
                {
                    throw new Exception("Failed to deserialize SAP items response", ex);
                }

            }

            await UpsertUomGroup(sapId, uomGroupList);

            return $"Sync completed. Success: {successCount}, Failed: {failCount}";
        }


        private async Task<string> BarCodeDone(int id)
        {

            var affectedRows = await _context.ItemBarCodes
                .Where(x => x.ItemBarCodeId == id)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(x => x.SapFlag, true)
                );


            return $"{affectedRows} barcode(s) marked as sent to SAP";
        }



        private async Task<IReadOnlyList<BarCodeFromWmsDtoRequest>> GetBarCodeFromWmsAsync(int sapId)
        {
            return await _context.ItemBarCodes
    .AsNoTracking()
    .Where(x => !x.SapFlag && x.IsActive && x.SapId == sapId)
    .Select(x => new BarCodeFromWmsDtoRequest
    {
        ItemBarCodeId = x.ItemBarCodeId,
        ItemCode = x.Item.ItemCode,

        ItemBarCodeCollection = new List<BarCodeFromWmsDto>
        {
            new BarCodeFromWmsDto
            {
                Barcode = x.BarCode,
                FreeText = x.FreeText,
                UoMEntry = x.UoMEntry
            }
        }
    })
    .ToListAsync();

        }
        private async Task<IReadOnlyList<DeleteBarCodeFromWmsDto>> GetBarCodeFromWmsIsNotActiveAsync(int sapId)
        {
            return await _context.ItemBarCodes
    .AsNoTracking()
    .Where(x => !x.IsActive && x.SapId == sapId)
    .Select(x => new DeleteBarCodeFromWmsDto { BarCodeId = x.ItemBarCodeId, AbsEntry = x.AbsEntry })
    .ToListAsync();

        }
        private async Task<string> BarCodeDelete(int barCodeId)
        {
            var row = await _context.ItemBarCodes.FindAsync(barCodeId);
            _context.ItemBarCodes.Remove(row);

            await _context.SaveChangesAsync();
            return string.Empty;

        }
        // uom helper

        private async Task<IReadOnlyList<string>> GetItemCodeWithoutUomGroupAsync(int sapId)
        {
            return await _context.Items
    .AsNoTracking()
    .Where(item => !item.ItemUomGroups.Any() && item.SapId == sapId)
    .Select(item => item.ItemCode).Distinct()
    .ToListAsync();
        }

        private async Task UpsertUomGroup(int sapId, List<SapUomGroupDto> dto)
        {
            if (dto == null || !dto.Any())
                return;

            // 1️⃣ هات كل ItemCodes اللي جاية من SAP
            var itemCodes = dto.Select(x => x.ItemCode).Distinct().ToList();

            // 2️⃣ هات الـ Items من الداتابيز ومعاهم الـ UomGroups
            var items = await _context.Items
                // .Include(i => i.ItemUomGroups)
                .Where(i => itemCodes.Contains(i.ItemCode) && i.SapId == sapId)
                .ToListAsync();

            foreach (var sapItem in dto)
            {
                var item = items.FirstOrDefault(i => i.ItemCode == sapItem.ItemCode);
                if (item == null)
                    continue;

                foreach (var sapUom in sapItem.Value)
                {
                    // var entity = await _context.ItemUomGroups.Where(i => i.UomEntry == sapUom.UomEntry).FirstOrDefaultAsync();


                    // ➕ Add
                    item.ItemUomGroups.Add(new ItemUomGroup
                    {
                        UomEntry = sapUom.UomEntry,
                        UomCode = sapUom.UomCode,
                        BaseQty = sapUom.BaseQty,
                        ItemId = item.ItemId,
                        SapId = sapId

                    });

                }
            }

            await _context.SaveChangesAsync();
        }

        ///

        #endregion

     
    }
}
