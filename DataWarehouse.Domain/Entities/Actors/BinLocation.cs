using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Actors
{
    public class BinLocation
    {
       
        public  int BinLocationId { get; set; }
        public string Description { get; set; }
        public string Location {  get; set; }

        //b
        public int ItemId { get; set; }
        public Item Item { get; set; }
    }
}
