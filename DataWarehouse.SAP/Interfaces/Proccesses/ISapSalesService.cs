using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.SAP.Enums;
using DataWarehouse.SAP.Interfaces.Based;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
public interface ISapSalesService
{
    Task<string> SyncSalesOrdersAsync(int sapId, CancellationToken ct = default);
}