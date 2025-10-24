namespace MughtaribatHouse.Models.ViewModels
{
    public class AnalyticsViewModel
    {
        public DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-6);
        public DateTime EndDate { get; set; } = DateTime.Now;

        public RevenueAnalytics Revenue { get; set; }
        public ResidentAnalytics Residents { get; set; }
        public MaintenanceAnalytics Maintenance { get; set; }
        public ExpenseAnalytics Expenses { get; set; }
    }

    public class RevenueAnalytics
    {
        public decimal TotalRevenue { get; set; }
        public decimal AverageMonthlyRevenue { get; set; }
        public decimal RevenueGrowth { get; set; }
        public List<MonthlyRevenue> MonthlyBreakdown { get; set; }
    }

    public class MonthlyRevenue
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public decimal Profit => Revenue - Expenses;
    }

    public class ResidentAnalytics
    {
        public int TotalResidents { get; set; }
        public int NewResidentsThisMonth { get; set; }
        public int DeparturesThisMonth { get; set; }
        public double OccupancyRate { get; set; }
        public double AverageStayDuration { get; set; }
    }

    public class MaintenanceAnalytics
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
        public Dictionary<string, int> TasksByPriority { get; set; }
    }

    public class ExpenseAnalytics
    {
        public decimal TotalExpenses { get; set; }
        public Dictionary<string, decimal> ExpensesByCategory { get; set; }
        public decimal AverageMonthlyExpenses { get; set; }
    }
}