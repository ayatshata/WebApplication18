using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Pages.Maintenance
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<MaintenanceTask> MaintenanceTasks { get; set; }

        public async Task OnGetAsync()
        {
            MaintenanceTasks = await _context.MaintenanceTasks
                .Include(m => m.ReportedByUser)
                .OrderByDescending(m => m.ReportedDate)
                .ToListAsync();
        }
    }
}