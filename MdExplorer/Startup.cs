using MdExplorer.Hubs;
using MdExplorer.Service.Models;
using Ad.Tools.Dal;
using Ad.Tools.Dal.Concrete;
using MDExplorer.DataAccess.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using MdExplorer.Features;
using MdExplorer.Service;
using MdExplorer.Abstractions.DB;
using MdExplorer.DataAccess.Engine;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.FileProviders;
using System.Linq;
using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using MdExplorer.Service.Automapper.RefactoringFilesController;
using Ad.Tools.FluentMigrator.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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

            // app.UseHttpsRedirection(); // Commented out to prevent warning when HTTPS is not configured for Kestrel

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
