using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models.DTOs;
using MughtaribatHouse.Models.ViewModels;

namespace MughtaribatHouse.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var totalResidents = await _context.Residents.CountAsync();
            var activeResidents = await _context.Residents.CountAsync(r => r.IsActive);
            var vacantRooms = 50 - activeResidents; // Assuming 50 total rooms

            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var monthlyRevenue = await _context.Payments
                .Where(p => p.ForMonth == currentMonth)
                .SumAsync(p => p.Amount);

            var pendingPayments = await _context.Residents
                .Where(r => r.IsActive)
                .SumAsync(r => r.MonthlyRent) - monthlyRevenue;

            var pendingMaintenance = await _context.MaintenanceTasks
                .CountAsync(m => m.Status == "Pending" || m.Status == "InProgress");

            var recentPayments = await _context.Payments
                .Include(p => p.Resident)
                .OrderByDescending(p => p.PaymentDate)
                .Take(10)
                .ToListAsync();

            var newResidents = await _context.Residents
                .Where(r => r.IsActive && r.CheckInDate >= DateTime.Now.AddMonths(-1))
                .OrderByDescending(r => r.CheckInDate)
                .Take(5)
                .ToListAsync();

            var urgentMaintenance = await _context.MaintenanceTasks
                .Where(m => m.Priority == "High" || m.Priority == "Critical")
                .Where(m => m.Status != "Completed")
                .OrderBy(m => m.Priority)
                .ThenBy(m => m.ReportedDate)
                .Take(5)
                .ToListAsync();

            // Monthly revenue data for charts
            var monthlyRevenueData = await GetMonthlyRevenueDataAsync();

            return new DashboardViewModel
            {
                TotalResidents = totalResidents,
                ActiveResidents = activeResidents,
                VacantRooms = vacantRooms,
                MonthlyRevenue = monthlyRevenue,
                PendingPayments = pendingPayments,
                PendingMaintenance = pendingMaintenance,
                RecentPayments = recentPayments,
                NewResidents = newResidents,
                UrgentMaintenance = urgentMaintenance,
                MonthlyRevenueData = monthlyRevenueData,
                OccupancyRateData = new OccupancyRateData
                {
                    TotalRooms = 50,
                    OccupiedRooms = activeResidents
                }
            };
        }

        public async Task<AnalyticsViewModel> GetAnalyticsDataAsync(DateTime startDate, DateTime endDate)
        {
            var revenue = await GetRevenueAnalyticsAsync(startDate, endDate);
            var residents = await GetResidentAnalyticsAsync(startDate, endDate);
            var maintenance = await GetMaintenanceAnalyticsAsync(startDate, endDate);
            var expenses = await GetExpenseAnalyticsAsync(startDate, endDate);

            return new AnalyticsViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                Revenue = revenue,
                Residents = residents,
                Maintenance = maintenance,
                Expenses = expenses
            };
        }

        public async Task<FinancialSummaryDto> GetFinancialSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var totalRevenue = await _context.Payments
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .SumAsync(p => p.Amount);

            var totalExpenses = await _context.Expenses
                .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
                .SumAsync(e => e.Amount);

            var netProfit = totalRevenue - totalExpenses;
            var profitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0;

            var revenueByCategory = await _context.Payments
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .GroupBy(p => "Rent") // All payments are rent for now
                .Select(g => new CategorySummaryDto
                {
                    Category = g.Key,
                    Amount = g.Sum(p => p.Amount),
                    Percentage = (double)(g.Sum(p => p.Amount) / totalRevenue * 100)
                })
                .ToListAsync();

            var expensesByCategory = await _context.Expenses
                .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
                .GroupBy(e => e.Category)
                .Select(g => new CategorySummaryDto
                {
                    Category = g.Key,
                    Amount = g.Sum(e => e.Amount),
                    Percentage = (double)(g.Sum(e => e.Amount) / totalExpenses * 100)
                })
                .ToListAsync();

            return new FinancialSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalExpenses = totalExpenses,
                NetProfit = netProfit,
          
                RevenueByCategory = revenueByCategory,
                ExpensesByCategory = expensesByCategory
            };
        }

        public async Task<Dictionary<string, decimal>> GetRevenueByCategoryAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .GroupBy(p => "Rent")
                .ToDictionaryAsync(g => g.Key, g => g.Sum(p => p.Amount));
        }

        public async Task<Dictionary<string, int>> GetResidentStatisticsAsync()
        {
            var stats = new Dictionary<string, int>
            {
                ["Total"] = await _context.Residents.CountAsync(),
                ["Active"] = await _context.Residents.CountAsync(r => r.IsActive),
                ["NewThisMonth"] = await _context.Residents
                    .CountAsync(r => r.CheckInDate >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)),
                ["CheckedOutThisMonth"] = await _context.Residents
                    .CountAsync(r => r.CheckOutDate >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1))
            };

            return stats;
        }

        private async Task<MonthlyRevenueData> GetMonthlyRevenueDataAsync()
        {
            var months = new List<string>();
            var revenue = new List<decimal>();

            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                var monthStart = new DateTime(date.Year, date.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var monthlyRevenue = await _context.Payments
                    .Where(p => p.PaymentDate >= monthStart && p.PaymentDate <= monthEnd)
                    .SumAsync(p => p.Amount);

                months.Add(monthStart.ToString("MMM yyyy"));
                revenue.Add(monthlyRevenue);
            }

            return new MonthlyRevenueData
            {
                Months = months,
                Revenue = revenue
            };
        }

        private async Task<RevenueAnalytics> GetRevenueAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var totalRevenue = await _context.Payments
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .SumAsync(p => p.Amount);

            var monthCount = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month + 1;
            var averageMonthlyRevenue = totalRevenue / monthCount;

            var previousPeriodRevenue = await _context.Payments
                .Where(p => p.PaymentDate >= startDate.AddMonths(-monthCount) && p.PaymentDate <= endDate.AddMonths(-monthCount))
                .SumAsync(p => p.Amount);

            var revenueGrowth = previousPeriodRevenue > 0 ?
                ((totalRevenue - previousPeriodRevenue) / previousPeriodRevenue) * 100 : 0;

            var monthlyBreakdown = new List<MonthlyRevenue>();
            var current = startDate;
            while (current <= endDate)
            {
                var monthEnd = new DateTime(current.Year, current.Month, 1).AddMonths(1).AddDays(-1);
                if (monthEnd > endDate) monthEnd = endDate;

                var monthRevenue = await _context.Payments
                    .Where(p => p.PaymentDate >= current && p.PaymentDate <= monthEnd)
                    .SumAsync(p => p.Amount);

                var monthExpenses = await _context.Expenses
                    .Where(e => e.ExpenseDate >= current && e.ExpenseDate <= monthEnd)
                    .SumAsync(e => e.Amount);

                monthlyBreakdown.Add(new MonthlyRevenue
                {
                    Month = current.ToString("MMM yyyy"),
                    Revenue = monthRevenue,
                    Expenses = monthExpenses
                });

                current = current.AddMonths(1);
            }

            return new RevenueAnalytics
            {
                TotalRevenue = totalRevenue,
                AverageMonthlyRevenue = averageMonthlyRevenue,
                RevenueGrowth = (decimal)revenueGrowth,
                MonthlyBreakdown = monthlyBreakdown
            };
        }

        private async Task<ResidentAnalytics> GetResidentAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var totalResidents = await _context.Residents.CountAsync();
            var newResidentsThisMonth = await _context.Residents
                .CountAsync(r => r.CheckInDate >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
            var departuresThisMonth = await _context.Residents
                .CountAsync(r => r.CheckOutDate >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));

            var activeResidents = await _context.Residents.CountAsync(r => r.IsActive);
            var occupancyRate = (activeResidents * 100.0) / 50; // Assuming 50 total rooms

            var averageStay = await _context.Residents
                .Where(r => !r.IsActive && r.CheckOutDate.HasValue)
                .AverageAsync(r => (r.CheckOutDate.Value - r.CheckInDate).TotalDays);

            return new ResidentAnalytics
            {
                TotalResidents = totalResidents,
                NewResidentsThisMonth = newResidentsThisMonth,
                DeparturesThisMonth = departuresThisMonth,
                OccupancyRate = occupancyRate,
                AverageStayDuration = averageStay
            };
        }

        private async Task<MaintenanceAnalytics> GetMaintenanceAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var totalTasks = await _context.MaintenanceTasks
                .CountAsync(m => m.ReportedDate >= startDate && m.ReportedDate <= endDate);

            var completedTasks = await _context.MaintenanceTasks
                .CountAsync(m => m.Status == "Completed" && m.ReportedDate >= startDate && m.ReportedDate <= endDate);

            var pendingTasks = await _context.MaintenanceTasks
                .CountAsync(m => (m.Status == "Pending" || m.Status == "InProgress") && m.ReportedDate >= startDate && m.ReportedDate <= endDate);

            var totalCost = await _context.MaintenanceTasks
                .Where(m => m.Status == "Completed" && m.ReportedDate >= startDate && m.ReportedDate <= endDate)
                .SumAsync(m => m.Cost ?? 0);

            var tasksByPriority = await _context.MaintenanceTasks
                .Where(m => m.ReportedDate >= startDate && m.ReportedDate <= endDate)
                .GroupBy(m => m.Priority)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            return new MaintenanceAnalytics
            {
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                PendingTasks = pendingTasks,
                TotalMaintenanceCost = totalCost,
                TasksByPriority = tasksByPriority
            };
        }

        private async Task<ExpenseAnalytics> GetExpenseAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var totalExpenses = await _context.Expenses
                .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
                .SumAsync(e => e.Amount);

            var monthCount = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month + 1;
            var averageMonthlyExpenses = totalExpenses / monthCount;

            var expensesByCategory = await _context.Expenses
                .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
                .GroupBy(e => e.Category)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(e => e.Amount));

            return new ExpenseAnalytics
            {
                TotalExpenses = totalExpenses,
                ExpensesByCategory = expensesByCategory,
                AverageMonthlyExpenses = averageMonthlyExpenses
            };
        }
    }
}