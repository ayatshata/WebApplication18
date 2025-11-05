using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Pages.Maintenance
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public MaintenanceTask MaintenanceTask { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MaintenanceTask = await _context.MaintenanceTasks
                .Include(m => m.ReportedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (MaintenanceTask == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id, string status)
        {
            if (id == null || string.IsNullOrEmpty(status))
            {
                return NotFound();
            }

            var maintenanceTask = await _context.MaintenanceTasks.FindAsync(id);
            if (maintenanceTask == null)
            {
                return NotFound();
            }

            maintenanceTask.Status = status;

            if (status == "Completed")
            {
                maintenanceTask.CompletedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id });
        }
    }
}