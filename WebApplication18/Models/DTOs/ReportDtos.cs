namespace MughtaribatHouse.Models.DTOs
{
    public class ReportRequestDto
    {
        public string ReportType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Format { get; set; } // PDF, Excel, CSV
        public List<string> Filters { get; set; }
    }

    public class FinancialReportDto
    {
        public ReportSummaryDto Summary { get; set; }
        public List<RevenueItemDto> Revenue { get; set; }
        public List<ExpenseItemDto> Expenses { get; set; }
        public List<PaymentItemDto> Payments { get; set; }
    }

    public class ResidentReportDto
    {
        public List<ResidentItemDto> Residents { get; set; }
        public OccupancyStatsDto Occupancy { get; set; }
        public List<AdmissionDischargeDto> Admissions { get; set; }
    }

    public class ReportSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome { get; set; }
        public decimal CollectionRate { get; set; }
    }

    public class RevenueItemDto
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class ExpenseItemDto
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class PaymentItemDto
    {
        public string ResidentName { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }
    }

    public class ResidentItemDto
    {
        public string Name { get; set; }
        public DateTime AdmissionDate { get; set; }
        public string Status { get; set; }
        public string Room { get; set; }
        public decimal Balance { get; set; }
    }

    public class OccupancyStatsDto
    {
        public int Capacity { get; set; }
        public int Occupied { get; set; }
        public double Rate { get; set; }
    }

    public class AdmissionDischargeDto
    {
        public string Type { get; set; }
        public string ResidentName { get; set; }
        public DateTime Date { get; set; }
    }
}