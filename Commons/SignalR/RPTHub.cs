using Microsoft.AspNetCore.SignalR;

namespace SLA_Management.Commons.SignalR
{
    public class RPTHub : Hub
    {
        public async Task SendMessage(bool jobStatus ,string jobTable)
        {
            await Clients.All.SendAsync("RPT_Job_Process", jobStatus, jobTable);
        }
    }
}
