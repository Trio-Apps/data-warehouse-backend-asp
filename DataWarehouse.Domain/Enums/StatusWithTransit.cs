using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Enums
{
    public enum StatusWithTransit
    {
        // Draft / InTransit / Completed / approval
        Draft = 1, InTransit = 2, Completed = 3, approval = 4

    }
}
