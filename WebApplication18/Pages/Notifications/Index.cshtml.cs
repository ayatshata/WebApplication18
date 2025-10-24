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

        public List<Notification> Notifications { get; set; }

        public async Task OnGetAsync()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Notifications = await _notificationService.GetUserNotificationsAsync(userId);
        }
    }
}