using MdExplorer.Models;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Services.FileSystemWatcherManager;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Hubs
{
    public class MonitorMDHub:Hub
    {
        private readonly IDatabaseManager _databaseManager;
        private readonly IFileSystemWatcherManager _fileSystemWatcherManager;
        private readonly ILogger<MonitorMDHub> _logger;

        public MonitorMDHub(
            IDatabaseManager databaseManager,
            IFileSystemWatcherManager fileSystemWatcherManager,
            ILogger<MonitorMDHub> logger,
            MdExplorer.Services.MarkDiagram.IMarkDiagramExplainService diagramExplain)
        {
            _databaseManager = databaseManager;
            _fileSystemWatcherManager = fileSystemWatcherManager;
            _logger = logger;
            _diagramExplain = diagramExplain;
        }

        /// <summary>The diagram sessions of a closed connection go with it (sprint 2026-09-29-Motore-LLM-Unico, D8).</summary>
        private readonly MdExplorer.Services.MarkDiagram.IMarkDiagramExplainService _diagramExplain;

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

        /// <summary>
        /// Called when a client disconnects from the hub.
        /// Cleans up database contexts and FileSystemWatcher instances for this connection.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;

            try
            {
                _logger?.LogInformation($"🔌 Client disconnecting: {connectionId}");

                // A CLI process per live diagram session: not left running after the page is gone.
                try { await _diagramExplain.ForgetConnectionAsync(connectionId); }
                catch (Exception ex) { _logger?.LogWarning(ex, "[MonitorMDHub] chiusura delle sessioni dei diagrammi fallita"); }

                // Unregister FileSystemWatcher for this connection
                if (_fileSystemWatcherManager.HasWatcher(connectionId))
                {
                    _fileSystemWatcherManager.UnregisterWatcher(connectionId);
                    _logger?.LogInformation($"✅ FileSystemWatcher unregistered for connection {connectionId}");
                }

                // Unregister database contexts for this connection
                if (_databaseManager.HasConnection(connectionId))
                {
                    _databaseManager.UnregisterConnection(connectionId);
                    _logger?.LogInformation($"✅ Database contexts unregistered for connection {connectionId}");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"❌ Error during disconnect cleanup for connection {connectionId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        //public async Task GetConnectionId()
        //    => await Clients.Client(Context.ConnectionId).SendAsync("getconnectionid", Context.ConnectionId);

    }
}
