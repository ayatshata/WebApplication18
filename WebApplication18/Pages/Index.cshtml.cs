using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MughtaribatHouse.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MughtaribatHouse.Pages.Attendance
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // ------------------ INPUT ------------------
        [BindProperty]
        public MughtaribatHouse.Models.Attendance Input { get; set; }

        [BindProperty]
        public string CheckInTimeString { get; set; }

        [BindProperty]
        public string CheckOutTimeString { get; set; }

        [BindProperty]
        public bool IsEditMode { get; set; }

        // ------------------ FILTERS ------------------
        [BindProperty(SupportsGet = true)]
        public int? FilterResidentId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FilterFromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FilterToDate { get; set; }

        public SelectList ResidentsSelectList { get; set; }

        public List<MughtaribatHouse.Models.Attendance> AllAttendances { get; set; } = new();

        // ------------------ GET ------------------
        public async Task OnGetAsync(int? editId = null)
        {
            await LoadResidentsSelectListAsync();

            var query = _context.Attendances
                .Include(a => a.Resident)
                .AsQueryable();

            if (FilterResidentId.HasValue)
                query = query.Where(a => a.ResidentId == FilterResidentId.Value);

            if (FilterFromDate.HasValue)
                query = query.Where(a => a.Date >= FilterFromDate.Value);

            if (FilterToDate.HasValue)
                query = query.Where(a => a.Date <= FilterToDate.Value);

            AllAttendances = await query
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            // ------------------ Load Edit ------------------
            if (editId.HasValue)
            {
                IsEditMode = true;
                Input = await _context.Attendances.FindAsync(editId.Value);
                if (Input != null)
                {
                    CheckInTimeString = Input.CheckInTime?.ToString(@"hh\:mm");
                    CheckOutTimeString = Input.CheckOutTime?.ToString(@"hh\:mm");
                }
            }
        }

        // ------------------ CREATE / UPDATE ------------------
        public async Task<IActionResult> OnPostAsync()
        {
            if (IsEditMode)
                return await OnPostEditAsync();

            if (!ModelState.IsValid)
            {
                await LoadResidentsSelectListAsync();
                return Page();
            }

            if (TimeSpan.TryParse(CheckInTimeString, out var checkIn))
                Input.CheckInTime = checkIn;

            if (TimeSpan.TryParse(CheckOutTimeString, out var checkOut))
                Input.CheckOutTime = checkOut;

            Input.RecordedAt = DateTime.UtcNow;
            Input.RecordedByUserId = User.Identity?.Name ?? "Admin";

            await _context.Attendances.AddAsync(Input);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تسجيل الحضور بنجاح ✅";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            var attendance = await _context.Attendances.FindAsync(Input.Id);
            if (attendance == null)
            {
                TempData["Error"] = "السجل غير موجود ❌";
                return RedirectToPage();
            }

            attendance.ResidentId = Input.ResidentId;
            attendance.Date = Input.Date;
            attendance.Notes = Input.Notes;

            if (TimeSpan.TryParse(CheckInTimeString, out var checkIn))
                attendance.CheckInTime = checkIn;

            if (TimeSpan.TryParse(CheckOutTimeString, out var checkOut))
                attendance.CheckOutTime = checkOut;

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تعديل السجل بنجاح ✏️";
            return RedirectToPage();
        }

        // ------------------ DELETE ------------------
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null)
            {
                TempData["Error"] = "السجل غير موجود ❌";
                return RedirectToPage();
            }

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف السجل بنجاح 🗑️";
            return RedirectToPage();
        }

        // ------------------ DETAILS ------------------
        public async Task<IActionResult> OnGetDetailsAsync(int id)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Resident)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
            {
                TempData["Error"] = "السجل غير موجود ❌";
                return RedirectToPage();
            }

            Input = attendance;
            CheckInTimeString = attendance.CheckInTime?.ToString(@"hh\:mm");
            CheckOutTimeString = attendance.CheckOutTime?.ToString(@"hh\:mm");

            return Page();
        }

        // ------------------ HELPER ------------------
        private async Task LoadResidentsSelectListAsync()
        {
            var residents = await _context.Residents.ToListAsync();
            ResidentsSelectList = new SelectList(residents, "Id", "FullName");
        }
    }
}
