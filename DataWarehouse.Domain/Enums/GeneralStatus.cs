using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Enums
{
  

  
    public enum GeneralStatus
    {
        Draft = 1, Processing = 2, Approval = 3, Rejected = 4, Completed = 5, PartiallyFailed = 6
    }
    public enum GeneralItemStatus
    {
        Draft = 1, Planned = 2, Released = 3, Received = 4, Closed = 5, Failed = 6
    }
}
