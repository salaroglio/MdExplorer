using MdExplorer.Hubs;
using MdExplorer.Service.Models;
using Ad.Tools.Dal;
using Ad.Tools.Dal.Concrete;
using MDExplorer.DataAccess.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using MdExplorer.Features;
using MdExplorer.Service;
using MdExplorer.Abstractions.DB;
using MdExplorer.DataAccess.Engine;
using MdExplorer.Features.Utilities;
using System.Linq;
using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using MdExplorer.Service.Automapper.RefactoringFilesController;
using Ad.Tools.FluentMigrator.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git;
using System.Text;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.AI;
using MdExplorer.Service.Services;
using MdExplorer.Abstractions.Services;
using System.Reflection;

namespace MdExplorer
{
    public class Startup
    {
        public static string[] Args;



        public IConfiguration _Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            _Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServiceFeatures();
            services.Configure<MdExplorerAppSettings>(_Configuration.GetSection(MdExplorerAppSettings.MdExplorer));
            string pathFromParameter = Args.Count() > 0 ? Path.GetDirectoryName(Args[0]) : null;
            ProjectsManager.SetProjectInitialization(services, pathFromParameter);

            services.AddAutoMapper(typeof(ProjectProfile).Assembly);
            services.AddMDExplorerCommands();

            // Add FoldersIgnoreService
            services.AddSingleton<FoldersIgnoreService>();

            // Project-level metadata stored in .development.yml (shared across users)
            services.AddSingleton<Services.IProjectMetadataService, Services.ProjectMetadataService>();

            // Git authors discovery for MdE Team participants
            services.AddSingleton<Services.Git.IGitAuthorsService, Services.Git.GitAuthorsService>();

            // Add ServerCache for command factories
            services.AddSingleton<IServerCache, ServerCache>();

            // Add DatabaseManager for multi-client support
            services.AddSingleton<Services.DatabaseManager.IDatabaseManager, Services.DatabaseManager.DatabaseManager>();

            // Add FileSystemWatcherManager for multi-client support
            services.AddSingleton<Services.FileSystemWatcherManager.IFileSystemWatcherManager, Services.FileSystemWatcherManager.FileSystemWatcherManager>();

            // Pipeline asincrona di indicizzazione (vedi docs-internal/md-tree-evolution2/passo-async-indexing.md)
            services.AddSingleton<Services.IndexingPipeline.IIndexingPipelineService, Services.IndexingPipeline.IndexingPipelineService>();

            // Shell execution for fenced code blocks (bash/sh/powershell/pwsh/cmd)
            services.AddSingleton<Services.Execution.ShellRegistry>();
            services.AddTransient<Services.Execution.ShellRunner>();

            // Add modern Git services with native credential management
            services.AddModernGitServices(_Configuration);
            
            // Configure LLamaSharp to use llama.cpp native libraries
            // Backends are downloaded on-demand to ~/MdExplorer-Models/llama-backends/
            // Fallback to LlamaCppOverride/ in app dir for dev/legacy scenarios
            var llamaCppVersion = _Configuration.GetValue<string>("LlamaCpp:Version") ?? "b8255";
            var llamaBackendPath = Features.Services.LlamaBackendService.ResolveBackendPath(llamaCppVersion);
            if (llamaBackendPath != null)
            {
                var llamaDllPath = Path.Combine(llamaBackendPath, "llama.dll");
                var mtmdDllPath = Path.Combine(llamaBackendPath, "mtmd.dll");
                if (File.Exists(llamaDllPath))
                {
                    var currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
                    Environment.SetEnvironmentVariable("PATH", llamaBackendPath + Path.PathSeparator + currentPath);

                    LLama.Native.NativeLibraryConfig.All
                        .WithLibrary(llamaDllPath, mtmdDllPath)
                        .WithAutoFallback(false)
                        .SkipCheck();
                }
            }

            // Add AI services
            services.AddHttpClient();
            services.AddSingleton<Features.Services.IModelDownloadService, Features.Services.ModelDownloadService>();
            services.AddSingleton<Features.Services.IAiConfigurationService, Features.Services.AiConfigurationService>();
            services.AddSingleton<Features.Services.IGpuDetectionService, Features.Services.GpuDetectionService>();
            services.AddSingleton<Features.Services.ILlamaBackendService, Features.Services.LlamaBackendService>();
            services.AddSingleton<Features.Services.IAiChatService, Features.Services.AiChatService>();
            services.AddSingleton<Features.Services.IGeminiApiService, Features.Services.GeminiApiService>();
            services.AddSingleton<Features.Services.AI.LocalLlamaProvider>();
            services.AddScoped<Services.IGitCommitAiService, Services.GitCommitAiService>();

            // Add Chat Interaction Logger for debugging tool calling
            services.AddSingleton<Features.Services.ChatInteractionLogger>();

            // Add multi-provider AI system
            // Registra tutti i provider disponibili (possono essere iniettati come IEnumerable<IAiProvider>)
            services.AddSingleton<IAiProvider, OpenAiProvider>();
            services.AddSingleton<IAiProvider, GeminiProvider>();
            services.AddSingleton<IAiProvider, CopilotCliProvider>();

            // Long-lived Copilot CLI ACP sessions (one persistent process per SignalR connection)
            services.AddSingleton<MdExplorer.Features.Services.AI.CopilotAcp.CopilotAcpSessionPool>();

            // Model discovery per ogni provider
            services.AddSingleton<IModelDiscoveryProvider, OpenAiModelDiscovery>();
            services.AddSingleton<IModelDiscoveryProvider, GeminiModelDiscovery>();
            services.AddSingleton<IModelDiscoveryProvider, CopilotCliModelDiscovery>();

            // Add AI Tool Calling services
            // PathValidator is now created dynamically by ToolExecutor with the current workspace root
            services.AddScoped<Abstractions.Services.IAiFileOperationNotifier, Services.AiFileOperationNotifier>();
            services.AddScoped<MdExplorer.bll.Services.AI.ToolExecutor>();

            // Add RAG services (base implementations - Premium module overrides IEmbeddingService)
            services.AddSingleton<Features.Services.IEmbeddingConfigService, Features.Services.EmbeddingConfigService>();
            services.AddSingleton<Abstractions.Services.IEmbeddingService, Features.Services.EmbeddingService>();
            services.AddSingleton<Abstractions.Services.IMarkdownChunkingService, Features.Services.AI.MarkdownChunkingService>();
            services.AddScoped<Abstractions.Services.IVectorSearchService, Features.Services.AI.VectorSearchService>();
            services.AddScoped<Abstractions.Services.IRagIndexingService, Services.RagIndexingService>();

            // Register both TocGenerationService and TocGenerationHubService
            services.AddScoped<Features.Services.TocGenerationService>();
            services.AddScoped<Features.Services.ITocGenerationService, Services.TocGenerationHubService>();

            // Register Team Chat services
            services.AddHttpClient("MdChat");
            // VpsChatStreamingService handles WebSocket connections for real-time cross-PC chat
            services.AddSingleton<Services.TeamChat.VpsChatStreamingService>();
            services.AddSingleton<Services.TeamChat.ITeamChatService, Services.TeamChat.TeamChatService>();

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 500L * 1024 * 1024; // 500 MB
            });

            services.AddSignalR(_ => _.KeepAliveInterval = TimeSpan.FromSeconds(20));

            // Try to load P2P Premium module if available
            Assembly? p2pAssembly = null;
            try
            {
                var p2pDllPath = Path.Combine(AppContext.BaseDirectory, "MdExplorer.P2P.Premium.dll");
                if (File.Exists(p2pDllPath))
                {
                    p2pAssembly = Assembly.LoadFrom(p2pDllPath);
                    var extensionsType = p2pAssembly.GetType("MdExplorer.P2P.Premium.DependencyInjection.ServiceCollectionExtensions");
                    var addMethod = extensionsType?.GetMethod("AddP2PServices");
                    addMethod?.Invoke(null, new object[] { services });
                    Console.WriteLine("[Startup] P2P Premium module loaded successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Startup] Warning: Could not load P2P Premium module: {ex.Message}");
            }

            // Try to load AI Premium module if available (overrides base AI stubs with LLamaSharp implementations)
            Assembly? aiPremiumAssembly = null;
            try
            {
                var aiPremiumDllPath = Path.Combine(AppContext.BaseDirectory, "MdExplorer.AI.Premium.dll");
                if (File.Exists(aiPremiumDllPath))
                {
                    aiPremiumAssembly = Assembly.LoadFrom(aiPremiumDllPath);
                    var extensionsType = aiPremiumAssembly.GetType("MdExplorer.AI.Premium.DependencyInjection.ServiceCollectionExtensions");
                    var addMethod = extensionsType?.GetMethod("AddAiPremiumServices");
                    addMethod?.Invoke(null, new object[] { services, null });
                    Console.WriteLine("[Startup] AI Premium module loaded successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Startup] Warning: Could not load AI Premium module: {ex.Message}");
            }

            var mvcBuilder = services.AddControllers(config =>
            {
                //config.Filters.Add<TransactionActionFilter>();
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.MaxDepth = 64;
                // Don't ignore properties with default values (like empty lists)
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
            });

            // Add P2P Premium controllers if assembly was loaded
            if (p2pAssembly != null)
            {
                mvcBuilder.AddApplicationPart(p2pAssembly);
                Console.WriteLine("[Startup] P2P Premium controllers registered");
            }

            // Add AI Premium controllers if assembly was loaded
            if (aiPremiumAssembly != null)
            {
                mvcBuilder.AddApplicationPart(aiPremiumAssembly);
                Console.WriteLine("[Startup] AI Premium controllers registered");
            }

        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env,
            IHostApplicationLifetime lifetime,
            ILogger<Startup> logger)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Add custom middleware to log only API requests (skip static files)
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.ToString().ToLower();
                
                // Log specifico per OpenFolderOnFileExplorer
                if (path.Contains("openfolder"))
                {
                    logger.LogInformation($"[MIDDLEWARE] OpenFolder request: {context.Request.Method} {context.Request.Path}");
                    if (context.Request.Method == "POST")
                    {
                        context.Request.EnableBuffering();
                        var body = await new System.IO.StreamReader(context.Request.Body).ReadToEndAsync();
                        context.Request.Body.Position = 0;
                        logger.LogInformation($"[MIDDLEWARE] OpenFolder body: {body}");
                    }
                }
                
                // Skip logging for static files
                bool isStaticFile = path.EndsWith(".js") || path.EndsWith(".css") || 
                                   path.EndsWith(".html") || path.EndsWith(".htm") ||
                                   path.EndsWith(".jpg") || path.EndsWith(".jpeg") || 
                                   path.EndsWith(".png") || path.EndsWith(".gif") ||
                                   path.EndsWith(".svg") || path.EndsWith(".ico") ||
                                   path.EndsWith(".woff") || path.EndsWith(".woff2") ||
                                   path.EndsWith(".ttf") || path.EndsWith(".eot") ||
                                   path.EndsWith(".map") || path.Contains("/jquery") ||
                                   path.Contains("/bootstrap") || path.Contains("/fontawesome");
                
                // Commentato per ridurre il rumore nei log durante debug
                // if (!isStaticFile)
                // {
                //     logger.LogInformation("HTTP {Method} {Path} from {RemoteIP}", 
                //         context.Request.Method, 
                //         context.Request.Path, 
                //         context.Connection.RemoteIpAddress);
                // }
                
                if (context.Request.Path.StartsWithSegments("/api/ModernGitToolbar"))
                {
                    logger.LogInformation("ModernGitToolbar request - Headers: {Headers}", 
                        string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}={h.Value}")));
                    
                    if (context.Request.Method == "POST" && context.Request.ContentLength > 0)
                    {
                        context.Request.EnableBuffering();
                        using var reader = new System.IO.StreamReader(context.Request.Body, encoding: System.Text.Encoding.UTF8, leaveOpen: true);
                        var body = await reader.ReadToEndAsync();
                        context.Request.Body.Position = 0;
                        logger.LogInformation("ModernGitToolbar POST Body: {Body}", body);
                    }
                }
                
                await next.Invoke();
                
                // Commentato per ridurre il rumore nei log durante debug
                // if (!isStaticFile)
                // {
                //     logger.LogInformation("HTTP {Method} {Path} responded with {StatusCode}", 
                //         context.Request.Method, 
                //         context.Request.Path, 
                //         context.Response.StatusCode);
                // }
            });

            // app.UseHttpsRedirection(); // Commented out to prevent warning when HTTPS is not configured for Kestrel

            app.UseRouting();
            
            app.UseStaticFiles();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.Map(

                    pattern: "/client2/{name:alpha}/{**anything}",
                    async context =>
                    {
                        context.Response.Redirect("/client2/index.html");
                    }
                    );
                endpoints.MapHub<MonitorMDHub>("/signalr/monitormd");
                endpoints.MapHub<AiChatHub>("/signalr/aichat");
                endpoints.MapHub<TeamChatHub>("/signalr/teamchat");

                // Map P2P hub if available (loaded dynamically)
                try
                {
                    var p2pDllPath = Path.Combine(AppContext.BaseDirectory, "MdExplorer.P2P.Premium.dll");
                    if (File.Exists(p2pDllPath))
                    {
                        var p2pAssembly = Assembly.LoadFrom(p2pDllPath);
                        var hubType = p2pAssembly.GetType("MdExplorer.P2P.Premium.Hubs.P2PTransferHub");
                        if (hubType != null)
                        {
                            // Use reflection to map the hub
                            var mapHubMethod = typeof(HubEndpointRouteBuilderExtensions)
                                .GetMethods()
                                .First(m => m.Name == "MapHub" && m.GetParameters().Length == 2);
                            var genericMapHub = mapHubMethod.MakeGenericMethod(hubType);
                            genericMapHub.Invoke(null, new object[] { endpoints, "/signalr/p2p" });
                            Console.WriteLine("[Startup] P2P SignalR hub mapped to /signalr/p2p");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Startup] Warning: Could not map P2P hub: {ex.Message}");
                }
            });

            //#if !DEBUG
            lifetime.ApplicationStarted.Register(
          () =>
          {
              DiscoverAddresses(app.ServerFeatures, logger);
          });
            //#endif
        }

        void DiscoverAddresses(IFeatureCollection features, ILogger<Startup> logger)
        {
            var addressFeature = features.Get<IServerAddressesFeature>();
            // Do something with the addresses
            foreach (var addresses in addressFeature.Addresses)
            {
                OpenUrl($"{addresses}/client2/index.html", logger);

                // Save port for MCP server discovery
                try
                {
                    var uri = new Uri(addresses);
                    var portDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MdExplorer");
                    Directory.CreateDirectory(portDir);
                    var portFile = Path.Combine(portDir, "port.txt");
                    System.IO.File.WriteAllText(portFile, uri.Port.ToString());
                    logger.LogInformation("[Startup] MCP port file written: {PortFile} = {Port}", portFile, uri.Port);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "[Startup] Could not write MCP port file");
                }
            }
        }

        private void OpenUrl(string url, ILogger<Startup> logger)
        {
            // Check if running from Electron - don't open browser if so
            var isElectron = Directory.GetCurrentDirectory().Contains(".mount_") || 
                            Directory.GetCurrentDirectory().Contains("app_service") ||
                            Environment.GetEnvironmentVariable("ELECTRON_RUN_AS_NODE") != null;
            
            if (isElectron)
            {
                logger.LogInformation($"Running from Electron, skipping browser launch. URL: {url}");
                return;
            }
            
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
#if DEBUG
                url = url.Replace("&", "^&");
                var processToStart = new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true };
                var processStarted = Process.Start(processToStart);
//#else
//                var currentApplicationPath = AppContext.BaseDirectory;
//                logger.LogInformation($"basedirectory: {currentApplicationPath}");
//                var command = $"{currentApplicationPath}Binaries\\ElectronMdExplorer\\ElectronMdExplorer \".\" \"{url}\"";
//                var processToStart = new ProcessStartInfo("cmd", $"/c start {command}") { CreateNoWindow = true };
#endif



            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }

        private void ProcessStarted_Exited(object sender, EventArgs e)
        {

        }

    }

}
