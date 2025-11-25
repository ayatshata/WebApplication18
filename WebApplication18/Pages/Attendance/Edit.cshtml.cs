using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;

namespace MughtaribatHouse.Pages.Attendance
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MughtaribatHouse.Models.Attendance Input { get; set; }

        [BindProperty]
        public string? CheckInTimeString { get; set; }

        [BindProperty]
        public string? CheckOutTimeString { get; set; }

        public SelectList ResidentsSelectList { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Input = await _context.Attendances.FindAsync(id);

            if (Input == null)
                return NotFound();

            CheckInTimeString = Input.CheckInTime?.ToString(@"hh\:mm");
            CheckOutTimeString = Input.CheckOutTime?.ToString(@"hh\:mm");

            ResidentsSelectList = new SelectList(_context.Residents, "Id", "FullName");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var attendance = await _context.Attendances.FindAsync(Input.Id);

            if (attendance == null)
                return NotFound();

            attendance.ResidentId = Input.ResidentId;
            attendance.Date = Input.Date;
            attendance.Notes = Input.Notes;

            attendance.CheckInTime = TimeSpan.TryParse(CheckInTimeString, out var inT) ? inT : null;
            attendance.CheckOutTime = TimeSpan.TryParse(CheckOutTimeString, out var outT) ? outT : null;

            await _context.SaveChangesAsync();

            return RedirectToPage("/Attendance/Index");
        }
    }
}
