using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Pages.Maintenance
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MaintenanceTask MaintenanceTask { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            MaintenanceTask.ReportedDate = DateTime.UtcNow;
            MaintenanceTask.Status = "Pending";
            MaintenanceTask.ReportedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            _context.MaintenanceTasks.Add(MaintenanceTask);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}