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

        [BindProperty]
        public MughtaribatHouse.Models.Attendance Input { get; set; }

        [BindProperty]
        public string CheckInTimeString { get; set; }

        [BindProperty]
        public string CheckOutTimeString { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FilterResidentId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FilterFromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FilterToDate { get; set; }

        public SelectList ResidentsSelectList { get; set; }

        public List<MughtaribatHouse.Models.Attendance> AllAttendances { get; set; } = new();

        public async Task OnGetAsync()
        {
            var residents = await _context.Residents.ToListAsync();
            ResidentsSelectList = new SelectList(residents, "Id", "FullName");

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
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var residents = await _context.Residents.ToListAsync();
                ResidentsSelectList = new SelectList(residents, "Id", "FullName");
                return Page();
            }

            if (TimeSpan.TryParse(CheckInTimeString, out var checkIn))
            {
                Input.CheckInTime = checkIn;
            }

            if (TimeSpan.TryParse(CheckOutTimeString, out var checkOut))
            {
                Input.CheckOutTime = checkOut;
            }

            Input.RecordedAt = DateTime.UtcNow;
            Input.RecordedByUserId = User.Identity?.Name ?? "Admin";

            await _context.Attendances.AddAsync(Input);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تسجيل الحضور بنجاح ✅";
            return RedirectToPage("/Attendance/Index");
        }
    }
}
