namespace MughtaribatHouse.Models.ViewModels
{
    public class ReportViewModel
    {
        public string ReportType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Format { get; set; } = "PDF";

        // Filter options
        public string? ResidentId { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
    }

    public class FinancialReportResult
    {
        public DateTime Period { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit => TotalRevenue - TotalExpenses;
        public List<RevenueItem> RevenueItems { get; set; }
        public List<ExpenseItem> ExpenseItems { get; set; }
    }

    public class RevenueItem
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
    }

    public class ExpenseItem
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
    }

    public class OccupancyReportResult
    {
        public DateTime Period { get; set; }
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int VacantRooms => TotalRooms - OccupiedRooms;
        public double OccupancyRate => TotalRooms > 0 ? (OccupiedRooms * 100.0) / TotalRooms : 0;
        public List<RoomStatus> RoomStatuses { get; set; }
    }

    public class RoomStatus
    {
        public string RoomNumber { get; set; }
        public string Status { get; set; }
        public string ResidentName { get; set; }
        public DateTime? CheckInDate { get; set; }
    }
}