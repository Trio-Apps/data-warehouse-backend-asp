using DataWarehouse.Core.DTOs.Dashboard;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Dashboard
{
    public interface IDashboardRepository
    {
        Task<DueTodaySummaryDto> GetDueTodaySummaryAsync(string userId, int? warehouseId = null);
    }
}