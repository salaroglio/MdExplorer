using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Utilities;
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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Service.HostedServices
{
    public class MonitorMDHostedService : IHostedService
    {
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly ILogger<MonitorMDHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWorkLink[] _linkManagers;
        private readonly IHelper _helper;
        private readonly FileChangeIgnoreConfiguration _ignoreConfiguration;

        public MonitorMDHostedService(
                IHubContext<MonitorMDHub> hubContext,
                FileSystemWatcher fileSystemWatcher,
                ILogger<MonitorMDHostedService> logger, 
                IServiceProvider serviceProvider,
                IWorkLink[] linkManagers,
                IHelper helper)
        {
            _hubContext = hubContext;
            _fileSystemWatcher = fileSystemWatcher;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _linkManagers = linkManagers;
            _helper = helper;
            
            // Load the ignore configuration from YAML file
            _ignoreConfiguration = LoadIgnoreConfiguration();
            
            // console closing management, send back closing server to the angular client
            handler = new ConsoleEventDelegate(SendExitToAngular);
            SetConsoleCtrlHandler(handler, true);

        }

        private bool SendExitToAngular(int eventType)
        {
            if (eventType == 2)
            {
                _hubContext.Clients.All.SendAsync("consoleClosed");
            }
            return false;                        
        }

        private FileChangeIgnoreConfiguration LoadIgnoreConfiguration()
        {
            var configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".mdchangeignore");
            
            if (File.Exists(configFilePath))
            {
                try
                {
                    var yamlContent = File.ReadAllText(configFilePath);
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
                    
                    var config = deserializer.Deserialize<FileChangeIgnoreConfiguration>(yamlContent);
                    _logger.LogInformation("Loaded file change ignore configuration from .mdchangeignore");
                    return config ?? new FileChangeIgnoreConfiguration();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load .mdchangeignore configuration. Using default hardcoded values.");
                }
            }
            else
            {
                _logger.LogWarning(".mdchangeignore file not found. Using default hardcoded values.");
            }

            // Return default configuration if file doesn't exist or parsing fails
            return new FileChangeIgnoreConfiguration
            {
                IgnoredDirectories = new List<string> { ".md" },
                IgnoredExtensions = new List<string> { ".pptx", ".docx", ".xlsx", ".xls", ".ppt", ".xlsb", ".bmpr", ".tmp" },
                IgnoredPatterns = new List<string> { ".md", ".0.pdnSave" },
                GitIgnoredFiles = new List<string> { "FETCH_HEAD", "COMMIT_EDITMSG", ".git/" },
                IgnoreFilesWithoutExtension = true
            };
        }

        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
                                               // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);


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
            //_fileSystemWatcher.Deleted += _fileSystemWatcher_Deleted;
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

        private async void _fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            // If wrong files then don't alert to client
            //bool isWrongDirectory = false, isWrongExtensionFile = false, isWrongGitFile =false;
            (var isWrongDirectory, var isWrongExtensionFile,var  isWrongGitFile, var hasNoExtension) = ThereIsNoNeedToAlertClient(e) ;

            if (isWrongDirectory || isWrongExtensionFile || isWrongGitFile || hasNoExtension)
            {
                return;
            }

            var lastWriteTime = File.GetLastWriteTime(e.FullPath);
            if (lastWriteTime > lastRead)
            {
                _logger.LogInformation($"Hey! {e.FullPath} has changed!");
                ParseNewFileIntoDB(e);

                var monitoredMd = new MonitoredMDModel
                {
                    Path = e.FullPath.Replace(_fileSystemWatcher.Path , string.Empty), //+ Path.DirectorySeparatorChar
                    Name = e.Name,
                    FullPath = e.FullPath,
                    RelativePath = e.FullPath.Replace(_fileSystemWatcher.Path , string.Empty) //+ Path.DirectorySeparatorChar

                };
                await _hubContext.Clients.All.SendAsync("markdownfileischanged", monitoredMd);
                lastRead = lastWriteTime.AddSeconds(1);
            }
        }

        private (bool,bool,bool,bool) ThereIsNoNeedToAlertClient(FileSystemEventArgs e)
        {
            // Check if file is in an ignored directory
            var isWrongDirectory = _ignoreConfiguration.IgnoredDirectories.Any(dir => 
                e.FullPath.Contains($"{Path.DirectorySeparatorChar}{dir}{Path.DirectorySeparatorChar}"));
            

            // Check if file has an ignored extension
            var extension = Path.GetExtension(e.FullPath).ToLower();
            var isWrongExtensionFile = _ignoreConfiguration.IgnoredExtensions.Any(ext => 
                extension == ext.ToLower());
            
            // Check for ignored patterns
            isWrongExtensionFile = isWrongExtensionFile || _ignoreConfiguration.IgnoredPatterns.Any(pattern => 
                e.FullPath.Contains(pattern));
            
            // Check for Git-related ignored files
            var isWrongGitFile = _ignoreConfiguration.GitIgnoredFiles.Any(gitFile => 
            {
                if (gitFile.EndsWith("/"))
                {
                    // It's a directory pattern
                    return e.FullPath.Contains($"{Path.DirectorySeparatorChar}{gitFile.TrimEnd('/')}{Path.DirectorySeparatorChar}");
                }
                else
                {
                    // It's a file pattern
                    return e.FullPath.Contains($"{Path.DirectorySeparatorChar}{gitFile}") || e.FullPath.Contains($".{gitFile}");
                }
            });
            
            var hasNoExtension = _ignoreConfiguration.IgnoreFilesWithoutExtension && 
                                Path.GetExtension(e.FullPath) == string.Empty;
                                
            return (isWrongDirectory, isWrongExtensionFile, isWrongGitFile, hasNoExtension);
        }

        private void ParseNewFileIntoDB(FileSystemEventArgs e)
        {
            using (var serviceScope = _serviceProvider.CreateScope())
            {
                // document analysis 
                // if not exits, then insert:
                // MarkdownFile + LinkInsideMarkdown
                // Else
                // Delete LinkInsideMarkdown and rebuild                    
                var engineDB = serviceScope.ServiceProvider.GetService<IEngineDB>();
                engineDB.BeginTransaction();
                var fileDal = engineDB.GetDal<MarkdownFile>();
                var mdf = fileDal.GetList().Where(_ => _.Path == e.FullPath).FirstOrDefault();
                if (mdf == null)
                {
                    mdf = new MarkdownFile
                    {
                        FileName = Path.GetFileName(e.FullPath),
                        FileType = "file",
                        Path = e.FullPath
                    };
                    fileDal.Save(mdf);
                }

                engineDB.Flush();
                var linkDal = engineDB.GetDal<LinkInsideMarkdown>();
                var listLinks = linkDal.GetList().Where(_ => _.MarkdownFile == mdf);
                foreach (var item in listLinks)
                {
                    linkDal.Delete(item);
                }

                engineDB.Flush();
                foreach (var getModifier in _linkManagers)
                {
                    var linksToStore = getModifier.GetLinksFromFile(e.FullPath);
                    foreach (var singleLink in linksToStore)
                    {
                        var fullPath = Path.GetDirectoryName(e.FullPath) + Path.DirectorySeparatorChar + singleLink.FullPath.Replace('/', Path.DirectorySeparatorChar);
                        var linkToStore = new LinkInsideMarkdown
                        {
                            FullPath = _helper.NormalizePath(fullPath),
                            Path = singleLink.FullPath,
                            Source = getModifier.GetType().Name,
                            LinkedCommand = singleLink.LinkedCommand,
                            SectionIndex = singleLink.SectionIndex,
                            MarkdownFile = mdf
                        };
                        linkDal.Save(linkToStore);
                    }
                }

                engineDB.Commit();
            }
        }
    }
}
