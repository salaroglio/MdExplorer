using System.Text.Json;
using MdExplorer.P2P.Premium.Models;

namespace MdExplorer.P2P.Premium.Services
{
    /// <summary>
    /// Interface for P2P file sharing service.
    /// Communicates with the Electron P2P plugin via HTTP API.
    /// </summary>
    public interface IP2PService
    {
        /// <summary>
        /// Check if the P2P service is available and running
        /// </summary>
        Task<bool> IsAvailableAsync();

        /// <summary>
        /// Get P2P service health status
        /// </summary>
        Task<HealthResponse?> GetHealthAsync();

        /// <summary>
        /// Get P2P service statistics
        /// </summary>
        Task<P2PStats?> GetStatsAsync();

        /// <summary>
        /// Get all active transfers
        /// </summary>
        Task<List<TransferInfo>> GetTransfersAsync();

        /// <summary>
        /// Get a specific transfer by info hash
        /// </summary>
        Task<TransferInfo?> GetTransferAsync(string infoHash);

        /// <summary>
        /// Share a file via P2P
        /// </summary>
        Task<ShareResult?> ShareFileAsync(string filePath, string? name = null);

        /// <summary>
        /// Download a file from magnet link
        /// </summary>
        Task<ShareResult?> DownloadAsync(string magnetUri, string? destPath = null);

        /// <summary>
        /// Pause a transfer
        /// </summary>
        Task<bool> PauseTransferAsync(string infoHash);

        /// <summary>
        /// Resume a transfer
        /// </summary>
        Task<bool> ResumeTransferAsync(string infoHash);

        /// <summary>
        /// Stop and remove a transfer
        /// </summary>
        Task<bool> StopTransferAsync(string infoHash, bool deleteFiles = false);

        /// <summary>
        /// Parse a magnet URI to get info
        /// </summary>
        Task<object?> ParseMagnetAsync(string magnetUri);

        /// <summary>
        /// Check tracker connectivity status
        /// </summary>
        Task<TrackerStatusResponse?> GetTrackerStatusAsync();

        /// <summary>
        /// Auto-restore seeding for all projects with P2P metadata.
        /// Scans each project for .p2pshare/metadata.json and restores seeding for all files.
        /// </summary>
        Task<AutoRestoreAllResult?> AutoRestoreAllAsync(List<ProjectInfo> projects);

        /// <summary>
        /// Subscribe to real-time P2P events via Server-Sent Events (SSE).
        /// Events include: peer-connected, upload-activity
        /// </summary>
        /// <param name="onEvent">Callback for each event received (eventType, data)</param>
        /// <param name="ct">Cancellation token to stop subscription</param>
        Task SubscribeToEventsAsync(Action<string, JsonElement> onEvent, CancellationToken ct);
    }
}
