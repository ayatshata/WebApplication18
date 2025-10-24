using MughtaribatHouse.Models.DTOs;
using MughtaribatHouse.Models.ViewModels;

namespace MughtaribatHouse.Services
{
    public interface IAnalyticsService
    {
        Task<DashboardViewModel> GetDashboardDataAsync();
        Task<AnalyticsViewModel> GetAnalyticsDataAsync(DateTime startDate, DateTime endDate);
        Task<FinancialSummaryDto> GetFinancialSummaryAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, decimal>> GetRevenueByCategoryAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetResidentStatisticsAsync();
    }
}