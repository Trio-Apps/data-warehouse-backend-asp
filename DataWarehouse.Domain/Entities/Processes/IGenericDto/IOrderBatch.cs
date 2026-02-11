using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.IGenericDto
{
    public interface IOrderBatch
    {

        public int OrderItemId { get; set; }
        public decimal Quantity { get; set; }
        public string? Comment { get; set; }
        // SAP Goods Receipt Document (DocEntry)
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
