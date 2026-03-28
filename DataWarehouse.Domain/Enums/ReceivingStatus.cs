using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Enums
{
    public enum ReceivingStatus
    {
     NoProcessing=1, InTransit = 2,Draft=3, Completed = 4, PartiallyReceived = 5
    }
}
