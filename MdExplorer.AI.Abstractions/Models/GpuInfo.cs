namespace MdExplorer.AI.Abstractions.Models
{
    public class GpuInfo
    {
        public bool IsNvidiaGpu { get; set; }
        public bool IsCudaAvailable { get; set; }
        public string Name { get; set; }
        public long TotalMemoryBytes { get; set; }
        public string FormattedMemory { get; set; }
        public string CudaVersion { get; set; }
        public string DriverVersion { get; set; }
        public string Status { get; set; }
    }
}
