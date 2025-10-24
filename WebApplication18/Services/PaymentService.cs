using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public PaymentService(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<List<Payment>> GetPaymentsAsync(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Payments
                .Include(p => p.Resident)
                .Include(p => p.ProcessedByUser)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(p => p.PaymentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.PaymentDate <= toDate.Value);

            return await query
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment> GetPaymentAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Resident)
                .Include(p => p.ProcessedByUser)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Payment> CreatePaymentAsync(CreatePaymentDto dto, string userId)
        {
            var payment = new Payment
            {
                ResidentId = dto.ResidentId,
                Amount = dto.Amount,
                PaymentDate = dto.PaymentDate,
                ForMonth = dto.ForMonth,
                PaymentMethod = dto.PaymentMethod,
                Notes = dto.Notes,
                ProcessedByUserId = userId
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(userId, "Create", "Payment",
                payment.Id.ToString(), null, $"Created payment of {payment.Amount} for resident {payment.ResidentId}");

            return payment;
        }

        public async Task<bool> DeletePaymentAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return false;

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(null, "Delete", "Payment",
                payment.Id.ToString(), null, $"Deleted payment {payment.Id}");

            return true;
        }

        public async Task<List<Payment>> GetPendingPaymentsAsync()
        {
            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var activeResidents = await _context.Residents
                .Where(r => r.IsActive)
                .Select(r => r.Id)
                .ToListAsync();

            var paidResidents = await _context.Payments
                .Where(p => p.ForMonth == currentMonth && activeResidents.Contains(p.ResidentId))
                .Select(p => p.ResidentId)
                .Distinct()
                .ToListAsync();

            var pendingResidents = activeResidents.Except(paidResidents).ToList();

            var pendingPayments = new List<Payment>();
            foreach (var residentId in pendingResidents)
            {
                var resident = await _context.Residents.FindAsync(residentId);
                if (resident != null)
                {
                    pendingPayments.Add(new Payment
                    {
                        ResidentId = residentId,
                        Amount = resident.MonthlyRent,
                        ForMonth = currentMonth,
                        Resident = resident
                    });
                }
            }

            return pendingPayments;
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Payments.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(p => p.PaymentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.PaymentDate <= toDate.Value);

            return await query.SumAsync(p => p.Amount);
        }

        public async Task<List<Payment>> GetResidentPaymentHistoryAsync(int residentId)
        {
            return await _context.Payments
                .Where(p => p.ResidentId == residentId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
    }
}