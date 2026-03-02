using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Enums.Approval
{
    public enum ProcessType
    {
        Sales = 1 ,
        SalesReturn = 2 ,
        Purchase = 3,
        Receipt = 4 ,
        GoodsReturn = 5 ,
        Production = 6,
        Transferred = 7,
        Received = 8,
        Counting = 9,
        DeliveryNote=10,


    }
}
