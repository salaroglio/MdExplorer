using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services
{
    public class LlamaBackendInfo
    {
        public string Variant { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long DownloadSize { get; set; }
        public bool IsInstalled { get; set; }
        public bool IsRecommended { get; set; }
    }

    public class LlamaBackendStatus
    {
        public bool IsInstalled { get; set; }
        public string InstalledVariant { get; set; }
        public string Version { get; set; }
        public string Path { get; set; }
        public bool HasCudaDll { get; set; }
        public bool HasVulkanDll { get; set; }
    }

    public interface ILlamaBackendService
    {
        List<LlamaBackendInfo> GetAvailableBackends();
        LlamaBackendStatus GetInstalledBackend();
        string GetBackendPath();
        bool IsBackendInstalled();
        Task<bool> DownloadBackendAsync(string variant, IProgress<DownloadProgress> progress, CancellationToken ct = default);
        Task<bool> DeleteBackendAsync();
        string GetRecommendedBackend();
        string GetVersion();
    }

    public class LlamaBackendService : ILlamaBackendService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LlamaBackendService> _logger;
        private readonly IGpuDetectionService _gpuService;
        private readonly string _version;
        private readonly string _backendsBaseDir;

        // GitHub release URL pattern
        private const string GitHubUrlTemplate = "https://github.com/ggml-org/llama.cpp/releases/download/{0}/llama-{0}-bin-{1}.zip";

        // Backend variants per platform
        private static readonly Dictionary<string, (string name, string description, long winSize, long linuxSize)> Variants = new()
        {
            ["cpu"] = ("CPU Only", "Runs on any CPU, no GPU required", 33_000_000, 27_000_000),
            ["cuda-12.4"] = ("NVIDIA CUDA 12.4", "GPU acceleration for NVIDIA GPUs (requires CUDA)", 214_000_000, 0),
            ["vulkan"] = ("Vulkan", "GPU acceleration for AMD, Intel, and NVIDIA GPUs", 49_000_000, 43_000_000),
        };

        public LlamaBackendService(
            IHttpClientFactory httpClientFactory,
            ILogger<LlamaBackendService> logger,
            IGpuDetectionService gpuService,
            IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromHours(1);
            _logger = logger;
            _gpuService = gpuService;

            _version = configuration.GetValue<string>("LlamaCpp:Version") ?? "b8255";

            var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _backendsBaseDir = Path.Combine(userHome, "MdExplorer-Models", "llama-backends");
            Directory.CreateDirectory(_backendsBaseDir);
        }

        /// <summary>
        /// Resolves the backend path statically (no DI needed).
        /// Used by Startup.cs before services are available.
        /// </summary>
        public static string ResolveBackendPath(string version)
        {
            var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var backendDir = Path.Combine(userHome, "MdExplorer-Models", "llama-backends", version);
            if (Directory.Exists(backendDir) && Directory.GetFiles(backendDir, "llama.*").Length > 0)
            {
                return backendDir;
            }

            // Fallback to LlamaCppOverride in app directory (dev/legacy)
            var overrideDir = Path.Combine(AppContext.BaseDirectory, "LlamaCppOverride");
            if (Directory.Exists(overrideDir) && Directory.GetFiles(overrideDir, "llama.*").Length > 0)
            {
                return overrideDir;
            }

            return null;
        }

        public string GetVersion() => _version;

        public string GetBackendPath()
        {
            return ResolveBackendPath(_version);
        }

        public bool IsBackendInstalled()
        {
            return GetBackendPath() != null;
        }

        public LlamaBackendStatus GetInstalledBackend()
        {
            var path = GetBackendPath();
            if (path == null)
            {
                return new LlamaBackendStatus { IsInstalled = false, Version = _version };
            }

            // Detect variant from installed files
            var hasCuda = Directory.GetFiles(path, "ggml-cuda.*").Length > 0;
            var hasVulkan = Directory.GetFiles(path, "ggml-vulkan.*").Length > 0;

            string variant = "cpu";
            if (hasCuda) variant = "cuda-12.4";
            else if (hasVulkan) variant = "vulkan";

            return new LlamaBackendStatus
            {
                IsInstalled = true,
                InstalledVariant = variant,
                Version = _version,
                Path = path,
                HasCudaDll = hasCuda,
                HasVulkanDll = hasVulkan
            };
        }

        public List<LlamaBackendInfo> GetAvailableBackends()
        {
            var installed = GetInstalledBackend();
            var recommended = GetRecommendedBackend();
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var backends = new List<LlamaBackendInfo>();
            foreach (var kv in Variants)
            {
                // Skip CUDA on non-Windows (Linux uses different packaging)
                if (kv.Key == "cuda-12.4" && !isWindows) continue;

                backends.Add(new LlamaBackendInfo
                {
                    Variant = kv.Key,
                    Name = kv.Value.name,
                    Description = kv.Value.description,
                    DownloadSize = isWindows ? kv.Value.winSize : kv.Value.linuxSize,
                    IsInstalled = installed.IsInstalled && installed.InstalledVariant == kv.Key,
                    IsRecommended = kv.Key == recommended
                });
            }

            return backends;
        }

        public string GetRecommendedBackend()
        {
            try
            {
                var gpuInfo = _gpuService.DetectGpu();
                if (gpuInfo.IsNvidiaGpu && gpuInfo.IsCudaAvailable &&
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return "cuda-12.4";
                }
                // Vulkan works on AMD, Intel, and NVIDIA
                if (gpuInfo.IsNvidiaGpu || !string.IsNullOrEmpty(gpuInfo.Name))
                {
                    return "vulkan";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GPU detection failed for backend recommendation");
            }

            return "cpu";
        }

        public async Task<bool> DownloadBackendAsync(string variant, IProgress<DownloadProgress> progress, CancellationToken ct = default)
        {
            if (!Variants.ContainsKey(variant))
            {
                _logger.LogError("Unknown backend variant: {Variant}", variant);
                return false;
            }

            var platformSuffix = GetPlatformSuffix(variant);
            if (platformSuffix == null)
            {
                _logger.LogError("Unsupported platform for variant: {Variant}", variant);
                return false;
            }

            var url = string.Format(GitHubUrlTemplate, _version, platformSuffix);
            var targetDir = Path.Combine(_backendsBaseDir, _version);
            var tempZip = Path.Combine(_backendsBaseDir, $"{_version}-{variant}.zip.download");
            long totalBytesRead = 0;

            try
            {
                _logger.LogInformation("Downloading llama.cpp backend {Variant} from {Url}", variant, url);

                progress?.Report(new DownloadProgress
                {
                    ModelId = $"backend-{variant}",
                    BytesDownloaded = 0,
                    TotalBytes = 0,
                    Status = "Connecting"
                });

                // Clean up target directory if it exists (switching variant)
                if (Directory.Exists(targetDir))
                {
                    Directory.Delete(targetDir, true);
                }

                // Download the zip file with resume support
                long startByte = 0;
                if (File.Exists(tempZip))
                {
                    startByte = new FileInfo(tempZip).Length;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                // GitHub redirects to CDN, need to follow redirects
                if (startByte > 0)
                {
                    request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(startByte, null);
                }

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

                if (response.StatusCode == System.Net.HttpStatusCode.RequestedRangeNotSatisfiable)
                {
                    // File might be complete
                    if (File.Exists(tempZip))
                    {
                        _logger.LogInformation("Zip file appears complete, proceeding to extract");
                        goto Extract;
                    }
                }

                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                if (startByte > 0) totalBytes += startByte;

                totalBytesRead = startByte;
                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(tempZip, startByte > 0 ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
                {
                    var buffer = new byte[81920];
                    int bytesRead;
                    var lastProgressUpdate = DateTime.UtcNow;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, ct);
                        totalBytesRead += bytesRead;

                        if ((DateTime.UtcNow - lastProgressUpdate).TotalMilliseconds >= 100)
                        {
                            progress?.Report(new DownloadProgress
                            {
                                ModelId = $"backend-{variant}",
                                BytesDownloaded = totalBytesRead,
                                TotalBytes = totalBytes,
                                Status = "Downloading"
                            });
                            lastProgressUpdate = DateTime.UtcNow;
                        }
                    }
                }

                Extract:
                // Extract the zip
                progress?.Report(new DownloadProgress
                {
                    ModelId = $"backend-{variant}",
                    BytesDownloaded = totalBytesRead,
                    TotalBytes = totalBytesRead,
                    Status = "Extracting"
                });

                _logger.LogInformation("Extracting backend to {Dir}", targetDir);
                Directory.CreateDirectory(targetDir);

                using (var archive = ZipFile.OpenRead(tempZip))
                {
                    // GitHub zips have a single root directory - extract DLLs/SOs flat into targetDir
                    foreach (var entry in archive.Entries)
                    {
                        // Skip directories and executables, keep only libraries
                        if (string.IsNullOrEmpty(entry.Name)) continue;

                        var ext = Path.GetExtension(entry.Name).ToLowerInvariant();
                        var isLibrary = ext == ".dll" || ext == ".so" || ext == ".dylib" || ext == ".metal";

                        if (!isLibrary) continue;

                        var destPath = Path.Combine(targetDir, entry.Name);
                        entry.ExtractToFile(destPath, true);
                    }
                }

                // Verify extraction: llama.dll or libllama.so must exist
                var llamaFiles = Directory.GetFiles(targetDir, "llama.*");
                var libLlamaFiles = Directory.GetFiles(targetDir, "libllama.*");
                if (llamaFiles.Length == 0 && libLlamaFiles.Length == 0)
                {
                    _logger.LogError("Extraction failed: llama library not found in {Dir}", targetDir);
                    Directory.Delete(targetDir, true);
                    return false;
                }

                // Clean up zip
                if (File.Exists(tempZip))
                {
                    File.Delete(tempZip);
                }

                _logger.LogInformation("Backend {Variant} installed successfully at {Dir}", variant, targetDir);

                progress?.Report(new DownloadProgress
                {
                    ModelId = $"backend-{variant}",
                    BytesDownloaded = totalBytesRead,
                    TotalBytes = totalBytesRead,
                    Status = "Complete"
                });

                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Backend download cancelled");
                progress?.Report(new DownloadProgress
                {
                    ModelId = $"backend-{variant}",
                    BytesDownloaded = 0,
                    TotalBytes = 0,
                    Status = "Cancelled"
                });
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading backend {Variant}", variant);
                progress?.Report(new DownloadProgress
                {
                    ModelId = $"backend-{variant}",
                    BytesDownloaded = 0,
                    TotalBytes = 0,
                    Status = $"Error: {ex.Message}"
                });
                return false;
            }
        }

        public async Task<bool> DeleteBackendAsync()
        {
            try
            {
                var targetDir = Path.Combine(_backendsBaseDir, _version);
                if (Directory.Exists(targetDir))
                {
                    Directory.Delete(targetDir, true);
                    _logger.LogInformation("Backend deleted from {Dir}", targetDir);
                }
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting backend");
                return false;
            }
        }

        private string GetPlatformSuffix(string variant)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return variant switch
                {
                    "cpu" => "win-cpu-x64",
                    "cuda-12.4" => "win-cuda-12.4-x64",
                    "vulkan" => "win-vulkan-x64",
                    _ => null
                };
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return variant switch
                {
                    "cpu" => "ubuntu-x64",
                    "vulkan" => "ubuntu-vulkan-x64",
                    _ => null
                };
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                    ? "macos-arm64"
                    : "macos-x64";
            }
            return null;
        }
    }
}
