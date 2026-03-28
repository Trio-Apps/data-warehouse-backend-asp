using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.IGenericDto
{
    public interface IOrder
    {
        int Id { get; }
        int WarehouseId { get; }
        string UserId { get; set; }
       GeneralStatus Status { get; set; }

    }

}
