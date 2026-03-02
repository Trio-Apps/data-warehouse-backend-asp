using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.OutSide
{
    public class SalesOrderBatch : IOrderBatch
    {
        public int SalesOrderBatchId {  get; set; }
        [NotMapped]
        public int OrderItemId
        {
            get => SalesOrderItemId;
            set => SalesOrderItemId = value;
        }
        public decimal Quantity { get; set; }
        public string? Comment { get; set; } = null;
        // SAP Goods Receipt Document (DocEntry)
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
       // public SalesReturnOrderBatch SalesReturnOrderBatch { get; set; }
        public int SalesOrderItemId { get; set; }
        public SalesOrderItem SalesOrderItem { get; set; }

        public ICollection<DeliveryNoteBatch> DeliveryNoteBatches { get; set; } = new List<DeliveryNoteBatch>();
   
    }
}
