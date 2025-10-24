using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Pages.Settings
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public int TotalUsers { get; set; }

        public async Task OnGetAsync()
        {
            TotalUsers = _userManager.Users.Count();
        }
    }
}