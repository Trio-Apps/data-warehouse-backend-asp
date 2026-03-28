namespace DataWarehouse.Core.DTOs.Actors;

public class ItemPriceWithUomResponse
{
    public int ItemPriceId { get; set; }
    public int PriceList { get; set; }
    public decimal? Price { get; set; }
    public string Currency { get; set; }

    public int? UoMEntry { get; set; }
    public string? UomCode { get; set; }
}
