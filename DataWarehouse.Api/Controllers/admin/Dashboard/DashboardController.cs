using DataWarehouse.Core.DTOs.Dashboard;
using DataWarehouse.Core.Interfaces.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DataWarehouse.Api.Controllers.admin.Dashboard
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardController(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        /// <summary>
        /// Returns a quick "Due Today" summary for the dashboard.
        /// </summary>
        [HttpGet("due-today")]
        public async Task<ActionResult<DueTodaySummaryDto>> GetDueTodaySummary([FromQuery] int? warehouseId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var summary = await _dashboardRepository.GetDueTodaySummaryAsync(userId, warehouseId);
                return Ok(summary);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
