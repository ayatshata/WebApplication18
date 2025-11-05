using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MughtaribatHouse.Services;

namespace MughtaribatHouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : BaseApiController
    {
        private readonly IAnalyticsService _analyticsService;

        public DashboardController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            var data = await _analyticsService.GetDashboardDataAsync();
            return Success(data);
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            startDate ??= DateTime.Now.AddMonths(-6);
            endDate ??= DateTime.Now;

            var analytics = await _analyticsService.GetAnalyticsDataAsync(startDate.Value, endDate.Value);
            return Success(analytics);
        }

        [HttpGet("financial-summary")]
        public async Task<IActionResult> GetFinancialSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            startDate ??= DateTime.Now.AddMonths(-6);
            endDate ??= DateTime.Now;

            var summary = await _analyticsService.GetFinancialSummaryAsync(startDate.Value, endDate.Value);
            return Success(summary);
        }
    }
}