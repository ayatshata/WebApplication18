using MughtaribatHouse.Models;
using MughtaribatHouse.Models.DTOs;

namespace MughtaribatHouse.Services
{
    public interface IPaymentService
    {
        Task<List<Payment>> GetPaymentsAsync(DateTime? fromDate, DateTime? toDate);
        Task<Payment> GetPaymentAsync(int id);
        Task<Payment> CreatePaymentAsync(CreatePaymentDto dto, string userId);
        Task<bool> DeletePaymentAsync(int id);
        Task<List<Payment>> GetPendingPaymentsAsync();
        Task<decimal> GetTotalRevenueAsync(DateTime? fromDate, DateTime? toDate);
        Task<List<Payment>> GetResidentPaymentHistoryAsync(int residentId);
    }

    public class CreatePaymentDto
    {
        public int ResidentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime ForMonth { get; set; }
        public string PaymentMethod { get; set; }
        public string Notes { get; set; }
    }
}