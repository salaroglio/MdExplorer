using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using MdExplorer.Features.Utilities;
using MdExplorer.Service;
using MdExplorer.Service.HostedServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SampleWebView.Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MdExplorer
{
    public class Program
    {
        private static Task _backgroundHostTask;
        public static Task<int> _uiTask;

        public static void Main(string[] args)
        {
            ExtractServer();
            CreateHostBuilder(args).Build().Run();
        }

        private static void ExtractServer()
        {
            var plantumlPath = AppContext.BaseDirectory + "plantuml.jar";
            if (!File.Exists(plantumlPath))
            {
                FileUtil.ExtractResFile("MdExplorer.Service.Binaries.plantuml.jar", plantumlPath);
            }
            var dotExePath  = AppContext.BaseDirectory + "dot.exe";
            if (!File.Exists(dotExePath))
            {
                FileUtil.ExtractResFile("MdExplorer.Service.Binaries.GraphWiz.binaries.dot.exe", dotExePath);
            }
            
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            Startup.Args = args;

            var toReturn = Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseUrls("http://127.0.0.1:0");
                   webBuilder.UseStartup<Startup>();
               })
               .ConfigureServices(services =>
               {
                   services.AddHostedService<MonitorMDHostedService>();
               });
            ;
            return toReturn;
        }



    }
}

