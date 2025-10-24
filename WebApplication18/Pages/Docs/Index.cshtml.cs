using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Pages.Docs
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Document> Documents { get; set; } = new List<Document>();
        public List<Resident> Residents { get; set; } = new List<Resident>();

        public async Task OnGetAsync()
        {
            Documents = await _context.Documents
                .Include(d => d.Resident)
                .Include(d => d.UploadedByUser)
                .Where(d => d.Status == "Active")
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            Residents = await _context.Residents
                .Where(r => r.IsActive)
                .OrderBy(r => r.FullName)
                .ToListAsync();
        }
    }
}