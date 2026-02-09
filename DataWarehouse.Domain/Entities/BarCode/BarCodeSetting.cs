using DataWarehouse.Domain.Entities.AllinAll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.BarCode
{
    public class BarCodeSetting
    {
        public int BarCodeSettingId { get; set; }
        public int TotalLength { get; set; }
        public string StartsWith { get; set; }
        public int SapStartPosition { get; set; }
        public int SapLength { get; set; }
        public int QuantityStartPosition { get; set; }
        public int QuantityLength { get; set; }
        public bool IgnoreLastDigit { get; set; }
        public string DefaultUom { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
      
    }
}
