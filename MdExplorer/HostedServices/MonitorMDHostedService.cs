using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Hubs;
using MdExplorer.Models;
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

                var oldFullPath = e.OldFullPath;
                var newFullPath = e.FullPath;
                // devo andare a cercare tutti i files coinvolti dal cambiamento.
                var fileAttr = File.GetAttributes(newFullPath);

                if (fileAttr.HasFlag(FileAttributes.Directory))
                {
                    // gestisci il cambio di directory
                }
                else
                {
                    // gestisci il rename di un file
                    var linkDal = engineDb.GetDal<MarkdownFile>();
                    var affectedFiles = linkDal.GetList().Where(_ => _.Links.Any(l => l.FullPath.Contains(oldFullPath)));
                        //linkDal.GetList().Where(_ => _.FullPath.Contains(oldFullPath)).GroupBy(_ => _.MarkdownFile);
                    var oldFileName = Path.GetFileName(oldFullPath);
                    var newFileName = Path.GetFileName(newFullPath);

                    foreach (var itemMarkdownFile in affectedFiles)
                    {
                        foreach (var linkManager in _linkManagers)
                        {
                            linkManager.SetLinkIntoFile(itemMarkdownFile.Path, oldFileName, newFileName);
                        }
                    }
                }




                //    // faccio un primo match con il fullPath (giusto per tagliare fuori doppioni (caso assets))
                //    // devo capire se si tratta di un folder (guardo se ha il punto o meno)
                //    // oppure se si tratta di un file
                //    // da capire come gestire il cut and paste. (viene gestito come un renamed, ma diventa un macello il ricalcolo del path)
                //    // capire che cosa devo scrivere al posto del precedente link (io farei una replace del vecchio con il nuovo)
                //    // devo gestire casi particolari comme /asset/asset/asset/asset/asset.md devo capire in quale punto esatto cambiare
                //    // mi aiuta di sicuro il full path. Ma devo trovare una relazione tra fullpath e relative path
                //    // Modificare tutti i link sul filesystem (devo usare le stesse funzioni di get, per andare ad agganciare i set)
                //    // il grande casino è come gestire i file relativi e capire se sono veramente coinvolti
                //    // Modificare tutti i link sul db
                //}



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
            var trafficLight1 = e.FullPath.Contains($"{Path.DirectorySeparatorChar}.md{Path.DirectorySeparatorChar}");
            var trafficLight2 = e.FullPath.Contains($"{Path.DirectorySeparatorChar}.md");
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
