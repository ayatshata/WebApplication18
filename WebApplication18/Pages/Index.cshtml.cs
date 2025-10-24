using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;

namespace MughtaribatHouse.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalResidents { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int PendingMaintenance { get; set; }

        public async Task OnGetAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                TotalResidents = await _context.Residents.CountAsync();

                var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                MonthlyRevenue = await _context.Payments
                    .Where(p => p.ForMonth == currentMonth)
                    .SumAsync(p => p.Amount);

                PendingMaintenance = await _context.MaintenanceTasks
                    .CountAsync(m => m.Status == "Pending" || m.Status == "InProgress");
            }
        }
    }
}