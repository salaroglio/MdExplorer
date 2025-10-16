namespace MdExplorer.Abstractions.Models
{
    public class GpuInfo
    {
        public bool IsNvidiaGpu { get; set; }
        public bool IsRtxCard { get; set; }
        public bool IsCudaAvailable { get; set; }
        public string Name { get; set; }
        public string DeviceId { get; set; }
        public long MemoryBytes { get; set; }
        public string FormattedMemory { get; set; }
        public int CudaVersion { get; set; }
        public string DriverVersion { get; set; }
        public string Status { get; set; }
    }
}
