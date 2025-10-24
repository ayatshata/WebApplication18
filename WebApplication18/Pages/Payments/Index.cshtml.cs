using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Pages.Payments
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Payment> Payments { get; set; }

        public async Task OnGetAsync()
        {
            Payments = await _context.Payments
                .Include(p => p.Resident)
                .Include(p => p.ProcessedByUser)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
    }
}