using System;

namespace MdExplorer.AI.Abstractions.Models
{
    public class DownloadProgress
    {
        public string ModelId { get; set; }
        public long BytesDownloaded { get; set; }
        public long TotalBytes { get; set; }
        public double PercentComplete { get; set; }
        public double SpeedBytesPerSecond { get; set; }
        public TimeSpan EstimatedTimeRemaining { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
    }
}
