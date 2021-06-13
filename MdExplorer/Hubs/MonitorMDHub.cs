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
     
       
        //public async Task BroadcastMarkdownData(List<MarkdownModel> data) => await Clients.All.SendAsync("broadcastmarkdowndata", data);
    }
}
