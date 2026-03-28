using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Models.Actors
{
    public  class ItemSapModel
    {

        public class ItemSapModelCustom
        {
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public int ItemsGroupCode { get; set; }
            public string PurchaseItem { get; set; }
            public string VatLiable { get; set; }
        }
           

        public class SapItemsResponse
        {
            public List<SapItemDto> Value { get; set; }
        }

        public class SapItemDto
        {
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public int ItemsGroupCode { get; set; }
            public string ProcurementMethod { get; set; }
            public string ManageBatchNumbers { get; set; }
            public DateTime UpdateDate { get; set; }
            public string PurchaseItem { get; set; }
            public string SalesItem { get; set; }
            public string InventoryItem { get; set; }
            public string Valid { get; set; }
            public string Frozen { get; set; }
            public ICollection<SapItemPriceDto> ItemPrices { get; set; }
            public ICollection<SapItemWarehouseDto> ItemWarehouseInfoCollection { get; set; }
            public ICollection<ItemBarCodesDto> ItemBarCodeCollection { get; set; }
            //ItemBarCodeCollection
        }
     

      
        public class SapItemWarehouseDtoResponse
        {
            public string ItemCode { get; set; }
            public ICollection<SapItemWarehouseDto> ItemWarehouseInfoCollection { get; set; }
        }

        public class SapItemWarehouseDto
        {
            public string WarehouseCode { get; set; }
            public decimal? MinimalStock { get; set; }
            public decimal? InStock { get; set; }
        }
        public class SapItemPricesDtoResponse
        {
            public string ItemCode { get; set; } = string.Empty;
            public ICollection<SapItemPriceDto> ItemPrices { get; set; } = new List<SapItemPriceDto>();
        }
        public class SapItemPriceDto
        {
            public int PriceList { get; set; }
            public decimal? Price { get; set; }
            public string? Currency { get; set; }
            
            public int? BasePriceList { get; set; }
            public decimal? Factor { get; set; }
            public List<SapItemUomPriceDto> UoMPrices { get; set; } = new();
        }

        public class SapItemUomPriceDto
        {
            public int PriceList { get; set; }
            public int UoMEntry { get; set; }
            public decimal? ReduceBy { get; set; }
            public decimal? Price { get; set; }
            public string? Currency { get; set; }
            
            public string? Auto { get; set; }
        }

        public class ItemBarCodesDtoResponse
        {
            public string ItemCode { get; set; }
            public ICollection<ItemBarCodesDto> ItemBarCodeCollection { get; set; }
        }
        public class ItemBarCodesDto
        {
            public int UoMEntry {  get; set; }
            public string Barcode { get; set; }
            public int AbsEntry { get; set; }
            public string? FreeText { get; set; }

        }

    }
}
