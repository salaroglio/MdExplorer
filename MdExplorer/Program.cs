using MdExplorer.Features.Utilities;
using MdExplorer.Service;
using MdExplorer.Service.HostedServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MdExplorer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            InitializeApplicationPreparingDb(args);
            CreateHostBuilder(args).Build().Run();

        }

        private static void InitializeApplicationPreparingDb(string[] args)
        {
            //var appdata = Environment.GetEnvironmentVariable("LocalAppData");
            //var currentDb = $@"{appdata}\MdExplorer.db";
            //if (!File.Exists(currentDb))
            //{
            //    FileUtil.ExtractResFile("MdExplorer.Service.MdExplorer.db", currentDb);
            //}

            //var currentDirectory = Path.GetDirectoryName(args[0]);
            //var hash = Helper.HGetHashString(currentDirectory);
            //var currentEngineDb = $@"{appdata}\MdEngine_{hash}.db";            
            //if (!File.Exists(currentEngineDb))
            //{
            //    FileUtil.ExtractResFile("MdExplorer.Service.MdEngine.db", currentEngineDb);
            //}


        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            Startup.Args = args;
            
            var toReturn = Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {                 
#if !DEBUG
                   webBuilder.UseUrls("https://127.0.0.1:0");
#endif
                   webBuilder.UseStartup<Startup>();
               })
               .ConfigureServices(services =>
               {
                   services.AddHostedService<MonitorMDHostedService>();
                   services.AddHostedService<MigratorHostedService>();
                   services.AddHostedService<MigratorEngineHostedService>();
               });
            ;
            return toReturn;
        }
           
        

    }
}

