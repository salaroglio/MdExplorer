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
            var configFilePath = Path.Combine(_fileSystemWatcher.Path, ".mdchangeignore");
            
            if (File.Exists(configFilePath))
            {
                try
                {
                    var yamlContent = File.ReadAllText(configFilePath);
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
                    
                    var config = deserializer.Deserialize<FileChangeIgnoreConfiguration>(yamlContent);
                    _logger.LogInformation($"Loaded file change ignore configuration from {configFilePath}");
                    return config ?? new FileChangeIgnoreConfiguration();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load .mdchangeignore configuration. Using default hardcoded values.");
                }
            }
            else
            {
                _logger.LogWarning($".mdchangeignore file not found at {configFilePath}. Using default hardcoded values.");
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
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 //| NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 //| NotifyFilters.Security
                                 | NotifyFilters.Size;
            //_fileSystemWatcher.Filter = "*.md";
            _fileSystemWatcher.IncludeSubdirectories = true;
            _fileSystemWatcher.EnableRaisingEvents = true;
            _fileSystemWatcher.Changed += _fileSystemWatcher_Changed;
            _fileSystemWatcher.Created += _fileSystemWatcher_Created;
            _fileSystemWatcher.Renamed += _fileSystemWatcher_Renamed;
            //_fileSystemWatcher.Deleted += _fileSystemWatcher_Deleted;
            return Task.CompletedTask;
        }

        private async void _fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                _logger.LogInformation($"🔍 FileSystemWatcher.Created triggered for: {e.FullPath}");
                
                // Controlla se è un file markdown
                if (!Path.GetExtension(e.FullPath).Equals(".md", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation($"❌ File {e.FullPath} is not a markdown file. Extension: {Path.GetExtension(e.FullPath)}");
                    return;
                }

                _logger.LogInformation($"✅ File {e.FullPath} is a markdown file");

                // Per i file markdown appena creati, usiamo una logica di filtraggio specifica
                if (ShouldIgnoreMarkdownFile(e.FullPath))
                {
                    _logger.LogInformation($"❌ Markdown file {e.FullPath} filtered out by markdown-specific rules");
                    return;
                }

                _logger.LogInformation($"🎯 Processing new markdown file: {e.FullPath}");
                
                // Salva il file nel database
                ParseNewFileIntoDB(e);

                // Crea il nodo per l'invio tramite SignalR
                var relativePath = GetRelativePath(e.FullPath);
                var newFileNode = new
                {
                    Name = Path.GetFileName(e.FullPath),
                    FullPath = e.FullPath,
                    Path = relativePath,
                    RelativePath = relativePath,
                    Type = "mdFile",
                    Level = CalculateFileLevel(relativePath),
                    Expandable = false,
                    IsIndexed = true, // Il file appena creato è considerato indicizzato
                    IndexingStatus = "completed"
                };

                // Invia notifica tramite SignalR per la creazione del nuovo nodo
                // Reduced logging - only log errors
                await _hubContext.Clients.All.SendAsync("markdownFileCreated", newFileNode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❗ ERROR in _fileSystemWatcher_Created for file: {e.FullPath}");
                _logger.LogError($"❗ Exception Type: {ex.GetType().Name}");
                _logger.LogError($"❗ Exception Message: {ex.Message}");
                _logger.LogError($"❗ Stack Trace: {ex.StackTrace}");
                
                // Log inner exception if exists
                if (ex.InnerException != null)
                {
                    _logger.LogError($"❗ Inner Exception: {ex.InnerException.Message}");
                }
                
                // Don't rethrow - keep the FileSystemWatcher alive
            }
        }

        private async void _fileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            try
            {
                // Controlla se il file rinominato è un file markdown (sia prima che dopo)
                bool oldIsMarkdown = Path.GetExtension(e.OldFullPath).Equals(".md", StringComparison.OrdinalIgnoreCase);
                bool newIsMarkdown = Path.GetExtension(e.FullPath).Equals(".md", StringComparison.OrdinalIgnoreCase);
                
                // Log only if it's a markdown file rename
                if (oldIsMarkdown || newIsMarkdown)
                {
                    _logger.LogInformation($"📝 MD file renamed: {Path.GetFileName(e.OldFullPath)} → {Path.GetFileName(e.FullPath)}");
                }
                
                // Processa se:
                // 1. Un file è stato rinominato DA qualsiasi estensione A .md
                // 2. Un file .md è stato rinominato (cambio nome ma ancora .md)
                bool shouldProcess = newIsMarkdown || oldIsMarkdown;
                
                if (!shouldProcess)
                {
                    _logger.LogInformation($"❌ Rename not relevant: neither old nor new file is markdown");
                    return;
                }
                
                // Log del tipo di rinominazione
                if (oldIsMarkdown && newIsMarkdown)
                {
                    _logger.LogInformation($"✅ Markdown file renamed (name change): {e.OldFullPath} → {e.FullPath}");
                }
                else if (!oldIsMarkdown && newIsMarkdown)
                {
                    _logger.LogInformation($"✅ File renamed to markdown: {e.FullPath}");
                }
                else if (oldIsMarkdown && !newIsMarkdown)
                {
                    _logger.LogInformation($"⚠️ Markdown file renamed to non-markdown: {e.OldFullPath} → {e.FullPath}");
                    // Potremmo voler gestire questo caso come una cancellazione
                    return;
                }

                // Per i file markdown appena creati/rinominati, usiamo una logica di filtraggio specifica
                if (ShouldIgnoreMarkdownFile(e.FullPath))
                {
                    _logger.LogInformation($"❌ Markdown file {e.FullPath} filtered out by markdown-specific rules");
                    return;
                }

                _logger.LogInformation($"🎯 Processing renamed markdown file: {e.FullPath}");
                
                // Per rinominazioni di file .md -> .md, dobbiamo notificare anche la rimozione del vecchio file
                if (oldIsMarkdown && newIsMarkdown)
                {
                    // Notifica la rimozione del vecchio file
                    var oldRelativePath = GetRelativePath(e.OldFullPath);
                    var fileDeletedData = new
                    {
                        FullPath = e.OldFullPath,
                        RelativePath = oldRelativePath,
                        Name = Path.GetFileName(e.OldFullPath)
                    };
                    
                    // Reduced logging
                    await _hubContext.Clients.All.SendAsync("markdownFileDeleted", fileDeletedData);
                }
                
                // Crea un FileSystemEventArgs per il parsing del database
                var fileEvent = new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(e.FullPath), Path.GetFileName(e.FullPath));
                
                // Salva il file nel database
                ParseNewFileIntoDB(fileEvent);

                // Crea il nodo per l'invio tramite SignalR
                var relativePath = GetRelativePath(e.FullPath);
                var newFileNode = new
                {
                    Name = Path.GetFileName(e.FullPath),
                    FullPath = e.FullPath,
                    Path = relativePath,
                    RelativePath = relativePath,
                    Type = "mdFile",
                    Level = CalculateFileLevel(relativePath),
                    Expandable = false,
                    IsIndexed = true, // Il file rinominato è considerato indicizzato
                    IndexingStatus = "completed"
                };

                // Invia notifica tramite SignalR per la creazione del nuovo nodo
                await _hubContext.Clients.All.SendAsync("markdownFileCreated", newFileNode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❗ ERROR in _fileSystemWatcher_Renamed for file: {e.FullPath}");
                _logger.LogError($"❗ Exception Type: {ex.GetType().Name}");
                _logger.LogError($"❗ Exception Message: {ex.Message}");
                _logger.LogError($"❗ Stack Trace: {ex.StackTrace}");
                
                // Log inner exception if exists
                if (ex.InnerException != null)
                {
                    _logger.LogError($"❗ Inner Exception: {ex.InnerException.Message}");
                }
                
                // Don't rethrow - keep the FileSystemWatcher alive
            }
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

        private int CalculateFileLevel(string relativePath)
        {
            // Rimuovi il separatore iniziale se presente
            var cleanPath = relativePath.TrimStart(Path.DirectorySeparatorChar);
            
            if (string.IsNullOrEmpty(cleanPath))
            {
                return 0; // File nella root
            }
            
            // Conta il numero di separatori per determinare il livello
            return cleanPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).Length - 1;
        }

        private string GetRelativePath(string fullPath)
        {
            var relativePath = fullPath.Replace(_fileSystemWatcher.Path, string.Empty);
            if (relativePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
            {
                relativePath = relativePath.Substring(1);
            }
            return relativePath;
        }

        private bool ShouldIgnoreMarkdownFile(string fullPath)
        {
            // Logica di filtraggio specifica per file markdown
            // NON filtriamo per estensione .md (è quello che vogliamo!)
            
            // Get relative path from project root
            var relativePath = fullPath.Substring(_fileSystemWatcher.Path.Length).TrimStart(Path.DirectorySeparatorChar);
            relativePath = relativePath.Replace(Path.DirectorySeparatorChar, '/');

            // Controlla se è in una cartella .md (che sono cartelle di sistema/cache)
            if (_ignoreConfiguration.IgnoredDirectories.Any(dir => 
                relativePath.Contains($"/{dir}/") || relativePath.StartsWith($"{dir}/")))
            {
                return true;
            }
            
            // Controlla per file Git-related ignored
            if (_ignoreConfiguration.GitIgnoredFiles.Any(gitFile => 
            {
                if (gitFile.EndsWith("/"))
                {
                    // It's a directory pattern
                    return relativePath.Contains($"/{gitFile.TrimEnd('/')}/");
                }
                else
                {
                    // It's a file pattern
                    return relativePath.Contains($"/{gitFile}") || relativePath.Contains($".{gitFile}");
                }
            }))
            {
                return true;
            }
            
            // Non ignoriamo per IgnoredPatterns che contengono ".md" dato che vogliamo i file .md
            // Non ignoriamo per IgnoredExtensions dato che .md non dovrebbe essere in quella lista
            
            return false;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        DateTime lastRead = DateTime.MinValue;

        private async void _fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                _logger.LogInformation($"📝 FileSystemWatcher.Changed triggered for: {e.FullPath}");
                
                // Check if it's a directory - skip directories
                if (Directory.Exists(e.FullPath))
                {
                    _logger.LogInformation($"❌ Path {e.FullPath} is a directory, skipping");
                    return;
                }
                
                // Check if it's a markdown file
                if (!Path.GetExtension(e.FullPath).Equals(".md", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation($"❌ File {e.FullPath} is not a markdown file. Extension: {Path.GetExtension(e.FullPath)}");
                    return;
                }

                // For markdown files, we want to process changes even if they're in .md directories
                // So we use a special check that doesn't filter out .md files
                if (ShouldIgnoreMarkdownFile(e.FullPath))
                {
                    _logger.LogInformation($"❌ Markdown file {e.FullPath} filtered out by markdown-specific rules");
                    return;
                }

                var lastWriteTime = File.GetLastWriteTime(e.FullPath);
                if (lastWriteTime > lastRead)
                {
                    _logger.LogInformation($"✅ Markdown file changed: {e.FullPath}");
                    ParseNewFileIntoDB(e);

                    // Calculate relative path and ensure it's correct (remove leading separator)
                    var relativePath = GetRelativePath(e.FullPath);
                    
                    // Removed verbose logging
                    
                    var monitoredMd = new MonitoredMDModel
                    {
                        Path = relativePath,
                        Name = Path.GetFileName(e.FullPath), // Use Path.GetFileName to ensure consistency
                        FullPath = e.FullPath,
                        RelativePath = relativePath
                    };
                    
                    // Reduced logging
                    await _hubContext.Clients.All.SendAsync("markdownfileischanged", monitoredMd);
                    
                    lastRead = lastWriteTime.AddSeconds(1);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❗ ERROR in _fileSystemWatcher_Changed for file: {e.FullPath}");
                _logger.LogError($"❗ Exception Type: {ex.GetType().Name}");
                _logger.LogError($"❗ Exception Message: {ex.Message}");
                _logger.LogError($"❗ Stack Trace: {ex.StackTrace}");
                
                // Log inner exception if exists
                if (ex.InnerException != null)
                {
                    _logger.LogError($"❗ Inner Exception: {ex.InnerException.Message}");
                }
                
                // Don't rethrow - keep the FileSystemWatcher alive
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
