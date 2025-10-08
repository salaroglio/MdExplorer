using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services
{
    public interface IGpuDetectionService
    {
        GpuInfo DetectGpu();
        bool IsNvidiaRtxAvailable();
        bool IsCudaAvailable();
        int GetOptimalGpuLayerCount(long modelSizeBytes);
    }

    public class GpuInfo
    {
        public bool IsNvidiaGpu { get; set; }
        public bool IsRtxCard { get; set; }
        public string Name { get; set; }
        public string DeviceId { get; set; }
        public long MemoryBytes { get; set; }
        public string DriverVersion { get; set; }
        public int CudaVersion { get; set; }
        public bool IsCudaAvailable { get; set; }
        public string Status { get; set; }
        
        public string FormattedMemory => FormatBytes(MemoryBytes);
        
        private string FormatBytes(long bytes)
        {
            if (bytes <= 0) return "0 GB";
            var gb = bytes / (1024.0 * 1024.0 * 1024.0);
            return $"{gb:F2} GB";
        }
    }

    public class GpuDetectionService : IGpuDetectionService
    {
        private readonly ILogger<GpuDetectionService> _logger;
        private GpuInfo _cachedGpuInfo;
        private DateTime _lastDetection = DateTime.MinValue;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        public GpuDetectionService(ILogger<GpuDetectionService> logger)
        {
            _logger = logger;
        }

        public GpuInfo DetectGpu()
        {
            // Return cached info if still valid
            if (_cachedGpuInfo != null && DateTime.Now - _lastDetection < _cacheExpiration)
            {
                return _cachedGpuInfo;
            }

            var gpuInfo = new GpuInfo();

            try
            {
                // Only attempt WMI detection on Windows
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    DetectGpuWindows(gpuInfo);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    DetectGpuLinux(gpuInfo);
                }
                else
                {
                    _logger.LogInformation("GPU detection not supported on this platform");
                    gpuInfo.Status = "Unsupported platform";
                }

                // Check CUDA availability
                gpuInfo.IsCudaAvailable = CheckCudaAvailability(gpuInfo);
                
                _cachedGpuInfo = gpuInfo;
                _lastDetection = DateTime.Now;
                
                LogGpuInfo(gpuInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting GPU");
                gpuInfo.Status = $"Detection error: {ex.Message}";
            }

            return gpuInfo;
        }

        private void DetectGpuWindows(GpuInfo gpuInfo)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        // Check if this is an active controller
                        if (mo["CurrentBitsPerPixel"] != null && mo["CurrentHorizontalResolution"] != null)
                        {
                            string name = mo["Name"]?.ToString() ?? "";
                            
                            // Check for NVIDIA GPU
                            if (name.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase))
                            {
                                gpuInfo.IsNvidiaGpu = true;
                                gpuInfo.Name = name;
                                gpuInfo.DeviceId = mo["DeviceID"]?.ToString() ?? "";
                                gpuInfo.DriverVersion = mo["DriverVersion"]?.ToString() ?? "";
                                
                                // Check if it's an RTX card
                                if (name.Contains("RTX", StringComparison.OrdinalIgnoreCase) || 
                                    name.Contains("RTX", StringComparison.OrdinalIgnoreCase))
                                {
                                    gpuInfo.IsRtxCard = true;
                                }
                                
                                // Get memory (AdapterRAM is in bytes)
                                if (mo["AdapterRAM"] != null && uint.TryParse(mo["AdapterRAM"].ToString(), out uint adapterRam))
                                {
                                    gpuInfo.MemoryBytes = adapterRam;
                                }
                                
                                gpuInfo.Status = "NVIDIA GPU detected";
                                
                                // If we found an NVIDIA GPU, stop searching
                                break;
                            }
                        }
                    }
                }
                
                if (!gpuInfo.IsNvidiaGpu)
                {
                    gpuInfo.Status = "No NVIDIA GPU detected";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting GPU via WMI");
                gpuInfo.Status = "WMI detection failed";
            }
        }

        private void DetectGpuLinux(GpuInfo gpuInfo)
        {
            // On Linux, we can try to use nvidia-smi if available
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "nvidia-smi",
                        Arguments = "--query-gpu=name,memory.total,driver_version --format=csv,noheader",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    var parts = output.Trim().Split(',');
                    if (parts.Length >= 3)
                    {
                        gpuInfo.IsNvidiaGpu = true;
                        gpuInfo.Name = parts[0].Trim();
                        
                        // Check if RTX
                        if (gpuInfo.Name.Contains("RTX", StringComparison.OrdinalIgnoreCase))
                        {
                            gpuInfo.IsRtxCard = true;
                        }
                        
                        // Parse memory (comes as "8192 MiB")
                        var memoryStr = parts[1].Trim();
                        if (memoryStr.EndsWith(" MiB"))
                        {
                            if (int.TryParse(memoryStr.Replace(" MiB", ""), out int memoryMib))
                            {
                                gpuInfo.MemoryBytes = (long)memoryMib * 1024 * 1024;
                            }
                        }
                        
                        gpuInfo.DriverVersion = parts[2].Trim();
                        gpuInfo.Status = "NVIDIA GPU detected via nvidia-smi";
                    }
                }
                else
                {
                    gpuInfo.Status = "nvidia-smi not available";
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "nvidia-smi not available or failed");
                gpuInfo.Status = "GPU detection not available on Linux";
            }
        }

        private bool CheckCudaAvailability(GpuInfo gpuInfo)
        {
            if (!gpuInfo.IsNvidiaGpu)
                return false;
            
            // Check for CUDA installation
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // First check environment variable
                var cudaPath = Environment.GetEnvironmentVariable("CUDA_PATH");
                if (!string.IsNullOrEmpty(cudaPath) && System.IO.Directory.Exists(cudaPath))
                {
                    _logger.LogInformation($"Found CUDA via environment variable at {cudaPath}");
                    // Try to extract version from path
                    if (cudaPath.Contains("v"))
                    {
                        var versionStr = cudaPath.Split('v').Last();
                        if (versionStr.Contains("."))
                        {
                            gpuInfo.CudaVersion = int.Parse(versionStr.Split('.')[0]);
                            gpuInfo.Status = $"CUDA {gpuInfo.CudaVersion} detected via CUDA_PATH";
                        }
                    }
                    return true;
                }
                
                // Check common CUDA installation paths (including newer versions for Ada Lovelace)
                var cudaPaths = new[]
                {
                    @"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v12.8",
                    @"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v12.7",
                    @"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v12.6",
                    @"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v12.5",
                    @"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v12.4",
                    @"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v12.3",
                    @"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v12.2",
                    @"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v12.1",
                    @"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v12.0",
                    @"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v11.8",
                    @"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v11.7",
                    @"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v11.0"
                };
                
                foreach (var path in cudaPaths)
                {
                    if (System.IO.Directory.Exists(path))
                    {
                        // Extract version from path
                        var version = path.Split('\\').Last().Replace("v", "");
                        if (version.Contains("."))
                        {
                            gpuInfo.CudaVersion = int.Parse(version.Split('.')[0]);
                            _logger.LogInformation($"Found CUDA {version} at {path}");
                            gpuInfo.Status = $"CUDA {version} detected at {path}";
                            return true;
                        }
                    }
                }
                
                // Check if nvcc is available in PATH
                try
                {
                    var process = new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "nvcc",
                            Arguments = "--version",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        }
                    };
                    
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    
                    if (process.ExitCode == 0 && output.Contains("release"))
                    {
                        // Extract version from nvcc output
                        var lines = output.Split('\n');
                        foreach (var line in lines)
                        {
                            if (line.Contains("release"))
                            {
                                var parts = line.Split("release");
                                if (parts.Length > 1)
                                {
                                    var versionStr = parts[1].Trim().Split(',')[0];
                                    if (versionStr.Contains("."))
                                    {
                                        gpuInfo.CudaVersion = int.Parse(versionStr.Split('.')[0]);
                                        _logger.LogInformation($"Found CUDA {versionStr} via nvcc");
                                        gpuInfo.Status = $"CUDA {versionStr} detected via nvcc";
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug($"nvcc not found in PATH: {ex.Message}");
                }
                
                // If CUDA not found, provide helpful message
                gpuInfo.Status = "CUDA not found. Please install CUDA Toolkit 12.x from https://developer.nvidia.com/cuda-downloads";
                _logger.LogWarning("CUDA not detected. For GPU acceleration with RTX cards, please install CUDA Toolkit 12.x");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Check if CUDA is available via nvcc
                try
                {
                    var process = new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "nvcc",
                            Arguments = "--version",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        }
                    };
                    
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    
                    if (process.ExitCode == 0 && output.Contains("cuda"))
                    {
                        // Try to extract version (e.g., "Cuda compilation tools, release 11.8")
                        if (output.Contains("release"))
                        {
                            var parts = output.Split("release");
                            if (parts.Length > 1)
                            {
                                var versionStr = parts[1].Trim().Split(' ')[0];
                                if (versionStr.Contains("."))
                                {
                                    gpuInfo.CudaVersion = int.Parse(versionStr.Split('.')[0]);
                                }
                            }
                        }
                        return true;
                    }
                }
                catch
                {
                    // nvcc not available
                }
            }
            
            return false;
        }

        public bool IsNvidiaRtxAvailable()
        {
            var gpu = DetectGpu();
            return gpu.IsNvidiaGpu && gpu.IsRtxCard && gpu.IsCudaAvailable;
        }

        public bool IsCudaAvailable()
        {
            var gpu = DetectGpu();
            return gpu.IsCudaAvailable;
        }

        public int GetOptimalGpuLayerCount(long modelSizeBytes)
        {
            var gpu = DetectGpu();
            
            if (!gpu.IsNvidiaGpu || !gpu.IsCudaAvailable || gpu.MemoryBytes <= 0)
            {
                return 0; // Use CPU only
            }
            
            // Reserve 2GB for system and other processes
            const long reservedMemory = 2L * 1024 * 1024 * 1024;
            var availableMemory = gpu.MemoryBytes - reservedMemory;
            
            if (availableMemory <= 0)
            {
                return 0;
            }
            
            // Estimate layers based on model size and available memory
            // This is a rough estimate - typically each layer needs about 100-200MB
            // We'll be conservative and assume 200MB per layer
            const long memoryPerLayer = 200L * 1024 * 1024;
            
            // Calculate how many layers we can fit
            var maxLayers = (int)(availableMemory / memoryPerLayer);
            
            // Also consider model size - smaller models have fewer layers
            // Rough estimates:
            // 1B model: ~24-32 layers
            // 3B model: ~32-40 layers
            // 7B model: ~32-48 layers
            int estimatedTotalLayers = 32; // Default
            
            if (modelSizeBytes < 1_500_000_000) // < 1.5GB (1B models)
            {
                estimatedTotalLayers = 28;
            }
            else if (modelSizeBytes < 3_000_000_000) // < 3GB (3B models)
            {
                estimatedTotalLayers = 36;
            }
            else if (modelSizeBytes < 5_000_000_000) // < 5GB (7B models)
            {
                estimatedTotalLayers = 40;
            }
            
            // Return the minimum of what we can fit and total layers
            var optimalLayers = Math.Min(maxLayers, estimatedTotalLayers);
            
            _logger.LogInformation($"Optimal GPU layers calculated: {optimalLayers} " +
                                 $"(Model size: {modelSizeBytes / (1024*1024)}MB, " +
                                 $"GPU memory: {gpu.FormattedMemory}, " +
                                 $"Available: {availableMemory / (1024*1024)}MB)");
            
            return optimalLayers;
        }

        private void LogGpuInfo(GpuInfo gpu)
        {
            if (gpu.IsNvidiaGpu)
            {
                _logger.LogInformation($"GPU Detected: {gpu.Name}");
                _logger.LogInformation($"  - RTX Card: {gpu.IsRtxCard}");
                _logger.LogInformation($"  - Memory: {gpu.FormattedMemory}");
                _logger.LogInformation($"  - Driver: {gpu.DriverVersion}");
                _logger.LogInformation($"  - CUDA Available: {gpu.IsCudaAvailable}");
                if (gpu.CudaVersion > 0)
                {
                    _logger.LogInformation($"  - CUDA Version: {gpu.CudaVersion}");
                }
            }
            else
            {
                _logger.LogInformation("No NVIDIA GPU detected - will use CPU for inference");
            }
        }
    }
}