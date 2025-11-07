using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Pages.Documents
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Document> Documents { get; set; } = new List<Document>();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Documents
                                .Include(d => d.Resident)
                                .Include(d => d.UploadedByUser)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(Search))
            {
                query = query.Where(d => d.Title.Contains(Search) ||
                                         d.Description.Contains(Search) ||
                                         d.Category.Contains(Search));
            }

            Documents = await query.OrderByDescending(d => d.UploadedAt).ToListAsync();
        }
    }
}
