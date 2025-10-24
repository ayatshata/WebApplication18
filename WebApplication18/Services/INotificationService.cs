using MughtaribatHouse.Models;

namespace MughtaribatHouse.Services
{
    public interface INotificationService
    {
        Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
        Task<Notification> CreateNotificationAsync(string userId, string title, string message,
            string type = "Info", string actionUrl = null);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> DeleteNotificationAsync(int notificationId);
    }
}