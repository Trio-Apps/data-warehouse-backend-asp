using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.IGenericDto
{
    public interface IOrderItem
    {
        int OrderId { get; set; }
        int ItemId { get; set; }
        decimal Quantity { get; set; }
        int UoMEntry { get; set; }
        string? BarCode { get; set; }
        GeneralItemStatus Status { get; set; }
        decimal? UnitPrice { get; set; }
        string? ErrorMessage { get; set; }
    }

}
