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
        public string SystemPrompt { get; set; }
    }

    public interface IModelDownloadService
    {
        Task<bool> DownloadModelAsync(string modelId, IProgress<DownloadProgress> progress, CancellationToken ct = default);
        Task<ModelInfo[]> GetAvailableModelsAsync();
        Task<ModelInfo[]> GetInstalledModelsAsync();
        bool IsModelInstalled(string modelName);
        string GetModelPath(string modelName);
        Task<bool> DeleteModelAsync(string modelName);
        /// <summary>
        /// Returns the path to the best installed embedding model.
        /// Prefers multilingual-e5-large-instruct, falls back to nomic-embed-text.
        /// Returns null if no embedding model is installed.
        /// </summary>
        string GetInstalledEmbeddingModelPath();
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
                ["llama3.2-1b"] = new ModelInfo
                {
                    Id = "llama3.2-1b",
                    Name = "Llama 3.2 1B Instruct Q4_K_M",
                    Description = "Ultra-compact Meta model for edge devices with 128K context",
                    Url = "https://huggingface.co/QuantFactory/Llama-3.2-1B-Instruct-GGUF/resolve/main/Llama-3.2-1B-Instruct.Q4_K_M.gguf",
                    FileName = "Llama-3.2-1B-Instruct.Q4_K_M.gguf",
                    FileSize = 808_000_000, // ~808MB
                    ContextLength = 128000,
                    Parameters = "1B"
                },
                ["llama3.2-1b-q5"] = new ModelInfo
                {
                    Id = "llama3.2-1b-q5",
                    Name = "Llama 3.2 1B Instruct Q5_K_M",
                    Description = "Higher quality 1B Meta model with excellent summarization",
                    Url = "https://huggingface.co/QuantFactory/Llama-3.2-1B-Instruct-GGUF/resolve/main/Llama-3.2-1B-Instruct.Q5_K_M.gguf",
                    FileName = "Llama-3.2-1B-Instruct.Q5_K_M.gguf",
                    FileSize = 912_000_000, // ~912MB
                    ContextLength = 128000,
                    Parameters = "1B"
                },
                ["llama3.2-3b"] = new ModelInfo
                {
                    Id = "llama3.2-3b",
                    Name = "Llama 3.2 3B Instruct Q4_K_M",
                    Description = "Powerful 3B Meta model for complex tasks with massive context",
                    Url = "https://huggingface.co/QuantFactory/Llama-3.2-3B-Instruct-GGUF/resolve/main/Llama-3.2-3B-Instruct.Q4_K_M.gguf",
                    FileName = "Llama-3.2-3B-Instruct.Q4_K_M.gguf",
                    FileSize = 2_020_000_000, // ~2.02GB
                    ContextLength = 128000,
                    Parameters = "3B"
                },
                ["qwen3.5-2b"] = new ModelInfo
                {
                    Id = "qwen3.5-2b",
                    Name = "Qwen 3.5 2B Q4_K_M",
                    Description = "Compact 2B hybrid model (Alibaba) - 262K context, 201 languages, fast inference",
                    Url = "https://huggingface.co/unsloth/Qwen3.5-2B-GGUF/resolve/main/Qwen3.5-2B-Q4_K_M.gguf",
                    FileName = "Qwen3.5-2B-Q4_K_M.gguf",
                    FileSize = 1_340_000_000, // ~1.28GB
                    ContextLength = 262144,
                    Parameters = "2B"
                },
                ["qwen3.5-9b"] = new ModelInfo
                {
                    Id = "qwen3.5-9b",
                    Name = "Qwen 3.5 9B Q4_K_M",
                    Description = "Powerful 9B hybrid model (Alibaba) - 262K context, 201 languages, reasoning capabilities",
                    Url = "https://huggingface.co/unsloth/Qwen3.5-9B-GGUF/resolve/main/Qwen3.5-9B-Q4_K_M.gguf",
                    FileName = "Qwen3.5-9B-Q4_K_M.gguf",
                    FileSize = 5_680_000_000,
                    ContextLength = 262144,
                    Parameters = "9B"
                },
                ["qwen3.5-9b-q5"] = new ModelInfo
                {
                    Id = "qwen3.5-9b-q5",
                    Name = "Qwen 3.5 9B Q5_K_M",
                    Description = "Higher quality 9B hybrid model (Alibaba) - 262K context, 201 languages, reasoning",
                    Url = "https://huggingface.co/unsloth/Qwen3.5-9B-GGUF/resolve/main/Qwen3.5-9B-Q5_K_M.gguf",
                    FileName = "Qwen3.5-9B-Q5_K_M.gguf",
                    FileSize = 6_520_000_000,
                    ContextLength = 262144,
                    Parameters = "9B"
                },
                ["nomic-embed-text"] = new ModelInfo
                {
                    Id = "nomic-embed-text",
                    Name = "Nomic Embed Text v1.5 Q8_0",
                    Description = "Embedding model for semantic search - English only (768 dimensions, 140MB)",
                    Url = "https://huggingface.co/nomic-ai/nomic-embed-text-v1.5-GGUF/resolve/main/nomic-embed-text-v1.5.Q8_0.gguf",
                    FileName = "nomic-embed-text-v1.5.Q8_0.gguf",
                    FileSize = 274_000_000,
                    ContextLength = 8192,
                    Parameters = "137M"
                },
                ["multilingual-e5-large-instruct"] = new ModelInfo
                {
                    Id = "multilingual-e5-large-instruct",
                    Name = "Multilingual E5 Large Instruct Q8_0",
                    Description = "Multilingual embedding model for semantic search - 100 languages including Italian (1024 dimensions, 603MB)",
                    Url = "https://huggingface.co/Ralriki/multilingual-e5-large-instruct-GGUF/resolve/main/multilingual-e5-large-instruct-q8_0.gguf",
                    FileName = "multilingual-e5-large-instruct-q8_0.gguf",
                    FileSize = 632_000_000,
                    ContextLength = 514,
                    Parameters = "560M"
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

        public Task<bool> DeleteModelAsync(string modelName)
        {
            try
            {
                var modelPath = GetModelPath(modelName);
                if (File.Exists(modelPath))
                {
                    File.Delete(modelPath);
                    _logger.LogInformation($"Deleted model: {modelName}");
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete model: {modelName}");
                return Task.FromResult(false);
            }
        }

        // Embedding model filenames in order of preference (best first)
        private static readonly string[] EmbeddingModelFiles = new[]
        {
            "multilingual-e5-large-instruct-q8_0.gguf",
            "nomic-embed-text-v1.5.Q8_0.gguf"
        };

        public string GetInstalledEmbeddingModelPath()
        {
            foreach (var fileName in EmbeddingModelFiles)
            {
                var path = GetModelPath(fileName);
                if (File.Exists(path))
                    return path;
            }
            return null;
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
                
                // Check if model already exists
                if (File.Exists(modelPath))
                {
                    _logger.LogInformation($"Model {modelInfo.Name} already exists at {modelPath}");
                    progress?.Report(new DownloadProgress
                    {
                        ModelId = modelId,
                        BytesDownloaded = modelInfo.FileSize,
                        TotalBytes = modelInfo.FileSize,
                        Status = "Already installed"
                    });
                    return true;
                }
                
                // Check if we can resume a partial download
                long startByte = 0;
                if (File.Exists(tempPath))
                {
                    var fileInfo = new FileInfo(tempPath);
                    startByte = fileInfo.Length;
                    
                    // Check if temp file is already complete or larger than expected
                    if (startByte >= modelInfo.FileSize)
                    {
                        _logger.LogInformation($"Temp file appears complete or corrupted (size: {startByte}, expected: {modelInfo.FileSize})");
                        
                        // If size matches, just rename it
                        if (startByte == modelInfo.FileSize)
                        {
                            _logger.LogInformation($"Temp file is complete, moving to final location");
                            File.Move(tempPath, modelPath);
                            progress?.Report(new DownloadProgress
                            {
                                ModelId = modelId,
                                BytesDownloaded = modelInfo.FileSize,
                                TotalBytes = modelInfo.FileSize,
                                Status = "Complete"
                            });
                            return true;
                        }
                        else
                        {
                            // File is larger than expected, delete and start fresh
                            _logger.LogWarning($"Temp file is larger than expected, deleting and starting fresh");
                            File.Delete(tempPath);
                            startByte = 0;
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"Resuming download from byte {startByte} of {modelInfo.FileSize}");
                    }
                }

                // Create HTTP request with range header for resume support
                var request = new HttpRequestMessage(HttpMethod.Get, modelInfo.Url);
                if (startByte > 0)
                {
                    request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(startByte, null);
                    _logger.LogInformation($"Setting Range header: bytes={startByte}-");
                }

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
                
                // Check for 416 Range Not Satisfiable
                if (response.StatusCode == System.Net.HttpStatusCode.RequestedRangeNotSatisfiable)
                {
                    _logger.LogWarning($"Server returned 416 - file might be complete or range invalid. Checking file integrity.");
                    
                    // Try to get the actual file size from server
                    var headRequest = new HttpRequestMessage(HttpMethod.Head, modelInfo.Url);
                    using var headResponse = await _httpClient.SendAsync(headRequest, ct);
                    var actualSize = headResponse.Content.Headers.ContentLength ?? modelInfo.FileSize;
                    
                    _logger.LogInformation($"Server reports file size: {actualSize}, temp file size: {startByte}");
                    
                    if (startByte >= actualSize)
                    {
                        // File is complete or over-downloaded
                        if (File.Exists(tempPath))
                        {
                            File.Move(tempPath, modelPath);
                            _logger.LogInformation($"Download complete (file was already fully downloaded)");
                            progress?.Report(new DownloadProgress
                            {
                                ModelId = modelId,
                                BytesDownloaded = actualSize,
                                TotalBytes = actualSize,
                                Status = "Complete"
                            });
                            return true;
                        }
                    }
                    
                    // Otherwise, delete temp and retry from scratch
                    if (File.Exists(tempPath))
                    {
                        _logger.LogInformation($"Deleting incomplete temp file and retrying from scratch");
                        File.Delete(tempPath);
                        return await DownloadModelAsync(modelId, progress, ct);
                    }
                }
                
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? modelInfo.FileSize;
                if (startByte > 0)
                {
                    totalBytes += startByte;
                }

                // Download the file
                long totalBytesRead = startByte;
                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(tempPath, startByte > 0 ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var buffer = new byte[8192];
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
                    
                    // Ensure all data is written to disk
                    await fileStream.FlushAsync();
                }
                // Streams are now closed

                // Final progress update
                progress?.Report(new DownloadProgress
                {
                    ModelId = modelId,
                    BytesDownloaded = totalBytesRead,
                    TotalBytes = totalBytes,
                    Status = "Finalizing"
                });

                // Add a small delay to ensure file handles are released on Windows
                await Task.Delay(100);

                // Move temp file to final location with retry logic
                int retryCount = 0;
                const int maxRetries = 5;
                while (retryCount < maxRetries)
                {
                    try
                    {
                        if (File.Exists(modelPath))
                        {
                            File.Delete(modelPath);
                        }
                        File.Move(tempPath, modelPath);
                        break; // Success, exit the retry loop
                    }
                    catch (IOException ioEx) when (retryCount < maxRetries - 1)
                    {
                        _logger.LogWarning($"File move attempt {retryCount + 1} failed: {ioEx.Message}. Retrying...");
                        await Task.Delay(500 * (retryCount + 1)); // Progressive delay
                        retryCount++;
                    }
                }

                _logger.LogInformation($"Successfully downloaded {modelInfo.Name}");
                
                // Report completion
                progress?.Report(new DownloadProgress
                {
                    ModelId = modelId,
                    BytesDownloaded = totalBytesRead,
                    TotalBytes = totalBytesRead,
                    Status = "Complete"
                });
                
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