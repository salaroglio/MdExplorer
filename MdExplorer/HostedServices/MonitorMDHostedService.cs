using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Service.HostedServices
{
    /// <summary>
    /// Hosted service that handles console closing events to notify all clients.
    /// NOTE: FileSystemWatcher events are now handled by FileSystemWatcherManager for multi-client support.
    /// This service no longer monitors file changes directly.
    /// </summary>
    public class MonitorMDHostedService : IHostedService
    {
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly ILogger<MonitorMDHostedService> _logger;

        public MonitorMDHostedService(
                IHubContext<MonitorMDHub> hubContext,
                ILogger<MonitorMDHostedService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;

            // console closing management, send back closing server to the angular client
#if WINDOWS_FORMS_AVAILABLE
            // Windows-specific console control handler
            handler = new ConsoleEventDelegate(SendExitToAngular);
            SetConsoleCtrlHandler(handler, true);
#else
            // On Linux/Mac, use AppDomain.ProcessExit or Console.CancelKeyPress
            Console.CancelKeyPress += (sender, e) =>
            {
                _hubContext.Clients.All.SendAsync("consoleClosed");
            };
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                _hubContext.Clients.All.SendAsync("consoleClosed");
            };
#endif
        }

        private bool SendExitToAngular(int eventType)
        {
            if (eventType == 2)
            {
                _hubContext.Clients.All.SendAsync("consoleClosed");
            }
            return false;
        }

#if WINDOWS_FORMS_AVAILABLE
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
                                               // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
#endif

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MonitorMDHostedService started (console closing handler only)");
            _logger.LogInformation("FileSystemWatcher events are handled by FileSystemWatcherManager for multi-client support");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
