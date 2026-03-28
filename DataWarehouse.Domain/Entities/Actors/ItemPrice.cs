using System.Collections.Generic;

namespace DataWarehouse.Domain.Entities.Actors
{
    public class ItemPrice
    {
        public int ItemPriceId { get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; }

        public int PriceList { get; set; }
        public decimal? Price { get; set; }
        public string? Currency { get; set; }
     
        public int? BasePriceList { get; set; }
        public decimal? Factor { get; set; }

        public ICollection<ItemUomPrice> UomPrices { get; set; } = new List<ItemUomPrice>();
    }
}
