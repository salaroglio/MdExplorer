namespace MdExplorer.P2P.Premium.Models
{
    /// <summary>
    /// Result of sharing a file via P2P
    /// </summary>
    public class ShareResult
    {
        public bool Success { get; set; } = true;
        public string InfoHash { get; set; } = string.Empty;
        public string MagnetUri { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public long Size { get; set; }
        public string? Error { get; set; }
        public List<FileInfo>? Files { get; set; }
    }

    /// <summary>
    /// Request to share a file
    /// </summary>
    public class ShareRequest
    {
        public string FilePath { get; set; } = string.Empty;
        public string? Name { get; set; }
    }

    /// <summary>
    /// Request to download via magnet link
    /// </summary>
    public class DownloadRequest
    {
        public string MagnetUri { get; set; } = string.Empty;
        public string? DestPath { get; set; }
    }

    /// <summary>
    /// Request to copy a file and share it via P2P
    /// The file will be copied to .p2pshare/files/ and a link will be appended to the document
    /// </summary>
    public class CopyAndShareRequest
    {
        /// <summary>
        /// Source path of the file to copy and share
        /// </summary>
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>
        /// Path to the markdown document where the P2P link will be appended
        /// </summary>
        public string DocumentPath { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to check if a file exists locally
    /// </summary>
    public class CheckFileRequest
    {
        public string Path { get; set; } = string.Empty;
    }
}
