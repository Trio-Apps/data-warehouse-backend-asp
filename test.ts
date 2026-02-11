   public async Task<GeneralResponse<SalesOrderItem>> AddSalesItemBySalesOrderIdAsync(int SalesOrderid, bool isBarcode,
          DynamicBarcodesDto? barcodeDto,
          AddSalesOrderItem? dto)
   {
    
       var model = new SalesOrderItem();
       var entity = await _context.SalesOrders.FirstOrDefaultAsync(e => e.SalesOrderId == SalesOrderid);


       if (entity == null)
           return GeneralResponse<SalesOrderItem>.FailResponse("id is not found");

       var checkApprovalStatus = await GetProcessItem(entity.SalesOrderId, ProcessType.Sales);

       if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
           return GeneralResponse<SalesOrderItem>.FailResponse("You cannot add any item because its approval status is 'Approved' and all approval steps have been completed.");


       if (isBarcode)
       {
           var isDynamic = await CheckDynamicCodeValidationLocal(barcodeDto.BarCode);
           var item = new ItemByBarCodeDto();
           if (isDynamic)
           {
               var resD = await barcodeOrder.GetItemByDynamicBarCodeAsync(entity.WarehouseId, barcodeDto);

               if (!resD.Success)
                   return GeneralResponse<SalesOrderItem>.FailResponse(resD.Message);

               item = resD.Data;

               if (resD.Data == null)
                   return GeneralResponse<SalesOrderItem>.FailResponse(resD.Message);
           }
           else
           {
               var resD = await barcodeOrder.GetItemByStaticBarCodeAsync(entity.WarehouseId, barcodeDto);
               if (!resD.Success)
                   return GeneralResponse<SalesOrderItem>.FailResponse(resD.Message);


               item = resD.Data;
               if (resD.Data == null)
                   return GeneralResponse<SalesOrderItem>.FailResponse(resD.Message);
           }

           model = new SalesOrderItem()
           {
               Status = GeneralItemStatus.Planned,
               SalesOrderId = SalesOrderid,
               ItemId = item.Id,
               Quantity = item.Quantity,
               BarCode = item.Barcode,
               UnitPrice = item.Price,
               UoMEntry = item.UoMEntry
           };
       }
       else
       {
           var item = await _context.Items.FirstOrDefaultAsync(e => e.ItemId == dto.ItemId);
           model = new SalesOrderItem
           {
               Status = GeneralItemStatus.Planned,
               SalesOrderId = dto.SalesOrderId,
               ItemId = dto.ItemId,
               Quantity = dto.Quantity,
               BarCode = "",
               UnitPrice = item.SalesPrice,
               UoMEntry = dto.UoMEntry,
           };
       }

       var res = await AddAsync(model);
       await SaveChangesAsync();

       var modelfin = new SalesOrderItem
       {
           SalesOrderId = res.SalesOrderId,
           Quantity = res.Quantity,
           Status = GetEnumString(res.Status),
           ItemId = res.ItemId,
           SalesOrderItemId = res.SalesOrderItemId,
           UoMEntry = res.UoMEntry,
           BarCode = res.BarCode,
           UnitPrice = res.UnitPrice,
           ErrorMessage = res.ErrorMessage
       };

       return GeneralResponse<SalesOrderItem>.SuccessResponse(modelfin);
   }
