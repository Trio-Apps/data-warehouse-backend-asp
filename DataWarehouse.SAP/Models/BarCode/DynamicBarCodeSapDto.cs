using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Models.BarCode
{

    public class DynamicBarCodeSapDto
    {
        public int UoMEntry { get; set; }
        public string BarCode { get; set; }
    }
    public class DynamicBarCodeFromWmsDtoRequest
    {
        public string ItemCode { get; set; }
        public int ItemBarCodeId { get; set; }
        public int DynamicBarCodeId { get; set; }

        public ICollection<DynamicBarCodeFromWmsDto> ItemDynamicBarCodeCollection { get; set; }
    }

    public class DynamicBarCodeFromWmsDto
    {
        public int UoMEntry { get; set; }
        public string Barcode { get; set; }
        public string FreeText { get; set; }

    }

    public class DeleteDynamicBarCodeFromWmsDto
    {

        public int AbsEntry { get; set; }
        public int BarCodeId { get; set; }


    }

 
}
