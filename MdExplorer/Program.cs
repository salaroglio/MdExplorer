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

        
        /// <summary>
        /// Vero quando il Service gira dentro l'app impacchettata Electron.
        /// </summary>
        private static bool IsElectron()
            => Directory.GetCurrentDirectory().Contains(".mount_")
               || Directory.GetCurrentDirectory().Contains("app_service")
               || Environment.GetEnvironmentVariable("ELECTRON_RUN_AS_NODE") != null;

        /// <summary>
        /// Cartella dove scrivere i log, oppure null se non se ne trova una scrivibile.
        /// <para>
        /// Prima i log sotto Electron erano semplicemente DISATTIVATI, e cioe' proprio nel caso in
        /// cui servono di piu': nell'app impacchettata non c'e' una console da guardare. Il motivo
        /// era che la working directory li' e' il mount di sola lettura dell'AppImage (o una
        /// cartella di sistema su Windows), quindi scriverci fallisce. La risposta giusta non e'
        /// rinunciare al log ma cambiare cartella: si scrive nell'AppData, che e' scrivibile per
        /// definizione perche' ospita gia' i database.
        /// </para>
        /// <para>
        /// Non basta creare la cartella per dire che ci si puo' scrivere: si tenta un file di
        /// prova. E se nessun candidato regge, si rinuncia ai log — mai impedire l'avvio per un
        /// log.
        /// </para>
        /// </summary>
        private static string ResolveLogDirectory()
        {
            foreach (var candidate in LogDirectoryCandidates())
            {
                if (string.IsNullOrWhiteSpace(candidate)) continue;
                try
                {
                    Directory.CreateDirectory(candidate);
                    var probe = Path.Combine(candidate, ".write-probe");
                    File.WriteAllText(probe, string.Empty);
                    File.Delete(probe);
                    return candidate;
                }
                catch
                {
                    // candidato non scrivibile: si prova il prossimo
                }
            }
            return null;
        }

        private static IEnumerable<string> LogDirectoryCandidates()
        {
            // Fuori da Electron la working directory resta la scelta storica, cosi' chi sviluppa
            // continua a trovare i log dove li ha sempre trovati.
            if (!IsElectron())
            {
                yield return Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            }

            string appData = null;
            try { appData = CrossPlatformPath.GetAppDataPath(); } catch { /* si prova il prossimo */ }
            if (!string.IsNullOrWhiteSpace(appData))
            {
                yield return Path.Combine(appData, "Logs");
            }

            yield return Path.Combine(Path.GetTempPath(), "MdExplorerLogs");
        }

        public static void Main(string[] args)
        {
            try
            {
                var logPath = ResolveLogDirectory();
                if (logPath != null)
                {
                    var logFile = Path.Combine(logPath, $"mdexplorer-startup-{DateTime.Now:yyyy-MM-dd}.log");

                    using (var writer = new StreamWriter(logFile, append: true))
                    {
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting MdExplorer...");
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Arguments: {string.Join(" ", args ?? new string[0])}");
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Working Directory: {Directory.GetCurrentDirectory()}");
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Electron: {IsElectron()}");
                    }

                    // Anche su stdout: e' la sola traccia visibile a chi avvia il Service a mano.
                    Console.WriteLine($"[MdExplorer] Log directory: {logPath}");
                }
                
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                // Un avvio fallito e' il momento in cui il log serve di piu': si scrive anche
                // (soprattutto) dentro Electron, dove non c'e' una console da guardare.
                try
                {
                    var logPath = ResolveLogDirectory();
                    if (logPath != null)
                    {
                        var logFile = Path.Combine(logPath, $"mdexplorer-crash-{DateTime.Now:yyyy-MM-dd}.log");
                        File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] FATAL ERROR:\n{ex}\n\n");
                    }
                }
                catch
                {
                    // Non si perde l'eccezione originale per colpa del log: si rilancia comunque.
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
                   
                   var logPath = ResolveLogDirectory();
                   if (logPath != null)
                   {
                       var logFile = Path.Combine(logPath, $"mdexplorer-{DateTime.Now:yyyy-MM-dd}.log");
                       logging.AddFile(logFile);
                   }
               })
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseUrls(url);
                   webBuilder.ConfigureKestrel(options =>
                   {
                       options.Limits.MaxRequestBodySize = 500L * 1024 * 1024; // 500 MB
                   });
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
