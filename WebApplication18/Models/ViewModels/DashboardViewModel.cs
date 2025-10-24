namespace MughtaribatHouse.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalResidents { get; set; }
        public int ActiveResidents { get; set; }
        public int VacantRooms { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal PendingPayments { get; set; }
        public int PendingMaintenance { get; set; }
        public int UnreadNotifications { get; set; }

        public List<Payment> RecentPayments { get; set; }
        public List<Resident> NewResidents { get; set; }
        public List<MaintenanceTask> UrgentMaintenance { get; set; }

        public MonthlyRevenueData MonthlyRevenueData { get; set; }
        public OccupancyRateData OccupancyRateData { get; set; }
    }

    public class MonthlyRevenueData
    {
        public List<string> Months { get; set; }
        public List<decimal> Revenue { get; set; }
    }

    public class OccupancyRateData
    {
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public double OccupancyRate => TotalRooms > 0 ? (OccupiedRooms * 100.0) / TotalRooms : 0;
    }
}