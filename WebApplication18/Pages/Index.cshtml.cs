using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;

namespace MughtaribatHouse.Pages
{
    [AllowAnonymous] // ???? ??????? ??? ???? ????? ????
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public int TotalResidents { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int PendingMaintenance { get; set; }

        public async Task OnGetAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                try
                {
                    TotalResidents = await _context.Residents.CountAsync();

                    var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    MonthlyRevenue = await _context.Payments
                        .Where(p => p.ForMonth == currentMonth)
                        .SumAsync(p => (decimal?)p.Amount) ?? 0;

                    PendingMaintenance = await _context.MaintenanceTasks
                        .CountAsync(m => m.Status == "Pending" || m.Status == "InProgress");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "??? ??? ????? ????? ?????? ?????? ????????.");
                    TotalResidents = 0;
                    MonthlyRevenue = 0;
                    PendingMaintenance = 0;
                }
            }
        }
    }
}
