using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.DTOs.BarCode
{
    public class BarCodeOrdersDto
    {
        public ICollection<DynamicBarCodeDto> dynamicBarCodeDtos { get; set; }
        public ICollection<StaticBarcodesDto> staticBarcodesDtos { get;set; }
    }

    public class DynamicBarcodesDto
    {
        public string BarCode { get; set; }
    }

    public class StaticBarcodesDto
    {
        public string BarCode { get; set; }
        public decimal Quentity { get; set; }
    }

    public class ItemByBarCodeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string UnitName { get; set; }
        public string Barcode { get; set; }
        public int UoMEntry {  get; set; }
    }

}
