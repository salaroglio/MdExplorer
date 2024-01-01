using MdExplorer.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Hubs
{
    public class MonitorMDHub:Hub
    {

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

        //public async Task GetConnectionId()
        //    => await Clients.Client(Context.ConnectionId).SendAsync("getconnectionid", Context.ConnectionId);

    }
}
