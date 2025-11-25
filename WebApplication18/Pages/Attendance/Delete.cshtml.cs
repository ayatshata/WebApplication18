using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;

namespace MughtaribatHouse.Pages.Attendance
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public MughtaribatHouse.Models.Attendance Attendance { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Attendance = await _context.Attendances
                .Include(a => a.Resident)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (Attendance == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var entry = await _context.Attendances.FindAsync(id);

            if (entry != null)
            {
                _context.Attendances.Remove(entry);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/Attendance/Index");
        }
    }
}
