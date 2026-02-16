using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.Services;

namespace MdExplorer.Features.Services
{
    public class EmbeddingConfig
    {
        public string SelectedModel { get; set; }
        public int ContextSize { get; set; }
        public int BatchSize { get; set; }
        public int MaxChunkChars { get; set; }
        public int MaxEmbeddingChars { get; set; }
    }

    public interface IEmbeddingConfigService
    {
        EmbeddingConfig GetConfig();
        void SaveConfig(EmbeddingConfig config);
        EmbeddingConfig GetPresetForModel(string modelId);
        Dictionary<string, EmbeddingConfig> GetAllPresets();
    }

    public class EmbeddingConfigService : IEmbeddingConfigService
    {
        private readonly ILogger<EmbeddingConfigService> _logger;
        private readonly IModelDownloadService _downloadService;
        private readonly string _configPath;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private EmbeddingConfig _cachedConfig;

        private static readonly Dictionary<string, EmbeddingConfig> Presets = new Dictionary<string, EmbeddingConfig>
        {
            ["nomic-embed-text"] = new EmbeddingConfig
            {
                SelectedModel = "nomic-embed-text",
                ContextSize = 4096,
                BatchSize = 4096,
                MaxChunkChars = 2000,
                MaxEmbeddingChars = 12000
            },
            ["multilingual-e5-large-instruct"] = new EmbeddingConfig
            {
                SelectedModel = "multilingual-e5-large-instruct",
                ContextSize = 512,
                BatchSize = 512,
                MaxChunkChars = 800,
                MaxEmbeddingChars = 1200
            }
        };

        public EmbeddingConfigService(ILogger<EmbeddingConfigService> logger, IModelDownloadService downloadService)
        {
            _logger = logger;
            _downloadService = downloadService;

            var modelsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "MdExplorer-Models");
            Directory.CreateDirectory(modelsDir);
            _configPath = Path.Combine(modelsDir, "embedding-config.json");
        }

        public EmbeddingConfig GetConfig()
        {
            _lock.EnterReadLock();
            try
            {
                if (_cachedConfig != null)
                    return _cachedConfig;
            }
            finally
            {
                _lock.ExitReadLock();
            }

            _lock.EnterWriteLock();
            try
            {
                // Double-check after acquiring write lock
                if (_cachedConfig != null)
                    return _cachedConfig;

                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    _cachedConfig = JsonSerializer.Deserialize<EmbeddingConfig>(json);
                    _logger.LogInformation("[EmbeddingConfig] Loaded config from {Path}", _configPath);
                    return _cachedConfig;
                }

                // No config file - return preset for best installed model
                _cachedConfig = GetDefaultConfig();
                _logger.LogInformation("[EmbeddingConfig] No config file, using default for model: {Model}", _cachedConfig.SelectedModel);
                return _cachedConfig;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[EmbeddingConfig] Error reading config, using defaults");
                _cachedConfig = GetDefaultConfig();
                return _cachedConfig;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void SaveConfig(EmbeddingConfig config)
        {
            _lock.EnterWriteLock();
            try
            {
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
                _cachedConfig = config;
                _logger.LogInformation("[EmbeddingConfig] Saved config: Model={Model}, ContextSize={Ctx}, BatchSize={Batch}, MaxChunkChars={Chunk}",
                    config.SelectedModel, config.ContextSize, config.BatchSize, config.MaxChunkChars);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public EmbeddingConfig GetPresetForModel(string modelId)
        {
            if (Presets.TryGetValue(modelId, out var preset))
            {
                // Return a copy
                return new EmbeddingConfig
                {
                    SelectedModel = preset.SelectedModel,
                    ContextSize = preset.ContextSize,
                    BatchSize = preset.BatchSize,
                    MaxChunkChars = preset.MaxChunkChars,
                    MaxEmbeddingChars = preset.MaxEmbeddingChars
                };
            }
            return null;
        }

        public Dictionary<string, EmbeddingConfig> GetAllPresets()
        {
            return new Dictionary<string, EmbeddingConfig>(Presets);
        }

        private EmbeddingConfig GetDefaultConfig()
        {
            // Determine which model is installed and return its preset
            var modelPath = _downloadService.GetInstalledEmbeddingModelPath();
            if (modelPath != null)
            {
                var fileName = Path.GetFileName(modelPath).ToLower();
                if (fileName.Contains("multilingual-e5"))
                    return GetPresetForModel("multilingual-e5-large-instruct");
                if (fileName.Contains("nomic"))
                    return GetPresetForModel("nomic-embed-text");
            }

            // Fallback to nomic defaults
            return GetPresetForModel("nomic-embed-text");
        }
    }
}
