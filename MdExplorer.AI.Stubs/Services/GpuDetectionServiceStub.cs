using MdExplorer.Abstractions.Models;
using MdExplorer.Abstractions.Services;

namespace MdExplorer.AI.Stubs.Services
{
    public class GpuDetectionServiceStub : IGpuDetectionService
    {
        public GpuInfo DetectGpu()
        {
            return new GpuInfo
            {
                IsNvidiaGpu = false,
                IsRtxCard = false,
                IsCudaAvailable = false,
                Name = "No GPU (Premium Required)",
                DeviceId = "stub",
                MemoryBytes = 0,
                FormattedMemory = "0 GB",
                CudaVersion = 0,
                DriverVersion = "N/A",
                Status = "GPU detection requires MdExplorer AI Premium"
            };
        }

        public int GetOptimalGpuLayerCount(long modelSizeBytes)
        {
            return 0;
        }
    }
}
