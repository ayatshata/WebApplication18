using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;
using Microsoft.AspNetCore.Mvc;

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

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Residents
                .Include(r => r.ManagedByUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(Search))
            {
                query = query.Where(r =>
                    r.FullName.Contains(Search) ||
                    r.IdentityNumber.Contains(Search) ||
                    r.RoomNumber.Contains(Search) ||
                    r.PhoneNumber.Contains(Search));
            }

            Residents = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }
    }
}
