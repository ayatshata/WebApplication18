using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Pages.Residents
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Resident> Residents { get; set; }

        public async Task OnGetAsync()
        {
            Residents = await _context.Residents
                .Include(r => r.ManagedByUser)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}