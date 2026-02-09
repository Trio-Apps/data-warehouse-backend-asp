using DataWarehouse.Domain.Entities.Actors;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.DTOs.BarCode
{
    public class BarCodeDto
    {
        public int ItemBarCodeId { get; set; }
        public string BarCode { get; set; }
        public int UoMEntry { get; set; }
        public string UnitName { get; set; }
        public string FreeText { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }

       // public string BarCodeType { get; set; }
    }

    

    public class AddBarCodeDto
    {
        public string BarCode { get; set; }
        public int UoMEntry { get; set; }
        public string FreeText { get; set; }
      //  public string BarCodeType { get; set; }

        //  public int ItemId { get; set; }   // FK → Item
    }

    public class UpdateBarCodeDto
    {
        public string? BarCode { get; set; }
        public int? UoMEntry { get; set; }
        public string? FreeText { get; set; }
      //  public string BarCodeType { get; set; }

        //  public int ItemId { get; set; }   // FK → Item
    }


    public class ItemUomGroupDto
    {

        public float BaseQty { get; set; }
        public string UomCode { get; set; }
        public int UomEntry { get; set; }

    }
}
