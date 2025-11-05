using Microsoft.AspNetCore.SignalR;

namespace MughtaribatHouse.Hubs
{
    public class DashboardHub : Hub
    {
        public async Task SendStatisticsUpdate(object statistics)
        {
            await Clients.All.SendAsync("UpdateStatistics", statistics);
        }
    }
}