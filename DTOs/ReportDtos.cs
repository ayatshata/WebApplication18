namespace MughtaribatHouse.Models.DTOs
{
    public class ReportRequestDto1
    {
        public string ReportType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Format { get; set; }
        public Dictionary<string, object> Filters { get; set; }
    }

    public class ReportResponseDto
    {
        public string ReportId { get; set; }
        public string ReportName { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string DownloadUrl { get; set; }
        public string Status { get; set; }
    }

    public class FinancialSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public List<CategorySummaryDto> RevenueByCategory { get; set; }
        public List<CategorySummaryDto> ExpensesByCategory { get; set; }
    }

    public class CategorySummaryDto
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
    }
}