using Microsoft.AspNet.SignalR;

namespace ISAI.Lessons.Web.Portal.Hubs
{
    public class OrderHub : Hub
    {

        public void HubTest()
        {
            Clients.All.hubTest();
        }

    }
}