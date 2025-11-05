using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;
using MughtaribatHouse.Models.DTOs;

namespace MughtaribatHouse.Services
{
    public class ResidentService : IResidentService
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public ResidentService(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<List<Resident>> GetResidentsAsync()
        {
            return await _context.Residents
                .Include(r => r.Payments)
                .Include(r => r.ManagedByUser)
                .Where(r => r.IsActive)
                .OrderBy(r => r.FullName)
                .ToListAsync();
        }

        public async Task<Resident> GetResidentAsync(int id)
        {
            return await _context.Residents
                .Include(r => r.Payments)
                .Include(r => r.Attendances)
                .Include(r => r.ManagedByUser)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Resident> CreateResidentAsync(CreateResidentDto dto, string userId)
        {
            var resident = new Resident
            {
                FullName = dto.Name,
              
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                RoomNumber = dto.RoomNumber,
                CheckInDate = dto.CheckInDate,
                MonthlyRent = dto.MonthlyRent,
                
                IsActive = true,
                ManagedByUserId = userId
            };

            _context.Residents.Add(resident);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(userId, "Create", "Resident",
                resident.Id.ToString(), null, $"Created resident {resident.FullName}");

            return resident;
        }

        public async Task<Resident> UpdateResidentAsync(int id, UpdateResidentDto dto)
        {
            var resident = await _context.Residents.FindAsync(id);
            if (resident == null)
                throw new ArgumentException("Resident not found");

            var oldValues = $"Name: {resident.FullName}, Phone: {resident.PhoneNumber}, Email: {resident.Email}, Rent: {resident.MonthlyRent}";

            resident.FullName = dto.Name;
            resident.PhoneNumber = dto.PhoneNumber;
            resident.Email = dto.Email;
            resident.MonthlyRent = dto.MonthlyRent;
          
            resident.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var newValues = $"Name: {resident.FullName}, Phone: {resident.PhoneNumber}, Email: {resident.Email}, Rent: {resident.MonthlyRent}";
            await _auditService.LogActionAsync(null, "Update", "Resident",
                resident.Id.ToString(), oldValues, newValues);

            return resident;
        }

        public async Task<bool> DeleteResidentAsync(int id)
        {
            var resident = await _context.Residents.FindAsync(id);
            if (resident == null)
                return false;

            resident.IsActive = false;
            resident.CheckOutDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(null, "Delete", "Resident",
                resident.Id.ToString(), null, $"Soft deleted resident {resident.FullName}");

            return true;
        }

        public async Task<List<Payment>> GetResidentPaymentsAsync(int residentId)
        {
            return await _context.Payments
                .Where(p => p.ResidentId == residentId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetResidentAttendanceAsync(int residentId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Attendances
                .Where(a => a.ResidentId == residentId);

            if (fromDate.HasValue)
                query = query.Where(a => a.Date >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Date <= toDate.Value);

            return await query
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<Resident> CheckOutResidentAsync(int id, DateTime checkOutDate)
        {
            var resident = await _context.Residents.FindAsync(id);
            if (resident == null)
                throw new ArgumentException("Resident not found");

            resident.IsActive = false;
            resident.CheckOutDate = checkOutDate;
            resident.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(null, "CheckOut", "Resident",
                resident.Id.ToString(), null, $"Checked out resident {resident.FullName}");

            return resident;
        }

        public async Task<List<Resident>> SearchResidentsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetResidentsAsync();

            return await _context.Residents
                .Where(r => r.IsActive &&
                    (r.FullName.Contains(searchTerm) ||
                     r.IdentityNumber.Contains(searchTerm) ||
                     r.RoomNumber.Contains(searchTerm) ||
                     r.PhoneNumber.Contains(searchTerm) ||
                     r.Email.Contains(searchTerm)))
                .OrderBy(r => r.FullName)
                .ToListAsync();
        }
    }
}