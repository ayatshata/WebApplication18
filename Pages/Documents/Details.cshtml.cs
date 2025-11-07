using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Pages.Documents
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Document Document { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Document = await _context.Documents
                .Include(d => d.Resident)
                .Include(d => d.UploadedByUser)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (Document == null)
                return NotFound();

            return Page();
        }
    }
}
