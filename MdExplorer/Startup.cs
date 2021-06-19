using MdExplorer.Hubs;
using MdExplorer.Service.HostedServices;
using MdExplorer.Service.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MdExplorer
{
    public class Startup
    {
        public static string[] Args;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
         
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services = ConfigFileSystemWatchers(services);
            services.AddHostedService<MonitorMDHostedService>();
            services.AddSignalR();
            services.AddControllers();
            services.Configure<MdExplorerAppSettings>(Configuration.GetSection(MdExplorerAppSettings.MdExplorer));
        }

        private IServiceCollection ConfigFileSystemWatchers(IServiceCollection services)
        {
            var defaultPath = @".\Documents";
            if (Args.Length > 0)
            {
                defaultPath = Path.GetDirectoryName(Args[0]);
            }
            var _fileSystemWatcher = new FileSystemWatcher { Path = defaultPath };
            
            services.AddSingleton<FileSystemWatcher>(_fileSystemWatcher);
            return services;
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
           
            //app.UseCors("CorsPolicy");
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
                    var processStarted= Process.Start(processToStart);                                     
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
