using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.DTOs.BarCode
{
    public class DynamicBarCodeDto
    {
        public int DynamicBarCodeId { get; set; }
        public string BarCode { get; set; }
        public int ItemBarCodeId { get; set; }
        public bool IsActive { get; set; }
    }

    public class AddDynamicBarCodeDto
    {
        public string BarCode { get; set; }
    }

    public class UpdateDynamicBarCodeDto
    {
        public int DynamicBarCodeId { get; set; }
        public string BarCode { get; set; }
    }
}
