namespace MdExplorer.P2P.Premium.Models
{
    /// <summary>
    /// P2P service statistics
    /// </summary>
    public class P2PStats
    {
        public int Torrents { get; set; }
        public double DownloadSpeed { get; set; }
        public double UploadSpeed { get; set; }
        public double Progress { get; set; }
        public double Ratio { get; set; }
    }

    /// <summary>
    /// P2P service status response
    /// </summary>
    public class P2PStatus
    {
        public bool Enabled { get; set; }
        public bool HttpRunning { get; set; }
        public P2PStats? Stats { get; set; }
    }

    /// <summary>
    /// Health check response from P2P service
    /// </summary>
    public class HealthResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string? Version { get; set; }
    }

    /// <summary>
    /// Individual tracker status
    /// </summary>
    public class TrackerInfo
    {
        public string Url { get; set; } = string.Empty;
        public bool Reachable { get; set; }
        public bool Authenticated { get; set; }
        public string? Error { get; set; }
        public int? Latency { get; set; }
    }

    /// <summary>
    /// Overall tracker status summary
    /// </summary>
    public class TrackerOverallStatus
    {
        public bool Reachable { get; set; }
        public bool Authenticated { get; set; }
        public string Status { get; set; } = string.Empty; // "connected", "unauthorized", "unreachable"
    }

    /// <summary>
    /// Tracker connectivity status response
    /// </summary>
    public class TrackerStatusResponse
    {
        public bool HasToken { get; set; }
        public List<TrackerInfo> Trackers { get; set; } = new();
        public TrackerOverallStatus Overall { get; set; } = new();
    }

    /// <summary>
    /// Request for auto-restore all operation
    /// </summary>
    public class AutoRestoreAllRequest
    {
        public List<ProjectInfo> Projects { get; set; } = new();
    }

    /// <summary>
    /// Result of auto-restore all operation
    /// </summary>
    public class AutoRestoreAllResult
    {
        public int Total { get; set; }
        public int WithP2P { get; set; }
        public int Restored { get; set; }
        public List<string>? Errors { get; set; }
    }
}
