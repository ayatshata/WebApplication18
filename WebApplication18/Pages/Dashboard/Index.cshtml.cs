using Microsoft.AspNetCore.Mvc.RazorPages;
using MughtaribatHouse.Models.ViewModels;
using MughtaribatHouse.Services;

namespace MughtaribatHouse.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly IAnalyticsService _analyticsService;

        public IndexModel(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        public DashboardViewModel DashboardData { get; set; }

        public async Task OnGetAsync()
        {
            DashboardData = await _analyticsService.GetDashboardDataAsync();
        }
    }
}