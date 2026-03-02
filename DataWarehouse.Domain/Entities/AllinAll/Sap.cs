using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Actors.IncrementalSync;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.BarCode;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.AllinAll
{
    public class Sap
    {
      
        public int SapId { get; set; }
        public string SapUrl { get; set; }
        public string CompanyDB { get; set; }
        public string Name {  get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; } = false;
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public ICollection<SapUser> UserSaps { get; set; } = new List<SapUser>();
        public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public ICollection<SapEmployee>  SapEmployees { get; set; } = new List<SapEmployee>();
        public ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
        public ICollection<Item> Items { get; set; } = new List<Item>();
        public ICollection<ItemUomGroup> ItemUomGroups { get; set; } = new List<ItemUomGroup>();
        public ICollection<BarCodeSetting> BarCodeSettings { get; set; } = new List<BarCodeSetting>();
        public ICollection<ItemBarCode> ItemBarCodes { get; set; } = new List<ItemBarCode>();
        public ICollection<SapSyncPagination> SapSyncPaginations { get; set; } = new List<SapSyncPagination>();
        public ICollection<SapSyncStatus> SapSyncStatuses { get; set; } = new List<SapSyncStatus>();
        public ICollection<WmsSyncStatus> WmsSyncStatuses { get; set; } = new List<WmsSyncStatus>();
        public ICollection<DynamicBarCode> DynamicBarCodes { get; set; } = new List<DynamicBarCode>();
        public ICollection<DocumentAttachment> DocumentAttachments { get; set; } = new List<DocumentAttachment>();

        // orders
        public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    }
}
