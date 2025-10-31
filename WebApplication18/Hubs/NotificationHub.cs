using Microsoft.AspNetCore.SignalR;

namespace MughtaribatHouse.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string message)
        {
            // يبعت الإشعار لكل المستخدمين المتصلين
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
}
