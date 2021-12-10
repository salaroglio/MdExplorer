using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using MdExplorer.Features.Utilities;
using MdExplorer.Service;
using MdExplorer.Service.HostedServices;
using Microsoft.AspNetCore.Hosting;
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
            CreateHostBuilder(args).Build().Run();                                   
        }


        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            Startup.Args = args;
            
            var toReturn = Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {

                   webBuilder.UseUrls("https://127.0.0.1:0");
                   //webBuilder.UseUrls("https://localhost:0");

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

