using MdExplorer.Hubs;
using MdExplorer.Service.HostedServices;
using MdExplorer.Service.Models;
using Ad.Tools.Dal;
using Ad.Tools.Dal.Concrete;
using Ad.Tools.FluentMigrator;
using MdExplorer.Migrations;
using MDExplorer.DataAccess.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using static Ad.Tools.FluentMigrator.FluentMigratorDI;
using Ad.Tools.FluentMigrator.Interfaces;
using Microsoft.Extensions.FileProviders;
//using MdExplorer.Service.Filters;

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
            services.Configure<MdExplorerAppSettings>(_Configuration.GetSection(MdExplorerAppSettings.MdExplorer));
            services = ConfigFileSystemWatchers(services);                        
            var appdata = Environment.GetEnvironmentVariable("LocalAppData");
            var databasePath = $@"Data Source = {appdata}\MdExplorer.db";
            services.AddDalFeatures(typeof(SettingsMap).Assembly,
                                    new DatabaseSQLite(),
                                    databasePath);

            services.AddSignalR();
            services.AddControllers(config =>
            {
                //config.Filters.Add<TransactionActionFilter>();
            });
            
        }

        private IServiceCollection ConfigFileSystemWatchers(IServiceCollection services)
        {
            var defaultPath = @".\Documents";
            if (Args.Length > 0)
            {
                if (File.Exists(Args[0]))
                {
                    var directoryName = Path.GetDirectoryName(Args[0]);
                    defaultPath = directoryName;
                }
            }
            LogStartup(defaultPath);
            services.AddSingleton<FileSystemWatcher>(new FileSystemWatcher { Path = defaultPath });
            var _fileSystemWatcher = new FileSystemWatcher { Path = defaultPath };
            services.AddSingleton<FileSystemWatcher>(_fileSystemWatcher);
            return services;
        }

        private void LogStartup(string defaultPath)
        {
            var createStartupLog = Convert.ToBoolean( _Configuration.GetSection(@"MdExplorer:CreateStartupLog").Value) ;            
            if (createStartupLog)
            {
                var test = File.CreateText("startup.txt");
                test.WriteLine($@"args: {Args[0]}");
                test.WriteLine($"defaultPath: {defaultPath}");
                test.Flush();
                test.Close();
            }            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();            
            var assembly = Assembly.Load(new AssemblyName("MdExplorer.Service"));

#if !DEBUG
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new EmbeddedFileProvider(
                assembly: Assembly.Load(new AssemblyName("MdExplorer.Service")),
                baseNamespace: "MdExplorer.Service.wwwroot")
            });
#else
            app.UseStaticFiles();
#endif
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
            });
#if !DEBUG
            lifetime.ApplicationStarted.Register(
          () =>
          {
              DiscoverAddresses(app.ServerFeatures);
          });
#endif
         
        }

        void DiscoverAddresses(IFeatureCollection features)
        {
            var addressFeature = features.Get<IServerAddressesFeature>();
            // Do something with the addresses
            foreach (var addresses in addressFeature.Addresses)
            {
                OpenUrl($"{addresses}/client2/index.html");
            }
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    var processToStart = new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true };
                    var processStarted = Process.Start(processToStart);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        private void ProcessStarted_Exited(object sender, EventArgs e)
        {

        }

    }

}
