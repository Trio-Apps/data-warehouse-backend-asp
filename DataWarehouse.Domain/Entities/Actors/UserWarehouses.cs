using DataWarehouse.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Actors
{
    public class UserWarehouses
    {
        public int UserWarehousesId { get; set; }
        public DateTime UpdateDate { get; set; }

        //b
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }


        //b
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

    }
}
