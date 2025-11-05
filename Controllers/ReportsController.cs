using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MughtaribatHouse.Services;

namespace MughtaribatHouse.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : BaseApiController
    {
        private readonly IExportService _exportService;
        private readonly IAnalyticsService _analyticsService;

        public ReportsController(IExportService exportService, IAnalyticsService analyticsService)
        {
            _exportService = exportService;
            _analyticsService = analyticsService;
        }

        [HttpPost("financial")]
        public async Task<IActionResult> GenerateFinancialReport([FromBody] ReportRequest request)
        {
            try
            {
                var financialData = await _analyticsService.GetFinancialSummaryAsync(request.StartDate, request.EndDate);
                byte[] reportData;

                if (request.Format == "Excel")
                {
                    reportData = await _exportService.ExportPaymentsToExcelAsync(request.StartDate, request.EndDate);
                    return File(reportData,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"financial_report_{DateTime.Now:yyyyMMdd}.xlsx");
                }
                else
                {
                    reportData = await _exportService.ExportFinancialReportToPdfAsync(financialData, request.StartDate, request.EndDate);
                    return File(reportData, "application/pdf", $"financial_report_{DateTime.Now:yyyyMMdd}.pdf");
                }
            }
            catch (Exception ex)
            {
                return Error($"فشل في إنشاء التقرير: {ex.Message}");
            }
        }

        [HttpPost("residents")]
        public async Task<IActionResult> GenerateResidentsReport([FromQuery] string format = "Excel")
        {
            try
            {
                byte[] reportData;

                if (format == "Excel")
                {
                    reportData = await _exportService.ExportResidentsToExcelAsync();
                    return File(reportData,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"residents_report_{DateTime.Now:yyyyMMdd}.xlsx");
                }
                else
                {
                    // For PDF, we would need to create a different export method
                    return Error("تنسيق PDF غير مدعوم لتقرير المقيمين حالياً");
                }
            }
            catch (Exception ex)
            {
                return Error($"فشل في إنشاء التقرير: {ex.Message}");
            }
        }

        [HttpGet("types")]
        public IActionResult GetReportTypes()
        {
            var reportTypes = new[]
            {
                new { Value = "financial", Name = "تقرير مالي" },
                new { Value = "residents", Name = "تقرير المقيمين" },
                new { Value = "occupancy", Name = "تقرير الإشغال" },
                new { Value = "maintenance", Name = "تقرير الصيانة" }
            };

            return Success(reportTypes);
        }
    }

    public class ReportRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Format { get; set; } = "PDF";
        public string ReportType { get; set; }
    }
}