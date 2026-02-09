using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.BarCode;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.BarCode
{
    public class BarCodeOrdersRepository : IBarCodeOrdersRepository
    {
        private readonly ISapCache sapCache;
        private readonly DataWarehouseDbContext _context;

        public BarCodeOrdersRepository(ISapCache sapCache, DataWarehouseDbContext context)
        {
            this.sapCache = sapCache;
            _context = context;
        }

   
        public async Task<GeneralResponse<ItemByBarCodeDto>> 
            GetItemByStaticBarCodeAsync(int warehouseId, DynamicBarcodesDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.BarCode))
                return GeneralResponse<ItemByBarCodeDto>.FailResponse("send barcode");


            var barcodeValidation = await CheckCodeValidationLocal(dto.BarCode);

            if (!barcodeValidation)
                return GeneralResponse<ItemByBarCodeDto>.FailResponse("this barcode not valid");


            var staticBarCode = await _context.ItemBarCodes
                .Where(ib => ib.BarCode == dto.BarCode && ib.IsActive)
                .Include(ib => ib.Item)
                .ThenInclude(i => i.ItemUomGroups)
                .Include(ib => ib.Item)
                .ThenInclude(i => i.WarehouseItems)
                .FirstOrDefaultAsync();

            if (staticBarCode == null)
                return GeneralResponse<ItemByBarCodeDto>.FailResponse("this barcode is not found");
          
                var item = staticBarCode.Item;

               // var quantity = item.WarehouseItems?.FirstOrDefault(w=> w.WarehouseId == warehouseId).InStock?? 0;

                //if ((decimal)quantity < dto.Quentity)
                //    return GeneralResponse<ItemByBarCodeDto>.FailResponse("This the quantity is not provide");


                return GeneralResponse<ItemByBarCodeDto>.SuccessResponse( new ItemByBarCodeDto
                {
                    Id = staticBarCode.Item.ItemId,
                    Name = staticBarCode.Item.ItemName,
                    Code = staticBarCode.Item.ItemCode,
                    Quantity = 1,
                    Price = staticBarCode.Item.SalesPrice,
                    UnitName = item.ItemUomGroups.FirstOrDefault(i => i.UomEntry == staticBarCode.UoMEntry).UomCode,
                    Barcode = dto.BarCode,
                    UoMEntry = staticBarCode.UoMEntry
                });
        }

        public async Task<GeneralResponse<ItemByBarCodeDto>>
            GetItemByDynamicBarCodeAsync(int warehouseId, DynamicBarcodesDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.BarCode))
                return GeneralResponse<ItemByBarCodeDto>.FailResponse("send barcode");

            var barcodeValidation = await CheckDynamicCodeValidationLocal(dto.BarCode);

            if (!barcodeValidation)
                return GeneralResponse<ItemByBarCodeDto>.FailResponse("this barcode not valid");


            var dynamicBarCode = await _context.DynamicBarCodes
                .Where(db => db.BarCode == dto.BarCode && db.IsActive)
                .Include(db => db.ItemBarCode)
                .ThenInclude(ib => ib.Item).ThenInclude(i=>i.ItemUomGroups)
                
                .FirstOrDefaultAsync();

            if (dynamicBarCode == null)
                return GeneralResponse<ItemByBarCodeDto>.FailResponse("this barcode is not found");

                var item = dynamicBarCode.ItemBarCode.Item;
              // var quantity = item.WarehouseItems?.FirstOrDefault(w => w.WarehouseId == warehouseId).InStock ?? 0;
              
                
                string fullCode = dynamicBarCode.BarCode;          // ABCD123456
                string staticCode = dynamicBarCode.ItemBarCode.BarCode; // ABCD

                int staticLength = staticCode.Length;

                string dynamicPart = fullCode.Substring(staticLength);
                string dynamicPartWithoutLastDigit = dynamicPart.Substring(0, dynamicPart.Length - 1);


                //if ((decimal)quantity < dynamicPart.Length)
                //    return GeneralResponse<ItemByBarCodeDto>.FailResponse("This the quantity is not provide");

               
            return GeneralResponse<ItemByBarCodeDto>.SuccessResponse(new ItemByBarCodeDto
                {
                    Id = item.ItemId,
                    Name = item.ItemName,
                    Code = item.ItemCode,
                    Quantity = staticLength,
                    Price = item.SalesPrice,
                    UnitName = item.ItemUomGroups.FirstOrDefault(i => i.UomEntry == dynamicBarCode.ItemBarCode.UoMEntry).UomCode,
                    Barcode= dto.BarCode,
                    UoMEntry = dynamicBarCode.ItemBarCode.UoMEntry

            });
            }

        private async Task<bool> CheckDynamicCodeValidationLocal(string barCode)
        {
            if (string.IsNullOrWhiteSpace(barCode))
                return false;

            var sapId = await sapCache.Get();

            var settings = await _context.BarCodeSettings
                .Where(bs => bs.Company.Saps.Any(s => s.SapId == sapId))
                .ToListAsync();

            foreach (var setting in settings)
            {
                if (!string.IsNullOrEmpty(setting.StartsWith) &&
                    !barCode.StartsWith(setting.StartsWith))
                    continue;

                if (barCode.Length != setting.TotalLength)
                    continue;

                if (setting.SapLength <= 0 || setting.SapLength > barCode.Length)
                    continue;

                return true;
            }

            return false;
        }

        private async Task<bool> CheckCodeValidationLocal(string barCode)
        {
            if (string.IsNullOrWhiteSpace(barCode))
                return false;
            var sapId = await sapCache.Get();

            var settings = await _context.BarCodeSettings
                .Where(bs => bs.Company.Saps.Any(s => s.SapId == sapId))
                .ToListAsync();
            if(!settings.Any())
                return false;

            return settings.Any(setting =>
                (string.IsNullOrEmpty(setting.StartsWith) || barCode.StartsWith(setting.StartsWith)) &&
                barCode.Length == setting.SapLength
            );
        }
        private async Task<List<BarCodeSetting>> GetBarCodeSettingsAsync(int sapId)
        {
            return await _context.BarCodeSettings
                .Where(bs => bs.Company.Saps.Any(s => s.SapId == sapId))
                .ToListAsync();
        }

        public async Task<ICollection<ItemByBarCodeDto>> GetItemsByBarCodesAsync(BarCodeOrdersDto barCodeOrdersDto)
        {
            var items = new HashSet<int>();
            var itemQuantityMap = new Dictionary<int, decimal>();

            // البحث في Static Barcodes
            if (barCodeOrdersDto.staticBarcodesDtos != null && barCodeOrdersDto.staticBarcodesDtos.Any())
            {
                var staticBarCodes = barCodeOrdersDto.staticBarcodesDtos.Select(s => s.BarCode).ToList();

                var staticItems = await _context.ItemBarCodes
                    .Where(ib => staticBarCodes.Contains(ib.BarCode) && ib.IsActive)
                    .Include(ib => ib.Item)
                    .ToListAsync();

                foreach (var staticItem in staticItems)
                {
                    var matchingStatic = barCodeOrdersDto.staticBarcodesDtos
                        .FirstOrDefault(s => s.BarCode == staticItem.BarCode);

                    if (matchingStatic != null && !items.Contains(staticItem.ItemId))
                    {
                        items.Add(staticItem.ItemId);
                        itemQuantityMap[staticItem.ItemId] = matchingStatic.Quentity;
                    }
                }
            }

            // البحث في Dynamic Barcodes
            if (barCodeOrdersDto.dynamicBarCodeDtos != null && barCodeOrdersDto.dynamicBarCodeDtos.Any())
            {
                var dynamicBarCodes = barCodeOrdersDto.dynamicBarCodeDtos.Select(d => d.BarCode).ToList();

                var dynamicItems = await _context.DynamicBarCodes
                    .Where(db => dynamicBarCodes.Contains(db.BarCode) && db.IsActive)
                    .Include(db => db.ItemBarCode)
                        .ThenInclude(ib => ib.Item)
                    .ToListAsync();

                foreach (var dynamicItem in dynamicItems)
                {
                    if (dynamicItem.ItemBarCode != null && !items.Contains(dynamicItem.ItemBarCode.ItemId))
                    {
                        items.Add(dynamicItem.ItemBarCode.ItemId);
                        // للـ dynamic barcode، الكمية من WarehouseItem
                        if (!itemQuantityMap.ContainsKey(dynamicItem.ItemBarCode.ItemId))
                        {
                            itemQuantityMap[dynamicItem.ItemBarCode.ItemId] = 0;
                        }
                    }
                }
            }

            // الحصول على تفاصيل الـ Items
            if (!items.Any())
                return new List<ItemByBarCodeDto>();

            var itemsList = await _context.Items
                .Where(i => items.Contains(i.ItemId))
                .Include(i => i.WarehouseItems)
                .ToListAsync();

            var result = new List<ItemByBarCodeDto>();

            foreach (var item in itemsList)
            {
                var quantity = itemQuantityMap.ContainsKey(item.ItemId)
                    ? itemQuantityMap[item.ItemId]
                    : (decimal)(item.WarehouseItems?.Sum(wi => wi.InStock) ?? 0);

                result.Add(new ItemByBarCodeDto
                {
                    Id = item.ItemId,
                    Name = item.ItemName,
                    Code = item.ItemCode,
                    Quantity = quantity,
                    Price = item.SalesPrice
                });
            }

            return result;
        }


    }
}
