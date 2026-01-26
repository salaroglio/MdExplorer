namespace MdExplorer.P2P.Premium.Models
{
    /// <summary>
    /// P2P service statistics
    /// </summary>
    public class P2PStats
    {
        public int Torrents { get; set; }
        public long DownloadSpeed { get; set; }
        public long UploadSpeed { get; set; }
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
}
