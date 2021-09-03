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
        private readonly IManageLink[] _linkManagers;

        public MonitorMDHostedService(IHubContext<MonitorMDHub> hubContext,
                FileSystemWatcher fileSystemWatcher,
                ILogger<MonitorMDHostedService> logger, IServiceProvider serviceProvider,
                IManageLink[] linkManagers)
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
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;
            //_fileSystemWatcher.Filter = "*.md";
            _fileSystemWatcher.IncludeSubdirectories = true;
            _fileSystemWatcher.EnableRaisingEvents = true;
            _fileSystemWatcher.Changed += ChangeWithLove;
            _fileSystemWatcher.Created += _fileSystemWatcher_Created;
            _fileSystemWatcher.Renamed += _fileSystemWatcher_Renamed;            
            _fileSystemWatcher.Deleted += _fileSystemWatcher_Deleted;
            return Task.CompletedTask;
        }

        private void _fileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void _fileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var engineDb = scope.ServiceProvider.GetService<IEngineDB>();
                engineDb.BeginTransaction();
                // Cerco se qualche file è coinvolto
                var currentGroupId = Guid.NewGuid();

                var linkInsideDAL = engineDb.GetDal<LinkInsideMarkdown>();
                var listOfFiles = linkInsideDAL.GetList().Where(_ => _.FullPath.Contains(e.OldFullPath)).Select(_=>_.MarkdownFile).ToList();


                var refactoringFileSystemEventDal = engineDb.GetDal<RefactoringFilesystemEvent>();
                var refFileSysEvent = new RefactoringFilesystemEvent
                {
                    EventName = "FileRenamed",
                    RefactoringGroupId = currentGroupId,
                    NewFullPath = e.FullPath,
                    OldFullPath = e.OldFullPath,
                    Processed = false
                };

                refactoringFileSystemEventDal.Save(refFileSysEvent);

                // le informazioni da mandare indietro sono:
                // i file coinvolti dal cambiamento originale che l'app deve
                // manipolare
                // info descrittive del cambiamento impostato dall'utente

                var monitoredMd = new RefactoredFile
                {
                    NewFullPath = e.FullPath,
                    OldFullPath = e.OldFullPath,
                    Name = Path.GetFileName(e.FullPath),                    
                };

                var refactoringEvent = new RefactoringFileEvent();
                refactoringEvent.EventName = "FileRenamed";
                engineDb.Commit();
                _hubContext.Clients.All.SendAsync("refactoringFileEvent", new { refactoringEvent = refactoringEvent });
            }

        }

        private void _fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            //throw new NotImplementedException();

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        DateTime lastRead = DateTime.MinValue;
        private void ChangeWithLove(object sender, FileSystemEventArgs e)
        {
            var fileAttr = File.GetAttributes(e.FullPath);                    

            if (fileAttr.HasFlag(FileAttributes.Directory))
            {
                // Inserisci l'informazione nel file di refactoring
                using (var scope = _serviceProvider.CreateScope())
                {
                    var engineDb = scope.ServiceProvider.GetService<IEngineDB>();
                    engineDb.BeginTransaction();
                    var eventDal = engineDb.GetDal<RefactoringFilesystemEvent>();
                    var refactoring = new RefactoringFilesystemEvent
                    {
                        EventName = "ChangeDirectory",
                        NewFullPath = e.FullPath,
                        Processed = false,
                        RefactoringGroupId = Guid.NewGuid()                        
                    };
                    eventDal.Save(refactoring);

                    engineDb.Commit();
                }

            }

            var trafficLight1 = e.FullPath.Contains($"{Path.DirectorySeparatorChar}.md{Path.DirectorySeparatorChar}");
            var trafficLight2 = e.FullPath.Contains($"{Path.DirectorySeparatorChar}.md")|| e.FullPath.Contains($".docx");
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
                    Name = e.Name
                };
                _hubContext.Clients.All.SendAsync("markdownfileischanged", monitoredMd);
                lastRead = lastWriteTime.AddSeconds(1);
            }
        }
    }
}
