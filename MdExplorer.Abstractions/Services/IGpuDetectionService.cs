using MdExplorer.Abstractions.Models;

namespace MdExplorer.Abstractions.Services
{
    /// <summary>
    /// Service for detecting and reporting GPU information
    /// </summary>
    public interface IGpuDetectionService
    {
        /// <summary>
        /// Detect available GPU and return information
        /// </summary>
        GpuInfo DetectGpu();
        
        /// <summary>
        /// Calculate optimal GPU layer count for a model
        /// </summary>
        int GetOptimalGpuLayerCount(long modelSizeBytes);
    }
}
