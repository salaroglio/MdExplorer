using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MdExplorer.P2P.Premium.Models;
using MdExplorer.P2P.Premium.Services;

namespace MdExplorer.P2P.Premium.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time P2P transfer updates.
    /// Clients can subscribe to transfer progress, completion, and error events.
    /// </summary>
    public class P2PTransferHub : Hub
    {
        private readonly IP2PService _p2pService;
        private readonly ILogger<P2PTransferHub> _logger;

        public P2PTransferHub(IP2PService p2pService, ILogger<P2PTransferHub> logger)
        {
            _p2pService = p2pService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("P2P client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("P2P client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Get current P2P service status
        /// </summary>
        public async Task<P2PStatus> GetStatus()
        {
            var isAvailable = await _p2pService.IsAvailableAsync();
            var stats = isAvailable ? await _p2pService.GetStatsAsync() : null;

            return new P2PStatus
            {
                Enabled = isAvailable,
                HttpRunning = isAvailable,
                Stats = stats
            };
        }

        /// <summary>
        /// Get all active transfers
        /// </summary>
        public async Task<List<TransferInfo>> GetTransfers()
        {
            return await _p2pService.GetTransfersAsync();
        }

        /// <summary>
        /// Subscribe to transfer updates for a specific transfer
        /// </summary>
        public async Task SubscribeToTransfer(string infoHash)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"transfer-{infoHash}");
            _logger.LogDebug("Client {ConnectionId} subscribed to transfer {InfoHash}",
                Context.ConnectionId, infoHash);
        }

        /// <summary>
        /// Unsubscribe from transfer updates
        /// </summary>
        public async Task UnsubscribeFromTransfer(string infoHash)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"transfer-{infoHash}");
            _logger.LogDebug("Client {ConnectionId} unsubscribed from transfer {InfoHash}",
                Context.ConnectionId, infoHash);
        }

        /// <summary>
        /// Subscribe to all transfer updates
        /// </summary>
        public async Task SubscribeToAllTransfers()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "all-transfers");
            _logger.LogDebug("Client {ConnectionId} subscribed to all transfers", Context.ConnectionId);
        }

        /// <summary>
        /// Unsubscribe from all transfer updates
        /// </summary>
        public async Task UnsubscribeFromAllTransfers()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "all-transfers");
            _logger.LogDebug("Client {ConnectionId} unsubscribed from all transfers", Context.ConnectionId);
        }
    }

    /// <summary>
    /// Extension methods for sending P2P events to SignalR clients
    /// </summary>
    public static class P2PTransferHubExtensions
    {
        /// <summary>
        /// Send transfer progress update to subscribed clients
        /// </summary>
        public static async Task SendTransferProgress(
            this IHubContext<P2PTransferHub> hubContext,
            TransferInfo transfer)
        {
            // Send to specific transfer subscribers
            await hubContext.Clients
                .Group($"transfer-{transfer.InfoHash}")
                .SendAsync("TransferProgress", transfer);

            // Send to all-transfers subscribers
            await hubContext.Clients
                .Group("all-transfers")
                .SendAsync("TransferProgress", transfer);
        }

        /// <summary>
        /// Send transfer complete event to subscribed clients
        /// </summary>
        public static async Task SendTransferComplete(
            this IHubContext<P2PTransferHub> hubContext,
            TransferInfo transfer)
        {
            await hubContext.Clients
                .Group($"transfer-{transfer.InfoHash}")
                .SendAsync("TransferComplete", transfer);

            await hubContext.Clients
                .Group("all-transfers")
                .SendAsync("TransferComplete", transfer);
        }

        /// <summary>
        /// Send transfer error event to subscribed clients
        /// </summary>
        public static async Task SendTransferError(
            this IHubContext<P2PTransferHub> hubContext,
            string infoHash,
            string error)
        {
            await hubContext.Clients
                .Group($"transfer-{infoHash}")
                .SendAsync("TransferError", new { infoHash, error });

            await hubContext.Clients
                .Group("all-transfers")
                .SendAsync("TransferError", new { infoHash, error });
        }

        /// <summary>
        /// Send peer connected event to all subscribers.
        /// Called when a remote peer connects to download from us.
        /// </summary>
        public static async Task SendPeerConnected(
            this IHubContext<P2PTransferHub> hubContext,
            string infoHash,
            string peerAddress,
            int numPeers,
            string torrentName)
        {
            var data = new
            {
                infoHash,
                peerAddress,
                numPeers,
                torrentName,
                timestamp = DateTime.UtcNow
            };

            await hubContext.Clients
                .Group($"transfer-{infoHash}")
                .SendAsync("PeerConnected", data);

            await hubContext.Clients
                .Group("all-transfers")
                .SendAsync("PeerConnected", data);
        }

        /// <summary>
        /// Send upload activity event to all subscribers.
        /// Called periodically when actively uploading to peers.
        /// </summary>
        public static async Task SendUploadActivity(
            this IHubContext<P2PTransferHub> hubContext,
            string infoHash,
            long bytes,
            double uploadSpeed,
            long totalUploaded,
            int numPeers,
            string torrentName)
        {
            var data = new
            {
                infoHash,
                bytes,
                uploadSpeed,
                totalUploaded,
                numPeers,
                torrentName,
                timestamp = DateTime.UtcNow
            };

            await hubContext.Clients
                .Group($"transfer-{infoHash}")
                .SendAsync("UploadActivity", data);

            await hubContext.Clients
                .Group("all-transfers")
                .SendAsync("UploadActivity", data);
        }
    }
}
