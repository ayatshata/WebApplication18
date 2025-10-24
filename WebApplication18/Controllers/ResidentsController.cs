using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MughtaribatHouse.Models.DTOs;
using MughtaribatHouse.Services;

namespace MughtaribatHouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ResidentsController : BaseApiController
    {
        private readonly IResidentService _residentService;

        public ResidentsController(IResidentService residentService)
        {
            _residentService = residentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetResidents()
        {
            var residents = await _residentService.GetResidentsAsync();
            return Success(residents);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetResident(int id)
        {
            var resident = await _residentService.GetResidentAsync(id);
            if (resident == null)
                return NotFound("المقيم غير موجود");

            return Success(resident);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateResident([FromBody] CreateResidentDto dto)
        {
            if (!ModelState.IsValid)
                return Error("بيانات المقيم غير صحيحة", ModelState);

            try
            {
                var resident = await _residentService.CreateResidentAsync(dto, UserId);
                return Success(resident, "تم إنشاء المقيم بنجاح");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateResident(int id, [FromBody] UpdateResidentDto dto)
        {
            if (!ModelState.IsValid)
                return Error("بيانات التحديث غير صحيحة", ModelState);

            try
            {
                var resident = await _residentService.UpdateResidentAsync(id, dto);
                return Success(resident, "تم تحديث بيانات المقيم بنجاح");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteResident(int id)
        {
            var result = await _residentService.DeleteResidentAsync(id);
            if (!result)
                return NotFound("المقيم غير موجود");

            return Success(null, "تم حذف المقيم بنجاح");
        }

        [HttpPost("{id}/checkout")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CheckOutResident(int id, [FromBody] CheckOutRequest request)
        {
            try
            {
                var resident = await _residentService.CheckOutResidentAsync(id, request.CheckOutDate);
                return Success(resident, "تم تسجيل خروج المقيم بنجاح");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet("{id}/payments")]
        public async Task<IActionResult> GetResidentPayments(int id)
        {
            var payments = await _residentService.GetResidentPaymentsAsync(id);
            return Success(payments);
        }

        [HttpGet("{id}/attendance")]
        public async Task<IActionResult> GetResidentAttendance(int id, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var attendance = await _residentService.GetResidentAttendanceAsync(id, fromDate, toDate);
            return Success(attendance);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchResidents([FromQuery] string term)
        {
            var residents = await _residentService.SearchResidentsAsync(term);
            return Success(residents);
        }
    }

    public class CheckOutRequest
    {
        public DateTime CheckOutDate { get; set; }
    }
}