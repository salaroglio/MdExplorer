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

        public static Task<int> _uiTask;

        
        public static void Main(string[] args)
        {            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            Startup.Args = args;

            string url = "http://127.0.0.1:0"; // Default to random port

            if (args != null && args.Length > 0)
            {
                if (int.TryParse(args[0], out int port) && port > 0 && port <= 65535)
                {
                    url = $"http://127.0.0.1:{port}";
                }
                // Optional: Add more sophisticated argument parsing here, e.g., --port <number>
                // For now, we assume the first argument, if an integer, is the port.
            }

            var toReturn = Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseUrls(url);
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
