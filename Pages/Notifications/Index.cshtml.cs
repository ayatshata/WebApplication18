using Microsoft.AspNetCore.Mvc.RazorPages;
using MughtaribatHouse.Models;
using MughtaribatHouse.Services;

namespace MughtaribatHouse.Pages.Notifications
{
    public class IndexModel : PageModel
    {
        private readonly INotificationService _notificationService;

        public IndexModel(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public List<Notification> Notifications { get; set; } = new();

        public async Task OnGetAsync()
        {
  
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
         
                Notifications = await _notificationService.GetUserNotificationsAsync(userId);
            }
            else
            {
           
                Notifications = new List<Notification>();
            }
        }
    }
}
