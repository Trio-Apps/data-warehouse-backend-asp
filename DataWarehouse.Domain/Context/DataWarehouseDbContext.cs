using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.BarCode;
using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Actors.IncrementalSync;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;

namespace DataWarehouse.Domain.Context;


public class DataWarehouseDbContext : IdentityDbContext<ApplicationUser,ApplicationRole,string>
{
    public DataWarehouseDbContext(DbContextOptions<DataWarehouseDbContext> options) : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder builder)
    {

        #region Processes

        // Purchase Order with items and warehouse
        #region Purchase Order
        builder.Entity<PurchaseOrder>()
    .HasMany(ps => ps.PurchaseOrderItems)
    .WithOne(pi => pi.PurchaseOrder)
    .HasForeignKey(pi => pi.PurchaseOrderId)
    .HasPrincipalKey(ps => ps.PurchaseOrderId)
    .OnDelete(DeleteBehavior.Cascade);


        builder.Entity<Item>()
       .HasMany(i => i.PurchaseOrderItems)
       .WithOne(pi => pi.Item)
       .HasForeignKey(pi => pi.ItemId)
       .HasPrincipalKey(i => i.ItemId)
       .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Warehouse>()
        .HasMany(w => w.PurchaseOrders)
        .WithOne(ps => ps.Warehouse)
        .HasForeignKey(ps => ps.WarehouseId)
        .HasPrincipalKey(w => w.WarehouseId)
        .OnDelete(DeleteBehavior.NoAction);

        //
        builder.Entity<Supplier>()
        .HasMany(s => s.PurchaseOrders)
        .WithOne(ps => ps.Supplier)
        .HasForeignKey(ps => ps.SupplierId)
        .HasPrincipalKey(s => s.SupplierId)
        .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ApplicationUser>()
      .HasMany(w => w.PurchaseOrders)
      .WithOne(po => po.User)
      .HasForeignKey(po => po.UserId)
      .HasPrincipalKey(w => w.Id)
      .OnDelete(DeleteBehavior.NoAction);

        #endregion

        // Receipt Purchase Stock with items and warehouse
        #region Receipt Purchase Order
        builder.Entity<ReceiptPurchaseOrder>()
    .HasMany(rpo => rpo.ReceiptPurchaseOrderItems)
    .WithOne(ri => ri.ReceiptPurchaseOrder)
    .HasForeignKey(ri => ri.ReceiptPurchaseOrderId)
    .HasPrincipalKey(rpo => rpo.ReceiptPurchaseOrderId)
    .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ReceiptPurchaseOrderItem>()
     .HasMany(o => o.ReceiptPurchaseOrderBatches).WithOne(a => a.ReceiptPurchaseOrderItem)
     .HasForeignKey(o => o.ReceiptPurchaseOrderItemId)
     .HasPrincipalKey(e => e.ReceiptPurchaseOrderItemId)
     .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Item>()
         .HasMany(i => i.ReceiptPurchaseOrderItems)
         .WithOne(ri => ri.Item)
         .HasForeignKey(ri => ri.ItemId)
         .HasPrincipalKey(i => i.ItemId)
         .OnDelete(DeleteBehavior.NoAction);


      builder.Entity<ReceiptPurchaseOrderItem>()
      .HasOne(ps => ps.PurchaseOrderItem)
      .WithOne(rpo => rpo.ReceiptPurchaseOrderItem)
      .HasForeignKey<ReceiptPurchaseOrderItem>(rpo => rpo.PurchaseOrderItemId)
      .HasPrincipalKey<PurchaseOrderItem>(ps => ps.PurchaseOrderItemId)
      .OnDelete(DeleteBehavior.NoAction); // أو Cascade حسب اللوجيك



        builder.Entity<Supplier>()
           .HasMany(s => s.ReceiptPurchaseOrders)
           .WithOne(ps => ps.Supplier)
           .HasForeignKey(ps => ps.SupplierId)
           .HasPrincipalKey(s => s.SupplierId)
           .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<PurchaseOrder>()
    .HasOne(ps => ps.ReceiptPurchaseOrder)
    .WithOne(rpo => rpo.PurchaseOrder)
    .HasForeignKey<ReceiptPurchaseOrder>(rpo => rpo.PurchaseOrderId)
    .HasPrincipalKey<PurchaseOrder>(ps => ps.PurchaseOrderId)
    .OnDelete(DeleteBehavior.NoAction); // أو Cascade حسب اللوجيك


        builder.Entity<ApplicationUser>()
        .HasMany(w => w.ReceiptPurchaseOrders)
        .WithOne(po => po.User)
        .HasForeignKey(po => po.UserId)
        .HasPrincipalKey(w => w.Id)
        .OnDelete(DeleteBehavior.NoAction);
        #endregion

        // Good return 
        #region Good Return 
        builder.Entity<ReceiptPurchaseOrder>()
        .HasOne(ps => ps.GoodsReturnOrder)
        .WithOne(rpo => rpo.ReceiptPurchaseOrder)
        .HasForeignKey<GoodsReturnOrder>(rpo => rpo.ReceiptPurchaseOrderId)
        .HasPrincipalKey<ReceiptPurchaseOrder>(ps => ps.ReceiptPurchaseOrderId)
        .OnDelete(DeleteBehavior.NoAction); // أو Cascade حسب اللوجيك

        builder.Entity<ReceiptPurchaseOrderItem>()
       .HasOne(ps => ps.GoodsReturnOrderItem)
       .WithOne(rpo => rpo.ReceiptPurchaseOrderItem)
       .HasForeignKey<GoodsReturnOrderItem>(rpo => rpo.ReceiptPurchaseOrderItemId)
       .HasPrincipalKey<ReceiptPurchaseOrderItem>(ps => ps.ReceiptPurchaseOrderItemId)
       .OnDelete(DeleteBehavior.NoAction); // أو Cascade حسب اللوجيك

        builder.Entity<ReceiptPurchaseOrderBatch>()
     .HasOne(ps => ps.GoodsReturnOrderBatch)
     .WithOne(rpo => rpo.ReceiptPurchaseOrderBatch)
     .HasForeignKey<GoodsReturnOrderBatch>(rpo => rpo.ReceiptPurchaseOrderBatchId)
     .HasPrincipalKey<ReceiptPurchaseOrderBatch>(ps => ps.ReceiptPurchaseOrderBatchId)
     .OnDelete(DeleteBehavior.NoAction); // أو Cascade حسب اللوجيك

        builder.Entity<GoodsReturnOrder>()
        .HasMany(s => s.GoodsReturnOrderItems)
        .WithOne(i => i.GoodsReturnOrder)
        .HasForeignKey(i => i.GoodsReturnOrderId)
        .HasPrincipalKey(s => s.GoodsReturnOrderId)
        .OnDelete(DeleteBehavior.Cascade);


        builder.Entity<GoodsReturnOrderItem>()
        .HasMany(s => s.GoodsReturnOrderBatches)
        .WithOne(i => i.GoodsReturnOrderItem)
        .HasForeignKey(i => i.GoodsReturnOrderItemId)
        .HasPrincipalKey(s => s.GoodsReturnOrderItemId)
        .OnDelete(DeleteBehavior.Cascade);


      
        builder.Entity<ApplicationUser>()
    .HasMany(w => w.GoodsReturnOrders)
    .WithOne(po => po.User)
    .HasForeignKey(po => po.UserId)
    .HasPrincipalKey(w => w.Id)
    .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Supplier>()
          .HasMany(s => s.GoodsReturnOrders)
          .WithOne(ps => ps.Supplier)
          .HasForeignKey(ps => ps.SupplierId)
          .HasPrincipalKey(s => s.SupplierId)
          .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Item>()
     .HasMany(o => o.GoodsReturnOrderItems).WithOne(a => a.Item)
     .HasForeignKey(o => o.ItemId)
     .HasPrincipalKey(e => e.ItemId)
     .OnDelete(DeleteBehavior.NoAction);
        #endregion

        // Sales Order with items and warehouse and customer
        #region Sales Order
        //  Warehouse  with Sales Order
        builder.Entity<Warehouse>()
       .HasMany(o => o.SalesOrders).WithOne(a => a.Warehouse)
       .HasForeignKey(o => o.WarehouseId)
       .HasPrincipalKey(e => e.WarehouseId)
       .OnDelete(DeleteBehavior.NoAction);


        //  Sales order with item

        builder.Entity<SalesOrder>()
     .HasMany(o => o.SalesOrderItems).WithOne(a => a.SalesOrder)
     .HasForeignKey(o => o.SalesOrderId)
     .HasPrincipalKey(e => e.SalesOrderId)
     .OnDelete(DeleteBehavior.Cascade);


        builder.Entity<SalesOrderItem>()
       .HasMany(o => o.SalesOrderBatches).WithOne(a => a.SalesOrderItem)
       .HasForeignKey(o => o.SalesOrderItemId)
       .HasPrincipalKey(e => e.SalesOrderItemId)
       .OnDelete(DeleteBehavior.Cascade);


        builder.Entity<Item>()
     .HasMany(o => o.SalesOrderItems).WithOne(a => a.Item)
     .HasForeignKey(o => o.ItemId)
     .HasPrincipalKey(e => e.ItemId)
     .OnDelete(DeleteBehavior.NoAction);


        // Customer with Sales order

        builder.Entity<Customer>()
     .HasMany(o => o.SalesOrders).WithOne(a => a.Customer)
     .HasForeignKey(o => o.CustomerId)
     .HasPrincipalKey(e => e.CustomerId)
     .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ApplicationUser>()
      .HasMany(w => w.SalesOrders)
      .WithOne(po => po.User)
      .HasForeignKey(po => po.UserId)
      .HasPrincipalKey(w => w.Id)
      .OnDelete(DeleteBehavior.NoAction);

        #endregion

        // Sales return 
        #region Sales Return 

        builder.Entity<SalesOrder>()
        .HasOne(ps => ps.SalesReturnOrder)
        .WithOne(rpo => rpo.SalesOrder)
        .HasForeignKey<SalesReturnOrder>(rpo => rpo.SalesOrderId)
        .HasPrincipalKey<SalesOrder>(ps => ps.SalesOrderId)
        .OnDelete(DeleteBehavior.NoAction); // أو Cascade حسب اللوجيك

        builder.Entity<SalesReturnOrder>()
        .HasMany(s => s.SalesReturnOrderItems)
        .WithOne(i => i.SalesReturnOrder)
        .HasForeignKey(i => i.SalesReturnOrderId)
        .HasPrincipalKey(s => s.SalesReturnOrderId)
        .OnDelete(DeleteBehavior.Cascade);


        builder.Entity<SalesOrderItem>()
       .HasOne(ps => ps.SalesReturnOrderItem)
       .WithOne(rpo => rpo.SalesOrderItem)
       .HasForeignKey<SalesReturnOrderItem>(rpo => rpo.SalesOrderItemId)
       .HasPrincipalKey<SalesOrderItem>(ps => ps.SalesOrderItemId)
       .OnDelete(DeleteBehavior.NoAction); // أو Cascade حسب اللوجيك

        builder.Entity<SalesReturnOrderItem>()
        .HasMany(s => s.SalesReturnOrderBatches)
        .WithOne(i => i.SalesReturnOrderItem)
        .HasForeignKey(i => i.SalesReturnOrderItemId)
        .HasPrincipalKey(s => s.SalesReturnOrderItemId)
        .OnDelete(DeleteBehavior.Cascade);


        builder.Entity<SalesOrderBatch>()
       .HasOne(ps => ps.SalesReturnOrderBatch)
       .WithOne(rpo => rpo.SalesOrderBatch)
       .HasForeignKey<SalesReturnOrderBatch>(rpo => rpo.SalesOrderBatchId)
       .HasPrincipalKey<SalesOrderBatch>(ps => ps.SalesOrderBatchId)
       .OnDelete(DeleteBehavior.NoAction); // أو Cascade حسب اللوجيك


        builder.Entity<ApplicationUser>()
        .HasMany(w => w.SalesReturnOrders)
        .WithOne(po => po.User)
        .HasForeignKey(po => po.UserId)
        .HasPrincipalKey(w => w.Id)
        .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Customer>()
          .HasMany(s => s.SalesReturnOrders)
          .WithOne(ps => ps.Customer)
          .HasForeignKey(ps => ps.CustomerId)
          .HasPrincipalKey(s => s.CustomerId)
          .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Item>()
     .HasMany(o => o.SalesReturnOrderItems)
     .WithOne(a => a.Item)
     .HasForeignKey(o => o.ItemId)
     .HasPrincipalKey(e => e.ItemId)
     .OnDelete(DeleteBehavior.NoAction);
        #endregion

     
        
        // Production Stock with items and warehouse
        #region Production Order


        builder.Entity<Warehouse>()
            .HasMany(w => w.ProductionOrders)
            .WithOne(po => po.Warehouse)
            .HasForeignKey(po => po.WarehouseId)
            .HasPrincipalKey(w => w.WarehouseId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ApplicationUser>()
         .HasMany(w => w.ProductionOrders)
         .WithOne(po => po.User)
         .HasForeignKey(po => po.UserId)
         .HasPrincipalKey(w => w.Id)
         .OnDelete(DeleteBehavior.NoAction);
        // new 

        //builder.Entity<Item>()
        //.HasMany(i => i.FinishedGoodItems)
        //.WithOne(poi => poi.Item)
        //.HasForeignKey(poi => poi.ItemId)
        //.HasPrincipalKey(i => i.ItemId)
        //.OnDelete(DeleteBehavior.NoAction);


        //builder.Entity<Warehouse>()
        //    .HasMany(w => w.FinishedGoodItems)
        //    .WithOne(po => po.Warehouse)
        //    .HasForeignKey(po => po.WarehouseId)
        //    .HasPrincipalKey(w => w.WarehouseId)
        //    .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ProductionOrder>()
          .HasMany(po => po.ProductionOrderItems)
          .WithOne(poi => poi.ProductionOrder)
          .HasForeignKey(poi => poi.ProductionOrderId)
          .HasPrincipalKey(po => po.ProductionOrderId)
          .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Item>()
            .HasMany(i => i.ProductionOrderItems)
            .WithOne(poi => poi.Item)
            .HasForeignKey(poi => poi.ItemId)
            .HasPrincipalKey(i => i.ItemId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ProductionOrderItem>()
           .HasMany(i => i.ProductionReceipts)
           .WithOne(poi => poi.ProductionOrderItem)
           .HasForeignKey(poi => poi.ProductionOrderItemId)
           .HasPrincipalKey(i => i.ProductionOrderItemId)
           .OnDelete(DeleteBehavior.NoAction);


        // indexex
        // في الـ Migration
        builder.Entity<ProductionOrderItem>()
            .HasIndex(x => new { x.Status, x.AbsoluteEntry });
        builder.Entity<WarehouseItem>()
    .HasIndex(x => new { x.WarehouseId, x.FinishedGood, x.HasActiveBOM });

        #endregion

        // Count Stock with items and warehouse
        #region Count Stock

        builder.Entity<CountStock>()
     .HasMany(s => s.CountStockItem)
     .WithOne(i => i.CountStock)
     .HasForeignKey(i => i.CountStockId)
     .HasPrincipalKey(s => s.CountStockId)
     .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CountStockItem>()
     .HasMany(s => s.CountStockBatches)
     .WithOne(i => i.CountStockItem)
     .HasForeignKey(i => i.CountStockItemId)
     .HasPrincipalKey(s => s.CountStockItemId)
     .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Item>()
     .HasMany(i => i.CountStockItems)
     .WithOne(sci => sci.Item)
     .HasForeignKey(sci => sci.ItemId)
     .HasPrincipalKey(i => i.ItemId)
     .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Warehouse>()
      .HasMany(w => w.CountStocks)
      .WithOne(sc => sc.Warehouse)
      .HasForeignKey(sc => sc.WarehouseId)
      .HasPrincipalKey(w => w.WarehouseId)
      .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ApplicationUser>()
      .HasMany(w => w.CountStocks)
      .WithOne(po => po.User)
      .HasForeignKey(po => po.UserId)
      .HasPrincipalKey(w => w.Id)
      .OnDelete(DeleteBehavior.NoAction);
        #endregion

      

        /** Need Modeify **/
        // receive with items and warehouse and transfer stock
        #region Received Stock

        builder.Entity<TransferredStock>()
   .HasOne(ri => ri.ReceivedStock)
   .WithOne(c => c.TransferredStock)
   .HasForeignKey<ReceivedStock>(c => c.TransferredStockId)
   .HasPrincipalKey<TransferredStock>(ri => ri.TransferredStockId)
   .OnDelete(DeleteBehavior.NoAction);


        builder.Entity<ReceivedStock>()
      .HasMany(r => r.ReceivedItems)
  .WithOne(i => i.ReceivedStock)
  .HasForeignKey(i => i.ReceivedStockId)
  .HasPrincipalKey(r => r.ReceivedStockId)
  .OnDelete(DeleteBehavior.Cascade);



        builder.Entity<TransferredItem>()
  .HasOne(ri => ri.ReceivedItem)
  .WithOne(c => c.TransferredItem)
  .HasForeignKey<ReceivedItem>(c => c.TransferredItemId)
  .HasPrincipalKey<TransferredItem>(ri => ri.TransferredItemId)
  .OnDelete(DeleteBehavior.NoAction);


        builder.Entity<TransferredStockBatch>()
  .HasOne(ri => ri.ReceivedStockBatch)
  .WithOne(c => c.TransferredStockBatch)
  .HasForeignKey<ReceivedStockBatch>(c => c.TransferredStockBatchId)
  .HasPrincipalKey<TransferredStockBatch>(ri => ri.TransferredStockBatchId)
  .OnDelete(DeleteBehavior.NoAction);


        builder.Entity<ReceivedItem>()
.HasMany(t => t.ReceivedStockBatches)
.WithOne(i => i.ReceivedItem)
.HasForeignKey(i => i.ReceivedItemId)
.HasPrincipalKey(t => t.ReceivedItemId)
.OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Item>()
     .HasMany(i => i.ReceivedItems)
     .WithOne(ri => ri.Item)
     .HasForeignKey(ri => ri.ItemId)
     .HasPrincipalKey(i => i.ItemId)
     .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Warehouse>()
        .HasMany(w => w.ReceivedStocks)
        .WithOne(rs => rs.Warehouse)
        .HasForeignKey(rs => rs.WarehouseId)
        .HasPrincipalKey(w => w.WarehouseId)
        .OnDelete(DeleteBehavior.NoAction);
        
      
        builder.Entity<Warehouse>()
       .HasMany(w => w.SourceReceivedStocks)
       .WithOne(rs => rs.SourceWarehouse)
       .HasForeignKey(rs => rs.SourceWarehouseId)
       .HasPrincipalKey(w => w.WarehouseId)
       .OnDelete(DeleteBehavior.NoAction);


        builder.Entity<ApplicationUser>()
      .HasMany(w => w.ReceivedStocks)
      .WithOne(po => po.User)
      .HasForeignKey(po => po.UserId)
      .HasPrincipalKey(w => w.Id)
      .OnDelete(DeleteBehavior.NoAction);
        #endregion

        // Transfer With items and warehouse
        #region Transferred Stock
        builder.Entity<TransferredStock>()
    .HasMany(t => t.TransferredItems)
    .WithOne(i => i.TransferredStock)
    .HasForeignKey(i => i.TransferredStockId)
    .HasPrincipalKey(t => t.TransferredStockId)
    .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TransferredItem>()
 .HasMany(t => t.TransferredStockBatches)
 .WithOne(i => i.TransferredItem)
 .HasForeignKey(i => i.TransferredItemId)
 .HasPrincipalKey(t => t.TransferredItemId)
 .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Item>()
        .HasMany(i => i.TransferredItems)
        .WithOne(ti => ti.Item)
        .HasForeignKey(ti => ti.ItemId)
        .HasPrincipalKey(i => i.ItemId)
        .OnDelete(DeleteBehavior.NoAction);


        builder.Entity<Warehouse>()
        .HasMany(i => i.TransferredStocks)
        .WithOne(ti => ti.Warehouse)
        .HasForeignKey(ti => ti.WarehouseId)
        .HasPrincipalKey(i => i.WarehouseId)
        .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Warehouse>()
      .HasMany(w => w.DistinationTransferredStocks)
      .WithOne(rs => rs.DistinationWarehouse)
      .HasForeignKey(rs => rs.DistinationWarehouseId)
      .HasPrincipalKey(w => w.WarehouseId)
      .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<ApplicationUser>()
      .HasMany(w => w.TransferredStocks)
      .WithOne(po => po.User)
      .HasForeignKey(po => po.UserId)
      .HasPrincipalKey(w => w.Id)
      .OnDelete(DeleteBehavior.NoAction);


        #endregion

        builder.Entity<Company>()
     .HasMany(t => t.ProcessesTypes)
   .WithOne(i => i.Company)
  .HasForeignKey(i => i.CompanyId)
   .HasPrincipalKey(t => t.CompanyId)
   .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ProcessesType>()
        .HasMany(t => t.ProcessesTypesDates)
      .WithOne(i => i.ProcessesType)
     .HasForeignKey(i => i.ProcessesTypeId)
      .HasPrincipalKey(t => t.ProcessesTypeId)
      .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProcessesType>()
        .HasIndex(i => i.ProcessesName)
        .HasDatabaseName("IX_ProcessesType_ProcessesName");

        builder.Entity<Sap>()
      .HasMany(t => t.DocumentAttachments)
    .WithOne(i => i.Sap)
   .HasForeignKey(i => i.SapId)
    .HasPrincipalKey(t => t.SapId)
    .OnDelete(DeleteBehavior.Cascade);

        #endregion

        // Shared Tables That be Based in mostly the relationships
        #region based table



        // item with BarCode
        #region Item
        builder.Entity<Item>()
      .HasIndex(i => i.ItemCode)
      .HasDatabaseName("IX_Items_ItemCode");

        builder.Entity<Item>()
            .HasIndex(i => i.ItemName)
            .HasDatabaseName("IX_Items_ItemName");


        builder.Entity<WarehouseItem>()
    .HasIndex(iw => iw.WarehouseId)
    .HasDatabaseName("IX_WarehouseItems_WarehouseId");

        builder.Entity<WarehouseItem>()
            .HasIndex(iw => new { iw.WarehouseId, iw.ItemId })
            .HasDatabaseName("IX_WarehouseItems_WarehouseId_ItemId");


        builder.Entity<ItemBarCode>()
         .HasIndex(x => new { x.SapId, x.BarCode })
                     .HasDatabaseName("IX_ItemBarcode_SapId_Barcode");


        builder.Entity<Item>()
        .HasMany(i => i.ItemBarCodes)
        .WithOne(d => d.Item)
        .HasForeignKey(d => d.ItemId)
        .HasPrincipalKey(i => i.ItemId)
        .OnDelete(DeleteBehavior.Cascade);


        builder.Entity<Item>()
        .HasMany(i => i.ItemUomGroups)
        .WithOne(d => d.Item)
        .HasForeignKey(d => d.ItemId)
        .HasPrincipalKey(i => i.ItemId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ItemBarCode>()
         .HasMany(i => i.DynamicBarCodes)
         .WithOne(d => d.ItemBarCode)
         .HasForeignKey(d => d.ItemBarCodeId)
         .HasPrincipalKey(i => i.ItemBarCodeId)
         .OnDelete(DeleteBehavior.Cascade);



        #endregion


        //  Approval with User Or and Warehouse and BarCode its Tables
        #region Approval with User Or and Warehouse and BarCode its Tables


        builder.Entity<ApplicationUser>()
        .HasMany(o => o.ProcessApprovals).WithOne(a => a.User)
        .HasForeignKey(o => o.UserId)
        .HasPrincipalKey(e => e.Id)
        .OnDelete(DeleteBehavior.NoAction);


        builder.Entity<ApprovalStep>()
    .HasMany(o => o.ProcessApprovals).WithOne(a => a.ApprovalStep)
    .HasForeignKey(o => o.ApprovalStepId)
    .HasPrincipalKey(e => e.ApprovalStepId)
    .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ProcessItemIsProgress>()
   .HasMany(o => o.ProcessApprovals).WithOne(a => a.ProcessItemIsProgress)
   .HasForeignKey(o => o.ProcessItemIsProgressId)
   .HasPrincipalKey(e => e.ProcessItemIsProgressId)
   .OnDelete(DeleteBehavior.NoAction);

      
 
        // Item with BinLocation
        builder.Entity<Item>()
       .HasMany(o => o.BinLocations).WithOne(a => a.Item)
       .HasForeignKey(o => o.ItemId)
       .HasPrincipalKey(e => e.ItemId)
       .OnDelete(DeleteBehavior.NoAction);



        builder.Entity<Company>()
    .HasMany(o => o.ApprovalSteps).WithOne(a => a.Company)
    .HasForeignKey(o => o.CompanyId)
    .HasPrincipalKey(e => e.CompanyId)
    .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Warehouse>()
       .HasMany(o => o.ProcessApprovals).WithOne(a => a.Warehouse)
       .HasForeignKey(o => o.WarehouseId)
       .HasPrincipalKey(e => e.WarehouseId)
       .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ProcessItemIsProgress>()
        .HasIndex(p => new { p.ReferenceId, p.ProcessType, p.Status });


        builder.Entity<ProcessItemIsProgress>()
      .HasIndex(p => p.ProcessType);

        builder.Entity<ProcessItemIsProgress>()
             .HasIndex(p => p.ReferenceId);



        #endregion

        // Supplier  with Item 
        #region Supplier  with Item 
        builder.Entity<Item>()
      .HasMany(o => o.SupplierItems).WithOne(a => a.Item)
      .HasForeignKey(o => o.ItemId)
      .HasPrincipalKey(e => e.ItemId)
      .OnDelete(DeleteBehavior.NoAction);


        builder.Entity<Supplier>()
       .HasMany(o => o.SupplierItems).WithOne(a => a.Supplier)
       .HasForeignKey(o => o.SupplierId)
       .HasPrincipalKey(e => e.SupplierId)
       .OnDelete(DeleteBehavior.NoAction);


        builder.Entity<Sap>()
       .HasMany(o => o.Suppliers).WithOne(a => a.Sap)
       .HasForeignKey(o => o.SapId)
       .HasPrincipalKey(e => e.SapId)
       .OnDelete(DeleteBehavior.NoAction);



        builder.Entity<Item>()
       .HasMany(o => o.SupplierItems).WithOne(a => a.Item)
       .HasForeignKey(o => o.ItemId)
       .HasPrincipalKey(e => e.ItemId)
       .OnDelete(DeleteBehavior.NoAction);


        builder.Entity<Supplier>()
       .HasMany(o => o.SupplierItems).WithOne(a => a.Supplier)
       .HasForeignKey(o => o.SupplierId)
       .HasPrincipalKey(e => e.SupplierId)
       .OnDelete(DeleteBehavior.NoAction);
        #endregion

        // Customer

        #region Customer
        builder.Entity<Sap>()
    .HasMany(o => o.Customers).WithOne(a => a.Sap)
    .HasForeignKey(o => o.SapId)
    .HasPrincipalKey(e => e.SapId)
    .OnDelete(DeleteBehavior.NoAction);

        #endregion

        #region User  with Warehouse and Incremental 
        // User  with Warehouse 

        builder.Entity<ApplicationUser>()
       .HasMany(o => o.UserWarehouses).WithOne(a => a.User)
       .HasForeignKey(o => o.UserId)
       .HasPrincipalKey(e => e.Id)
       .OnDelete(DeleteBehavior.NoAction);


        builder.Entity<Warehouse>()
          .HasMany(o => o.UserWarehouses).WithOne(a => a.Warehouse)
          .HasForeignKey(o => o.WarehouseId)
          .HasPrincipalKey(e => e.WarehouseId)
          .OnDelete(DeleteBehavior.NoAction);


        builder.Entity<ApplicationUser>()
      .HasMany(o => o.SapSyncStatusFronts).WithOne(a => a.User)
      .HasForeignKey(o => o.UserId)
      .HasPrincipalKey(e => e.Id)
      .OnDelete(DeleteBehavior.Cascade);

        #endregion

         #region Item with Warehouse

        builder.Entity<Item>()
.HasMany(i => i.WarehouseItems)
.WithOne(s => s.Item)
.HasForeignKey(s => s.ItemId)
.HasPrincipalKey(i => i.ItemId)
.OnDelete(DeleteBehavior.NoAction);


        builder.Entity<Warehouse>()
.HasMany(i => i.WarehouseItems)
.WithOne(s => s.Warehouse)
.HasForeignKey(s => s.WarehouseId)
.HasPrincipalKey(i => i.WarehouseId)
.OnDelete(DeleteBehavior.NoAction);


        builder.Entity<WarehouseItem>()
      .HasIndex(wi => new { wi.ItemId, wi.WarehouseId })
      .IsUnique();
        builder.Entity<WarehouseItem>()
     .HasIndex(wi => wi.WarehouseId);


        builder.Entity<Item>()
         .HasIndex(x => new { x.SapId, x.ItemCode });




        #endregion

        #region Sap
        builder.Entity<Sap>()
       .HasMany(i => i.Warehouses)
       .WithOne(s => s.Sap)
       .HasForeignKey(s => s.SapId)
       .HasPrincipalKey(i => i.SapId)
       .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<SapUser>()
            .HasOne(us => us.User)
            .WithMany(u => u.UserSaps)
            .HasForeignKey(us => us.UserId)
            .HasPrincipalKey(u => u.Id)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<SapUser>()
            .HasOne(us => us.Sap)
            .WithMany(s => s.UserSaps)
            .HasForeignKey(us => us.SapId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<SapUser>()
            .HasIndex(us => us.UserId);

        builder.Entity<SapUser>()
           .HasIndex(us => us.SapId);
          

        // employee

       

        //builder.Entity<SapEmployee>()
        // .HasOne(ri => ri.User)
        //  .WithOne(c => c.SapEmployee)
        //   .HasForeignKey<SapEmployee>(c => c.UserId)
        //   .OnDelete(DeleteBehavior.Cascade);


        //builder.Entity<SapEmployee>()
        //    .HasOne(us => us.Sap)
        //    .WithMany(s => s.SapEmployees)
        //    .HasForeignKey(us => us.SapId)
        //    .OnDelete(DeleteBehavior.NoAction);

        //builder.Entity<SapEmployee>()
        //    .HasIndex(us => us.UserId)
        //    .IsUnique();
        //builder.Entity<SapEmployee>()
        //   .HasIndex(us => us.SapId);
        /// item
        /// 

        builder.Entity<Sap>()
     .HasMany(i => i.Items)
     .WithOne(s => s.Sap)
      .HasForeignKey(s => s.SapId)
.HasPrincipalKey(i => i.SapId)
.OnDelete(DeleteBehavior.NoAction);
        // UomGroup 
        builder.Entity<Sap>()
   .HasMany(i => i.ItemUomGroups)
   .WithOne(s => s.Sap)
    .HasForeignKey(s => s.SapId)
.HasPrincipalKey(i => i.SapId)
.OnDelete(DeleteBehavior.NoAction);

        // Barcodes
        builder.Entity<Sap>()
   .HasMany(i => i.ItemBarCodes)
   .WithOne(s => s.Sap)
    .HasForeignKey(s => s.SapId)
.HasPrincipalKey(i => i.SapId)
.OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Sap>()
 .HasMany(i => i.DynamicBarCodes)
 .WithOne(s => s.Sap)
  .HasForeignKey(s => s.SapId)
.HasPrincipalKey(i => i.SapId)
.OnDelete(DeleteBehavior.NoAction);

        // Barcode Settings
        builder.Entity<Company>()
   .HasMany(i => i.BarCodeSettings)
   .WithOne(s => s.Company)
    .HasForeignKey(s => s.CompanyId)
.HasPrincipalKey(i => i.CompanyId)
.OnDelete(DeleteBehavior.NoAction);

        #endregion

         #region Company

        builder.Entity<Company>()
        .HasMany(i => i.Saps)
        .WithOne(s => s.Company)
        .HasForeignKey(s => s.CompanyId)
        .HasPrincipalKey(i => i.CompanyId)
        .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Company>()
       .HasMany(i => i.Roles)
       .WithOne(s => s.Company)
       .HasForeignKey(s => s.CompanyId)
       .HasPrincipalKey(i => i.CompanyId)
       .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<CompanyUser>()
           .HasOne(us => us.User)
           .WithOne(u => u.CompanyUser)
           .HasForeignKey<CompanyUser>(us => us.UserId)
           .HasPrincipalKey<ApplicationUser>(au=> au.Id)
           .OnDelete(DeleteBehavior.Cascade);


        builder.Entity<CompanyUser>()
            .HasOne(us => us.Company)
            .WithMany(s => s.CompanyUsers)
            .HasForeignKey(us => us.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<CompanyUser>()
            .HasIndex(us => us.UserId)
            .IsUnique();

        #endregion

        #region Permissions

        builder.Entity<Permission>()
       .HasMany(i => i.RolePermissions)
        .WithOne(s => s.Permission)
       .HasForeignKey(s => s.PermissionId)
       .HasPrincipalKey(i => i.PermissionId)
       .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ApplicationRole>()
       .HasMany(i => i.RolePermissions)
       .WithOne(s => s.Role)
       .HasForeignKey(s => s.RoleId)
       .HasPrincipalKey(i => i.Id)
       .OnDelete(DeleteBehavior.Cascade);


        builder.Entity<RolePermission>()
          .HasKey(rp => new {rp.RoleId,rp.PermissionId});


        builder.Entity<ApplicationRole>(b =>
        {
            // Composite unique per company
            b.HasIndex(r => new { r.CompanyId, r.NormalizedName }).IsUnique();

            // Optional: enforce unique for global roles (CompanyId null)
            b.HasIndex(r => new { r.CompanyId, r.NormalizedName })
             .HasFilter("[CompanyId] IS NOT NULL AND [NormalizedName] IS NOT NULL")
             .IsUnique();

            b.HasIndex(r => r.NormalizedName)
             .HasFilter("[CompanyId] IS NULL AND [NormalizedName] IS NOT NULL")
             .IsUnique();

            // لو EF لسه عامل RoleNameIndex، هنشيله بالـ Migration
        });


        #endregion


        #endregion


        base.OnModelCreating(builder);
    }


    // DbSets

    #region based tables
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<UserWarehouses> UserWarehouses { get; set; }
    public DbSet<WarehouseItem> WarehouseItems { get; set; }

    public DbSet<Item> Items { get; set; }
    public DbSet<BinLocation> BinLocations { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<SupplierItem> SupplierItems { get; set; }
    public DbSet<Customer> Customers { get; set; }
  
    public DbSet<ApprovalStep> ApprovalSteps { get; set; }
    public DbSet<ProcessItemIsProgress> ProcessItemIsProgresses { get; set; }
    public DbSet<ProcessApproval> ProcessApprovals { get; set; }

   

    //

    #endregion

    //BarCode 
    #region Bar Code
    public DbSet<ItemBarCode> ItemBarCodes { get; set; }
    public DbSet<BarCodeSetting> BarCodeSettings { get; set; }
    public DbSet<ItemUomGroup> ItemUomGroups { get; set; }
    public DbSet<DynamicBarCode>  DynamicBarCodes { get; set; }

    #endregion

    // Processes

    #region Processes

    // attachements
    public DbSet<DocumentAttachment> DocumentAttachments { get; set; }

    // Purchase Order
    public DbSet<ProductionOrder> ProductionOrders { get; set; }
    public DbSet<ProductionOrderItem> ProductionOrderItems { get; set; }
   // public DbSet<FinishedGoodItem> FinishedGoodItems { get; set; }
    public DbSet<ProductionReceipt> ProductionReceipts { get; set; }

    // Purchase Order
    public DbSet<ReceiptPurchaseOrder> ReceiptPurchaseOrders { get; set; }
    public DbSet<ReceiptPurchaseOrderItem> ReceiptPurchaseOrderItems { get; set; }
    public DbSet<ReceiptPurchaseOrderBatch> ReceiptPurchaseOrderBatches { get; set; }


    // Purchase Order
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }

    // Count Stocks
    public DbSet<CountStock> CountStocks { get; set; }
    public DbSet<CountStockItem> CountStockItems { get; set; }
    public DbSet<CountStockBatch> CountStockBatches { get; set; }

    // Good Return
    public DbSet<GoodsReturnOrder> GoodsReturnOrders { get; set; }
    public DbSet<GoodsReturnOrderItem>  GoodsReturnOrderItems { get; set; }
    public DbSet<GoodsReturnOrderBatch> GoodsReturnOrderBatches { get; set; }

    // receive
    public DbSet<ReceivedStock> ReceivedStocks { get; set; }
    public DbSet<ReceivedItem> ReceivedItems { get; set; }
    public DbSet<ReceivedStockBatch> ReceivedStockBatches { get; set; }

    // transfer
    public DbSet<TransferredStock> TransferredStocks { get; set; }
    public DbSet<TransferredItem> TransferredItems { get; set; }
    public DbSet<TransferredStockBatch> TransferredStockBatches { get; set; }
    // Sales Order
    public DbSet<SalesOrder> SalesOrders { get; set; }
    public DbSet<SalesOrderItem> SalesOrderItems { get; set; }
    public DbSet<SalesOrderBatch>  SalesOrderBatches { get; set; }

    // Sales Return Order
    public DbSet<SalesReturnOrder>  SalesReturnOrders { get; set; }
    public DbSet<SalesReturnOrderItem>  SalesReturnOrderItems { get; set; }
    public DbSet<SalesReturnOrderBatch> SalesReturnOrderBatches  { get; set; }


    // 

    public DbSet<ProcessesType> ProcessesTypes { get; set; }
    public DbSet<ProcessesTypesDate> ProcessesTypesDates { get; set; }

    #endregion

    #region Permissions
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    #endregion

    #region incremental
    public DbSet<SapSyncStatus> SapSyncStatuses { get; set; }
    public DbSet<SapSyncStatusFront> SapSyncStatusFronts { get; set; }
    public DbSet<SapSyncPagination>  SapSyncPaginations { get; set; }

    public DbSet<WmsSyncStatus> WmsSyncStatuses { get; set; }

    #endregion
    // 

    #region Sap
    public DbSet<Sap> Saps { get; set; }
    public DbSet<SapUser> SapUsers { get; set; }
    //public DbSet<SapEmployee>  SapEmployees { get; set; }

    #endregion

    #region company
    public DbSet<Company> Companies { get; set; }
    public DbSet<CompanyUser> CompanyUsers { get; set; }
    #endregion


}

