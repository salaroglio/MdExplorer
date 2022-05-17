using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Hubs;
using MdExplorer.Models;
using MdExplorer.Service.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly IWorkLink[] _linkManagers;

        public MonitorMDHostedService(IHubContext<MonitorMDHub> hubContext,
                FileSystemWatcher fileSystemWatcher,
                ILogger<MonitorMDHostedService> logger, IServiceProvider serviceProvider,
                IWorkLink[] linkManagers)
        {
            _hubContext = hubContext;
            _fileSystemWatcher = fileSystemWatcher;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _linkManagers = linkManagers;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"monitored path: { _fileSystemWatcher.Path}");
            _fileSystemWatcher.NotifyFilter = NotifyFilters.Attributes
                                 //| NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 //| NotifyFilters.LastAccess
                                 //| NotifyFilters.LastWrite
                                 //| NotifyFilters.Security
                                 | NotifyFilters.Size;
            //_fileSystemWatcher.Filter = "*.md";
            _fileSystemWatcher.IncludeSubdirectories = true;
            _fileSystemWatcher.EnableRaisingEvents = true;
            _fileSystemWatcher.Changed += _fileSystemWatcher_Changed;
            //_fileSystemWatcher.Created += _fileSystemWatcher_Created;
            //_fileSystemWatcher.Renamed += _fileSystemWatcher_Renamed;
            _fileSystemWatcher.Deleted += _fileSystemWatcher_Deleted;
            return Task.CompletedTask;
        }

        private void _fileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            //SaveEventOnDb(e);
        }

        private static string SetNameChangedType(string fullPath, string changeType)
        {
            var fileAttr = File.GetAttributes(fullPath);

            if (fileAttr.HasFlag(FileAttributes.Directory))
            {
                changeType += "_Directory";
            }
            else
            {
                changeType += "_File";
            }

            return changeType;
        }
        
       
        

        

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        DateTime lastRead = DateTime.MinValue;
        private void _fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            
            // Inserisci l'informazione nel file di refactoring
            
            var trafficLight1 = e.FullPath.Contains($"{Path.DirectorySeparatorChar}.md{Path.DirectorySeparatorChar}");
            var trafficLight2 = e.FullPath.Contains($"{Path.DirectorySeparatorChar}.md") || e.FullPath.Contains($".docx");
            if (trafficLight1 || trafficLight2)
            {
                return;
            }
            var lastWriteTime = File.GetLastWriteTime(e.FullPath);
            if (lastWriteTime > lastRead)
            {
                _logger.LogInformation($"Hey! {e.FullPath} has changed!");

                var monitoredMd = new MonitoredMDModel
                {
                    Path = e.FullPath.Replace(_fileSystemWatcher.Path + Path.DirectorySeparatorChar, string.Empty),
                    Name = e.Name,
                    FullPath = e.FullPath,
                    RelativePath = e.FullPath.Replace(_fileSystemWatcher.Path + Path.DirectorySeparatorChar, string.Empty)

                };
                _hubContext.Clients.All.SendAsync("markdownfileischanged", monitoredMd);
                lastRead = lastWriteTime.AddSeconds(1);
            }
        }
    }
}
