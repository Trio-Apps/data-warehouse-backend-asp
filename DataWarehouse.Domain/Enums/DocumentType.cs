using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Enums
{
    public enum DocumentType
    {
        Purchase = 1,
        Sales = 2,
        Production = 3,
        Inventory = 4,
        GoodsReceipt = 5,
        GoodsIssue = 6
    }
}
