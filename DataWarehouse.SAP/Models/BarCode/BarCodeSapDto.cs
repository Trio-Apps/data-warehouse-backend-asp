using DataWarehouse.Domain.Entities.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Models.BarCode
{
    public class BarCodeSapDto
    {
        public int UoMEntry { get; set; }
        public string BarCode { get; set; }
        public string FreeText { get; set; }
      
    }
    public class BarCodeFromWmsDtoRequest
    {
        public string ItemCode { get; set; }
        public int ItemBarCodeId { get; set; }
        public ICollection<BarCodeFromWmsDto> ItemBarCodeCollection { get; set; }
    }

    public class BarCodeFromWmsDto
    {
       
        public int UoMEntry { get; set; }
        public string Barcode { get; set; }
        public string FreeText { get; set; }

    }

    public class DeleteBarCodeFromWmsDto
    {

        public int AbsEntry { get; set; }
        public int BarCodeId { get; set; }


    }

    // uom


    public class SapUomGroupDto
    {
        public string ItemCode { get; set; }
        public ICollection<UomDto> Value { get; set; }
    }
    public class UomDto
    {
       
        public float BaseQty { get; set; }

        public string UomCode { get; set; }
        public int UomEntry { get; set; }

    }
}
