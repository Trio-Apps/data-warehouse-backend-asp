namespace DataWarehouse.Domain.Entities.Actors
{
    public class ItemUomPrice
    {
        public int ItemUomPriceId { get; set; }

        public int ItemPriceId { get; set; }
        public ItemPrice ItemPrice { get; set; }

        public int PriceList { get; set; }
        public int UoMEntry { get; set; }
        public decimal? ReduceBy { get; set; }
        public decimal? Price { get; set; }
        public string? Currency { get; set; }
      
        public bool? Auto { get; set; }
    }
}
