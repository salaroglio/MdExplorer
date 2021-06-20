using Ad.Tools.Dal;
using Ad.Tools.Dal.Concrete;
using Ad.Tools.FluentMigrator;
using MdExplorer.HubConfig;
using MdExplorer.Migrations;
using MdExplorer.Service.Middleware;
using MDExplorer.DataAccess.Mapping;
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
using static Ad.Tools.FluentMigrator.FluentMigratorDI;

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
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder => builder
                .WithOrigins("http://localhost:4200")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            });
            services.AddSignalR();

            services.AddControllers();
            var defaultPath = @".\Documents";
            if (Args.Length>0)
            {
                defaultPath = Path.GetDirectoryName(Args[0]);
            }
            services.AddSingleton<FileSystemWatcher>(new FileSystemWatcher {Path= defaultPath });
            var appdata = Environment.GetEnvironmentVariable("LocalAppData");
            var databasePath = @$"Data Source={appdata}\MdExplorer.db";
            services.AddFluentMigratorFeatures(databasePath,
                                                DatabaseConfigurations.ConfigureSQLite, 
                                                typeof(Migration0).Assembly);
            services.AddDalFeatures(typeof(SettingsMap).Assembly,
                                    new DatabaseSQLite(),
                                    databasePath);

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
           
            app.UseCors("CorsPolicy");
            var assembly = Assembly.Load(new AssemblyName("MdExplorer.Service"));

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new EmbeddedFileProvider(
                assembly: Assembly.Load(new AssemblyName("MdExplorer.Service")),
                baseNamespace: "MdExplorer.Service.wwwroot")
            });

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.CreateApplicationBuilder()
                .UseMiddleware< ServerAddressMiddleware>().Build();

                endpoints.Map(

                    pattern: "/client2/{name:alpha}/{**anything}",
                    async context =>
                    {
                        context.Response.Redirect("/client2/index.html");
                    }
                    );
                endpoints.MapHub<ChartHub>("/chart");
            });
            lifetime.ApplicationStarted.Register(
          () =>
          {
              DiscoverAddresses(app.ServerFeatures);
          });
            var currentDirectory = Directory.GetCurrentDirectory();
            logger.LogInformation($"Current Directory:{currentDirectory}");
            //env.ContentRootPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var p = System.Reflection.Assembly.GetEntryAssembly().Location;
            p = p.Substring(0, p.LastIndexOf(@"\") + 1);
            logger.LogInformation($"Location:{p}" );
            logger.LogInformation($"CurrontRootPath:{env.ContentRootPath}");
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            logger.LogInformation($"assemblyPath:{assemblyPath}");
            var lastchance = Path.GetDirectoryName(new System.Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            logger.LogInformation($"lastchance:{lastchance}");

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
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
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
    }

}
