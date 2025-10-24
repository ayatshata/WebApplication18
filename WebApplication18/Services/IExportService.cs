using MughtaribatHouse.Models.DTOs;
using MughtaribatHouse.Models.ViewModels;

namespace MughtaribatHouse.Services
{
    public interface IExportService
    {
        Task<byte[]> ExportPaymentsToExcelAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> ExportResidentsToExcelAsync();
        Task<byte[]> ExportFinancialReportToPdfAsync(FinancialSummaryDto financialData, DateTime startDate, DateTime endDate);
        Task<byte[]> ExportOccupancyReportToPdfAsync(OccupancyReportResult occupancyData);
    }
}