using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MdExplorer.P2P.Premium.Hubs;

namespace MdExplorer.P2P.Premium.Services
{
    /// <summary>
    /// Background service that bridges P2P events from the Electron plugin (via SSE)
    /// to SignalR clients in the frontend.
    /// </summary>
    public class P2PEventBridgeService : BackgroundService
    {
        private readonly IP2PService _p2pService;
        private readonly IHubContext<P2PTransferHub> _hubContext;
        private readonly ILogger<P2PEventBridgeService> _logger;

        public P2PEventBridgeService(
            IP2PService p2pService,
            IHubContext<P2PTransferHub> hubContext,
            ILogger<P2PEventBridgeService> logger)
        {
            _p2pService = p2pService;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[P2PEventBridge] Starting event bridge service...");

            // Wait a bit for the P2P service to be available
            await Task.Delay(5000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Check if P2P service is available
                    var isAvailable = await _p2pService.IsAvailableAsync();
                    if (!isAvailable)
                    {
                        _logger.LogDebug("[P2PEventBridge] P2P service not available, retrying in 10s...");
                        await Task.Delay(10000, stoppingToken);
                        continue;
                    }

                    _logger.LogInformation("[P2PEventBridge] Connecting to SSE events...");

                    // Subscribe to SSE events and forward them to SignalR
                    await _p2pService.SubscribeToEventsAsync(
                        (eventType, data) => HandleEvent(eventType, data),
                        stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Expected when stopping
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[P2PEventBridge] Error in event bridge, retrying in 5s...");
                    await Task.Delay(5000, stoppingToken);
                }
            }

            _logger.LogInformation("[P2PEventBridge] Event bridge service stopped.");
        }

        private void HandleEvent(string eventType, JsonElement data)
        {
            try
            {
                switch (eventType)
                {
                    case "peer-connected":
                        HandlePeerConnected(data);
                        break;

                    case "upload-activity":
                        HandleUploadActivity(data);
                        break;

                    case "connected":
                        _logger.LogInformation("[P2PEventBridge] SSE connection established");
                        break;

                    default:
                        _logger.LogDebug("[P2PEventBridge] Unknown event type: {EventType}", eventType);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[P2PEventBridge] Error handling event {EventType}", eventType);
            }
        }

        private void HandlePeerConnected(JsonElement data)
        {
            var infoHash = data.GetProperty("infoHash").GetString() ?? "";
            var peerAddress = data.GetProperty("peerAddress").GetString() ?? "unknown";
            var numPeers = data.GetProperty("numPeers").GetInt32();
            var torrentName = data.GetProperty("torrentName").GetString() ?? "";

            _logger.LogInformation(
                "[P2PEventBridge] Peer connected: {PeerAddress} to {TorrentName} ({NumPeers} peers)",
                peerAddress, torrentName, numPeers);

            // Fire and forget - send to SignalR clients
            _ = _hubContext.SendPeerConnected(infoHash, peerAddress, numPeers, torrentName);
        }

        private void HandleUploadActivity(JsonElement data)
        {
            var infoHash = data.GetProperty("infoHash").GetString() ?? "";
            var bytes = data.GetProperty("bytes").GetInt64();
            var uploadSpeed = data.GetProperty("uploadSpeed").GetDouble();
            var uploaded = data.GetProperty("uploaded").GetInt64();
            var numPeers = data.GetProperty("numPeers").GetInt32();
            var torrentName = data.GetProperty("torrentName").GetString() ?? "";

            _logger.LogDebug(
                "[P2PEventBridge] Upload activity: {TorrentName} - {Speed}/s to {NumPeers} peers",
                torrentName, FormatBytes(uploadSpeed), numPeers);

            // Fire and forget - send to SignalR clients
            _ = _hubContext.SendUploadActivity(infoHash, bytes, uploadSpeed, uploaded, numPeers, torrentName);
        }

        private static string FormatBytes(double bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes /= 1024;
            }
            return $"{bytes:0.##} {sizes[order]}";
        }
    }
}
