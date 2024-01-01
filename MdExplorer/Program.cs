using MdExplorer.Features.Utilities;
using MdExplorer.Service;
using MdExplorer.Service.HostedServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer
{
    public class Program
    {

        //public static Task<int> _uiTask;

        
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

