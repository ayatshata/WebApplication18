using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Pages.Docs
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Document Document { get; set; }
        public bool CanDelete { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Document = await _context.Documents
                .Include(d => d.Resident)
                .Include(d => d.UploadedByUser)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (Document == null)
            {
                return NotFound();
            }

            // Check if user can delete this document
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            CanDelete = Document.UploadedByUserId == currentUserId ||
                       User.IsInRole("Admin") ||
                       User.IsInRole("Manager");

            return Page();
        }
    }
}