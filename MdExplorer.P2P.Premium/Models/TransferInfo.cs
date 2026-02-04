namespace MdExplorer.P2P.Premium.Models
{
    /// <summary>
    /// Information about a P2P transfer (upload or download)
    /// </summary>
    public class TransferInfo
    {
        public string InfoHash { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "seeding" or "downloading"
        public string Name { get; set; } = string.Empty;
        public double Progress { get; set; }
        public double DownloadSpeed { get; set; }
        public double UploadSpeed { get; set; }
        public int NumPeers { get; set; }
        public long Size { get; set; }
        public long Downloaded { get; set; }
        public long Uploaded { get; set; }
        public double? TimeRemaining { get; set; }
        public bool Paused { get; set; }
        public DateTime StartedAt { get; set; }
        public string? Path { get; set; }
        public string? MagnetUri { get; set; }
        public List<FileInfo>? Files { get; set; }
    }

    /// <summary>
    /// File information within a torrent
    /// </summary>
    public class FileInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }
        public double? Progress { get; set; }
    }
}
