using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Pages.Attendance
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
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
    }
}
