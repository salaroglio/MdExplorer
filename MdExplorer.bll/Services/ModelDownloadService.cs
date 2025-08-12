using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services
{
    public class DownloadProgress
    {
        public string ModelId { get; set; }
        public long BytesDownloaded { get; set; }
        public long TotalBytes { get; set; }
        public double PercentComplete => TotalBytes > 0 ? (double)BytesDownloaded / TotalBytes * 100 : 0;
        public string Status { get; set; }
    }

    public class ModelInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public bool IsInstalled { get; set; }
        public string LocalPath { get; set; }
        public int ContextLength { get; set; }
        public string Parameters { get; set; }
    }

    public interface IModelDownloadService
    {
        Task<bool> DownloadModelAsync(string modelId, IProgress<DownloadProgress> progress, CancellationToken ct = default);
        Task<ModelInfo[]> GetAvailableModelsAsync();
        Task<ModelInfo[]> GetInstalledModelsAsync();
        bool IsModelInstalled(string modelName);
        string GetModelPath(string modelName);
        Task<bool> DeleteModelAsync(string modelName);
    }

    public class ModelDownloadService : IModelDownloadService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ModelDownloadService> _logger;
        private readonly string _modelsDirectory;
        private readonly Dictionary<string, ModelInfo> _availableModels;

        public ModelDownloadService(IHttpClientFactory httpClientFactory, ILogger<ModelDownloadService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromHours(2); // Long timeout for large downloads
            _logger = logger;
            
            // Setup models directory
            var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _modelsDirectory = Path.Combine(userHome, "MdExplorer-Models");
            Directory.CreateDirectory(_modelsDirectory);
            
            // Initialize available models catalog
            _availableModels = new Dictionary<string, ModelInfo>
            {
                ["qwen3-8b"] = new ModelInfo
                {
                    Id = "qwen3-8b",
                    Name = "Qwen3 8B Q4_K_M",
                    Description = "Powerful 8B parameter model with excellent reasoning capabilities",
                    Url = "https://huggingface.co/bartowski/Qwen3-8B-GGUF/resolve/main/Qwen3-8B-Q4_K_M.gguf",
                    FileName = "Qwen3-8B-Q4_K_M.gguf",
                    FileSize = 5_100_000_000, // ~5.1GB
                    ContextLength = 8192,
                    Parameters = "8B"
                },
                ["qwen2.5-7b"] = new ModelInfo
                {
                    Id = "qwen2.5-7b",
                    Name = "Qwen2.5 7B Instruct Q4_K_M",
                    Description = "Latest Qwen 2.5 model optimized for instruction following",
                    Url = "https://huggingface.co/Qwen/Qwen2.5-7B-Instruct-GGUF/resolve/main/qwen2.5-7b-instruct-q4_k_m.gguf",
                    FileName = "qwen2.5-7b-instruct-q4_k_m.gguf",
                    FileSize = 4_700_000_000, // ~4.7GB
                    ContextLength = 32768,
                    Parameters = "7B"
                },
                ["phi3-mini"] = new ModelInfo
                {
                    Id = "phi3-mini",
                    Name = "Phi-3 Mini 4K Instruct",
                    Description = "Compact 3.8B model from Microsoft with strong performance",
                    Url = "https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-gguf/resolve/main/Phi-3-mini-4k-instruct-q4.gguf",
                    FileName = "Phi-3-mini-4k-instruct-q4.gguf",
                    FileSize = 2_200_000_000, // ~2.2GB
                    ContextLength = 4096,
                    Parameters = "3.8B"
                }
            };
        }

        public async Task<ModelInfo[]> GetAvailableModelsAsync()
        {
            var models = _availableModels.Values.ToArray();
            
            // Check which models are installed
            foreach (var model in models)
            {
                model.IsInstalled = IsModelInstalled(model.FileName);
                if (model.IsInstalled)
                {
                    model.LocalPath = GetModelPath(model.FileName);
                }
            }
            
            return await Task.FromResult(models);
        }

        public async Task<ModelInfo[]> GetInstalledModelsAsync()
        {
            var allModels = await GetAvailableModelsAsync();
            return allModels.Where(m => m.IsInstalled).ToArray();
        }

        public bool IsModelInstalled(string modelName)
        {
            var modelPath = GetModelPath(modelName);
            return File.Exists(modelPath);
        }

        public string GetModelPath(string modelName)
        {
            return Path.Combine(_modelsDirectory, modelName);
        }

        public async Task<bool> DeleteModelAsync(string modelName)
        {
            try
            {
                var modelPath = GetModelPath(modelName);
                if (File.Exists(modelPath))
                {
                    File.Delete(modelPath);
                    _logger.LogInformation($"Deleted model: {modelName}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete model: {modelName}");
                return false;
            }
        }

        public async Task<bool> DownloadModelAsync(string modelId, IProgress<DownloadProgress> progress, CancellationToken ct = default)
        {
            if (!_availableModels.TryGetValue(modelId, out var modelInfo))
            {
                _logger.LogError($"Model not found: {modelId}");
                return false;
            }

            var modelPath = GetModelPath(modelInfo.FileName);
            var tempPath = modelPath + ".download";

            try
            {
                _logger.LogInformation($"Starting download of {modelInfo.Name} from {modelInfo.Url}");
                
                // Check if we can resume a partial download
                long startByte = 0;
                if (File.Exists(tempPath))
                {
                    var fileInfo = new FileInfo(tempPath);
                    startByte = fileInfo.Length;
                    _logger.LogInformation($"Resuming download from byte {startByte}");
                }

                // Create HTTP request with range header for resume support
                var request = new HttpRequestMessage(HttpMethod.Get, modelInfo.Url);
                if (startByte > 0)
                {
                    request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(startByte, null);
                }

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? modelInfo.FileSize;
                if (startByte > 0)
                {
                    totalBytes += startByte;
                }

                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(tempPath, startByte > 0 ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                long totalBytesRead = startByte;
                int bytesRead;
                var lastProgressUpdate = DateTime.UtcNow;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, ct);
                    totalBytesRead += bytesRead;

                    // Update progress every 100ms to avoid overwhelming the UI
                    if ((DateTime.UtcNow - lastProgressUpdate).TotalMilliseconds >= 100)
                    {
                        progress?.Report(new DownloadProgress
                        {
                            ModelId = modelId,
                            BytesDownloaded = totalBytesRead,
                            TotalBytes = totalBytes,
                            Status = "Downloading"
                        });
                        lastProgressUpdate = DateTime.UtcNow;
                    }
                }

                // Final progress update
                progress?.Report(new DownloadProgress
                {
                    ModelId = modelId,
                    BytesDownloaded = totalBytesRead,
                    TotalBytes = totalBytes,
                    Status = "Finalizing"
                });

                // Move temp file to final location
                if (File.Exists(modelPath))
                {
                    File.Delete(modelPath);
                }
                File.Move(tempPath, modelPath);

                _logger.LogInformation($"Successfully downloaded {modelInfo.Name}");
                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"Download cancelled for {modelInfo.Name}");
                progress?.Report(new DownloadProgress
                {
                    ModelId = modelId,
                    BytesDownloaded = 0,
                    TotalBytes = 0,
                    Status = "Cancelled"
                });
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading {modelInfo.Name}");
                progress?.Report(new DownloadProgress
                {
                    ModelId = modelId,
                    BytesDownloaded = 0,
                    TotalBytes = 0,
                    Status = $"Error: {ex.Message}"
                });
                return false;
            }
        }
    }
}