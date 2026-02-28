using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.OutSide
{
    public class SalesReturnOrderBatch : IOrderBatch
    {

        public int SalesReturnOrderBatchId { get; set; }    
        public decimal Quantity { get; set; }
        public string? Comment { get; set; } = null;
        // SAP Goods Receipt Document (DocEntry)
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        [NotMapped]
        public int OrderItemId
        {
            get => SalesReturnOrderItemId;
            set => SalesReturnOrderItemId = value;
        }
        public int SalesOrderBatchId { get; set; }

        public SalesOrderBatch SalesOrderBatch { get; set; }

        public int SalesReturnOrderItemId { get; set; }
        public SalesReturnOrderItem SalesReturnOrderItem { get; set; }
    }
}
