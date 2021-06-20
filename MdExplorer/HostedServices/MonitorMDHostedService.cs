using MdExplorer.Hubs;
using MdExplorer.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Service.HostedServices
{
    public class MonitorMDHostedService : IHostedService
    {
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly ILogger<MonitorMDHostedService> _logger;

        public MonitorMDHostedService(IHubContext<MonitorMDHub> hubContext, 
                FileSystemWatcher fileSystemWatcher, 
                ILogger<MonitorMDHostedService> logger)
        {
            _hubContext = hubContext;
            _fileSystemWatcher = fileSystemWatcher;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"monitored path: { _fileSystemWatcher.Path}");            
            _fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _fileSystemWatcher.Filter = "*.md";
            _fileSystemWatcher.IncludeSubdirectories = true;
            _fileSystemWatcher.EnableRaisingEvents = true;
            _fileSystemWatcher.Changed += ChangeWithLove;            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {            
            return Task.CompletedTask;
        }

        DateTime lastRead = DateTime.MinValue;
        private void ChangeWithLove(object sender, FileSystemEventArgs e)
        {
            
            var lastWriteTime = File.GetLastWriteTime(e.FullPath);
            if (lastWriteTime > lastRead)
            {
                _logger.LogInformation($"Hey! {e.FullPath} has changed!");

                var monitoredMd = new MonitoredMDModel
                {
                    Path =  e.FullPath.Replace(_fileSystemWatcher.Path,string.Empty),
                    Name = e.Name
                };
                _hubContext.Clients.All.SendAsync("markdownfileischanged", monitoredMd);
                lastRead = lastWriteTime.AddSeconds(1);
            }
        }
    }
}
