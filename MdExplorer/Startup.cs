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
            
            // Add modern Git services with native credential management
            services.AddModernGitServices(_Configuration);
            
            // Add AI services
            services.AddHttpClient();
            services.AddSingleton<Features.Services.IModelDownloadService, Features.Services.ModelDownloadService>();
            services.AddSingleton<Features.Services.IAiConfigurationService, Features.Services.AiConfigurationService>();
            services.AddSingleton<Features.Services.IGpuDetectionService, Features.Services.GpuDetectionService>();
            services.AddSingleton<Features.Services.IAiChatService, Features.Services.AiChatService>();
            services.AddSingleton<Features.Services.IGeminiApiService, Features.Services.GeminiApiService>();
            services.AddScoped<Services.IGitCommitAiService, Services.GitCommitAiService>();
            
            // Register both TocGenerationService and TocGenerationHubService
            services.AddScoped<Features.Services.TocGenerationService>();
            services.AddScoped<Features.Services.ITocGenerationService, Services.TocGenerationHubService>();
            
            services.AddSignalR(_ => _.KeepAliveInterval = TimeSpan.FromSeconds(20));
            services.AddControllers(config =>
            {
                //config.Filters.Add<TransactionActionFilter>();
            }).AddJsonOptions(options => options.JsonSerializerOptions.MaxDepth = 64);

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
