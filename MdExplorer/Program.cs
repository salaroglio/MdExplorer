using MdExplorer.Features.Utilities;
using MdExplorer.Service;
using MdExplorer.Service.HostedServices;
using MdExplorer.Utilities;
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
            try
            {
                // Check if running from Electron (AppImage or packaged app)
                var isElectron = Directory.GetCurrentDirectory().Contains(".mount_") || 
                                Directory.GetCurrentDirectory().Contains("app_service") ||
                                Environment.GetEnvironmentVariable("ELECTRON_RUN_AS_NODE") != null;
                
                if (!isElectron)
                {
                    // Setup file logging only when NOT running from Electron
                    var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                    Directory.CreateDirectory(logPath);
                    var logFile = Path.Combine(logPath, $"mdexplorer-startup-{DateTime.Now:yyyy-MM-dd}.log");
                    
                    using (var writer = new StreamWriter(logFile, append: true))
                    {
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting MdExplorer...");
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Arguments: {string.Join(" ", args ?? new string[0])}");
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Working Directory: {Directory.GetCurrentDirectory()}");
                    }
                }
                
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                // Check if running from Electron before trying to write logs
                var isElectron = Directory.GetCurrentDirectory().Contains(".mount_") || 
                                Directory.GetCurrentDirectory().Contains("app_service") ||
                                Environment.GetEnvironmentVariable("ELECTRON_RUN_AS_NODE") != null;
                
                if (!isElectron)
                {
                    // Log startup failures only when NOT running from Electron
                    var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                    Directory.CreateDirectory(logPath);
                    var logFile = Path.Combine(logPath, $"mdexplorer-crash-{DateTime.Now:yyyy-MM-dd}.log");
                    
                    File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] FATAL ERROR:\n{ex}\n\n");
                }
                
                throw;
            }
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
               .ConfigureLogging((hostingContext, logging) =>
               {
                   logging.ClearProviders();
                   logging.AddConsole();
                   logging.AddDebug();
                   
                   // Check if running from Electron
                   var isElectron = Directory.GetCurrentDirectory().Contains(".mount_") || 
                                   Directory.GetCurrentDirectory().Contains("app_service") ||
                                   Environment.GetEnvironmentVariable("ELECTRON_RUN_AS_NODE") != null;
                   
                   if (!isElectron)
                   {
                       // Add file logging only when NOT running from Electron
                       var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                       Directory.CreateDirectory(logPath);
                       var logFile = Path.Combine(logPath, $"mdexplorer-{DateTime.Now:yyyy-MM-dd}.log");
                       logging.AddFile(logFile);
                   }
               })
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseUrls(url);
                   webBuilder.UseStartup<Startup>();
               })
               .ConfigureServices(services =>
               {
                   services.AddHostedService<MonitorMDHostedService>();
                   services.AddHostedService<ApplicationInitializationService>();
               });
            ;
            return toReturn;
        }



    }
}
