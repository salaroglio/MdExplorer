using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.Exceptions;
using MdExplorer.Abstractions.Models;
using MdExplorer.Abstractions.Services;

namespace MdExplorer.AI.Stubs.Services
{
    /// <summary>
    /// Implementazione stub per download modelli.
    /// Mostra modelli disponibili ma richiede premium per download.
    /// </summary>
    public class ModelDownloadServiceStub : IModelDownloadService
    {
        private readonly ILogger<ModelDownloadServiceStub> _logger;

        public ModelDownloadServiceStub(ILogger<ModelDownloadServiceStub> logger)
        {
            _logger = logger;
            _logger.LogInformation("ℹ️  Model Download Service stub initialized");
        }

        public Task<ModelInfo[]> GetAvailableModelsAsync()
        {
            // Mostra modelli disponibili per far vedere cosa si perdono :)
            var models = new[]
            {
                new ModelInfo
                {
                    Id = "qwen3-8b",
                    Name = "Qwen 3 8B (Requires Premium)",
                    Description = "High-quality 8B parameter model - Fast and accurate",
                    HuggingFaceRepo = "Qwen/Qwen2.5-8B-Instruct-GGUF",
                    FileName = "qwen2.5-8b-instruct-q5_k_m.gguf",
                    SizeBytes = 5_400_000_000,
                    FormattedSize = "5.0 GB",
                    IsInstalled = false,
                    LocalPath = "",
                    RequiredCapabilities = new[] { "8gb-ram" }
                },
                new ModelInfo
                {
                    Id = "qwen2.5-7b",
                    Name = "Qwen 2.5 7B (Requires Premium)",
                    Description = "Balanced performance model",
                    HuggingFaceRepo = "Qwen/Qwen2.5-7B-Instruct-GGUF",
                    FileName = "qwen2.5-7b-instruct-q5_k_m.gguf",
                    SizeBytes = 4_800_000_000,
                    FormattedSize = "4.5 GB",
                    IsInstalled = false,
                    LocalPath = "",
                    RequiredCapabilities = new[] { "8gb-ram" }
                },
                new ModelInfo
                {
                    Id = "phi3-mini",
                    Name = "Phi-3 Mini (Requires Premium)",
                    Description = "Small but capable model for faster inference",
                    HuggingFaceRepo = "microsoft/Phi-3-mini-4k-instruct-gguf",
                    FileName = "Phi-3-mini-4k-instruct-q4.gguf",
                    SizeBytes = 2_300_000_000,
                    FormattedSize = "2.2 GB",
                    IsInstalled = false,
                    LocalPath = "",
                    RequiredCapabilities = new[] { "4gb-ram" }
                }
            };

            return Task.FromResult(models);
        }

        public Task<ModelInfo[]> GetInstalledModelsAsync()
        {
            // Nessun modello installato in versione free
            return Task.FromResult(Array.Empty<ModelInfo>());
        }

        public Task<bool> DownloadModelAsync(
            string modelId,
            IProgress<DownloadProgress> progress,
            CancellationToken ct = default)
        {
            _logger.LogWarning($"Model download requested for {modelId} but AI Premium not available");

            // Invia un messaggio di errore via progress
            progress?.Report(new DownloadProgress
            {
                ModelId = modelId,
                Status = "Error",
                ErrorMessage = "AI Premium required to download models.\n" +
                              "Visit https://mdexplorer.net/ai-premium to get access.",
                BytesDownloaded = 0,
                TotalBytes = 0,
                PercentComplete = 0
            });

            throw new LicenseRequiredException("Model download requires AI Premium");
        }

        public Task<bool> DeleteModelAsync(string fileName)
        {
            _logger.LogWarning($"Model delete requested for {fileName} but no models in free version");
            return Task.FromResult(false);
        }
    }
}
