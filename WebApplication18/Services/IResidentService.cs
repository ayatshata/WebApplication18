using MughtaribatHouse.Models;
using MughtaribatHouse.Models.DTOs;

namespace MughtaribatHouse.Services
{
    public interface IResidentService
    {
        Task<List<Resident>> GetResidentsAsync();
        Task<Resident> GetResidentAsync(int id);
        Task<Resident> CreateResidentAsync(CreateResidentDto dto, string userId);
        Task<Resident> UpdateResidentAsync(int id, UpdateResidentDto dto);
        Task<bool> DeleteResidentAsync(int id);
        Task<List<Payment>> GetResidentPaymentsAsync(int residentId);
        Task<List<Attendance>> GetResidentAttendanceAsync(int residentId, DateTime? fromDate, DateTime? toDate);
        Task<Resident> CheckOutResidentAsync(int id, DateTime checkOutDate);
        Task<List<Resident>> SearchResidentsAsync(string searchTerm);
    }
}