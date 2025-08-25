using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LLama;
using LLama.Common;
using LLama.Native;
using LLama.Sampling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.IO;

namespace MdExplorer.Features.Services
{
    public interface IAiChatService
    {
        Task<string> ChatAsync(string prompt);
        IAsyncEnumerable<string> StreamChatAsync(string prompt, CancellationToken ct = default);
        bool IsModelLoaded();
        Task<bool> LoadModelAsync(string modelPath, string modelId = null);
        string GetCurrentModelName();
        string GetCurrentModelId();
        Task SetSystemPromptAsync(string systemPrompt);
        string GetSystemPrompt();
        GpuInfo GetGpuInfo();
        bool IsGpuEnabled();
        int GetGpuLayerCount();
    }

    public class AiChatService : IAiChatService, IDisposable
    {
        private readonly ILogger<AiChatService> _logger;
        private readonly IAiConfigurationService _configService;
        private readonly IGpuDetectionService _gpuService;
        private readonly AiChatConfiguration _aiConfig;
        private LLamaWeights _model;
        private LLamaContext _context;
        private InteractiveExecutor _executor;
        private string _currentModelPath;
        private string _currentModelName;
        private string _currentModelId;
        private string _systemPrompt;
        private GpuInfo _currentGpuInfo;
        private int _currentGpuLayerCount;
        private bool _gpuEnabled;
        private readonly SemaphoreSlim _modelLock = new SemaphoreSlim(1, 1);

        public AiChatService(
            ILogger<AiChatService> logger, 
            IGpuDetectionService gpuService = null, 
            IAiConfigurationService configService = null,
            IConfiguration configuration = null)
        {
            _logger = logger;
            _configService = configService;
            _gpuService = gpuService;
            
            // Load configuration
            _aiConfig = new AiChatConfiguration();
            if (configuration != null)
            {
                configuration.GetSection("AiChat").Bind(_aiConfig);
            }
            
            _logger.LogInformation($"[AiChatService] Service initialized with GPU auto-detection: {_aiConfig.Gpu.EnableAutoDetection}");
            
            // Detect GPU if service is available and auto-detection is enabled
            if (_gpuService != null && _aiConfig.Gpu.EnableAutoDetection)
            {
                _currentGpuInfo = _gpuService.DetectGpu();
                if (_currentGpuInfo.IsNvidiaGpu && _currentGpuInfo.IsCudaAvailable)
                {
                    _logger.LogInformation($"[AiChatService] GPU acceleration available: {_currentGpuInfo.Name}");
                    if (_aiConfig.Gpu.LogGpuDetails)
                    {
                        _logger.LogInformation($"[AiChatService] GPU Memory: {_currentGpuInfo.FormattedMemory}");
                        _logger.LogInformation($"[AiChatService] CUDA Version: {_currentGpuInfo.CudaVersion}");
                        _logger.LogInformation($"[AiChatService] Driver Version: {_currentGpuInfo.DriverVersion}");
                    }
                }
            }
            
            // Log current environment for debugging
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var ldPath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH");
                _logger.LogInformation($"[AiChatService] Running on Linux, LD_LIBRARY_PATH: {ldPath}");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _logger.LogInformation($"[AiChatService] Running on Windows");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _logger.LogInformation($"[AiChatService] Running on macOS");
            }
        }

        public bool IsModelLoaded()
        {
            return _model != null && _context != null && _executor != null;
        }

        public string GetCurrentModelName()
        {
            return _currentModelName ?? "None";
        }

        public string GetCurrentModelId()
        {
            return _currentModelId ?? "None";
        }

        public async Task SetSystemPromptAsync(string systemPrompt)
        {
            _systemPrompt = systemPrompt;
            if (_configService != null && !string.IsNullOrEmpty(_currentModelId))
            {
                await _configService.SaveSystemPromptAsync(_currentModelId, systemPrompt);
            }
        }

        public string GetSystemPrompt()
        {
            return _systemPrompt;
        }

        public async Task<bool> LoadModelAsync(string modelPath, string modelId = null)
        {
            await _modelLock.WaitAsync();
            try
            {
                _logger.LogInformation($"[AiChatService] LoadModelAsync called with path: {modelPath}");
                
                // Check if file exists
                if (!System.IO.File.Exists(modelPath))
                {
                    _logger.LogError($"[AiChatService] Model file does not exist: {modelPath}");
                    return false;
                }
                
                var fileInfo = new System.IO.FileInfo(modelPath);
                _logger.LogInformation($"[AiChatService] Model file size: {fileInfo.Length / (1024*1024*1024.0):F2} GB");

                // Dispose of existing model
                _logger.LogInformation($"[AiChatService] Disposing existing model...");
                DisposeModel();

                // Detect GPU and determine layer count
                int gpuLayerCount = 0;
                if (_gpuService != null && _aiConfig.Gpu.EnableAutoDetection && _aiConfig.Gpu.PreferGpuAcceleration)
                {
                    _currentGpuInfo = _gpuService.DetectGpu();
                    if (_currentGpuInfo.IsNvidiaGpu && _currentGpuInfo.IsCudaAvailable)
                    {
                        // Use configured layer count or auto-detect
                        if (_aiConfig.Gpu.DefaultGpuLayerCount > 0)
                        {
                            gpuLayerCount = _aiConfig.Gpu.DefaultGpuLayerCount;
                            _logger.LogInformation($"[AiChatService] Using configured GPU layer count: {gpuLayerCount}");
                        }
                        else
                        {
                            gpuLayerCount = _gpuService.GetOptimalGpuLayerCount(fileInfo.Length);
                            _logger.LogInformation($"[AiChatService] Auto-detected optimal GPU layer count: {gpuLayerCount}");
                        }
                        
                        _logger.LogInformation($"[AiChatService] GPU acceleration enabled with {gpuLayerCount} layers on {_currentGpuInfo.Name}");
                        _gpuEnabled = true;
                    }
                    else
                    {
                        _logger.LogInformation($"[AiChatService] GPU not available or not NVIDIA/CUDA, using CPU");
                        _gpuEnabled = false;
                    }
                }
                else if (!_aiConfig.Gpu.EnableAutoDetection)
                {
                    _logger.LogInformation($"[AiChatService] GPU auto-detection disabled, using CPU");
                    _gpuEnabled = false;
                }
                _currentGpuLayerCount = gpuLayerCount;

                // Load new model with configuration
                _logger.LogInformation($"[AiChatService] Creating ModelParams with GpuLayerCount={gpuLayerCount}...");
                var parameters = new ModelParams(modelPath)
                {
                    ContextSize = (uint)_aiConfig.Models.DefaultContextSize,
                    GpuLayerCount = gpuLayerCount
                    // Seed removed - not available in LLamaSharp 0.18.0
                };

                _logger.LogInformation($"[AiChatService] Loading weights from file...");
                _model = LLamaWeights.LoadFromFile(parameters);
                
                _logger.LogInformation($"[AiChatService] Creating context...");
                _context = _model.CreateContext(parameters);
                
                _logger.LogInformation($"[AiChatService] Creating executor...");
                _logger.LogInformation($"[AiChatService] Context is null? {_context == null}");
                _logger.LogInformation($"[AiChatService] Model is null? {_model == null}");
                
                try 
                {
                    _executor = new InteractiveExecutor(_context);
                    _logger.LogInformation($"[AiChatService] Executor created successfully. Is null? {_executor == null}");
                }
                catch (Exception exExecutor)
                {
                    _logger.LogError(exExecutor, "[AiChatService] Failed to create InteractiveExecutor");
                    throw;
                }

                _currentModelPath = modelPath;
                _currentModelName = System.IO.Path.GetFileNameWithoutExtension(modelPath);
                _currentModelId = modelId ?? _currentModelName;
                
                // Load system prompt from configuration
                if (_configService != null)
                {
                    _systemPrompt = await _configService.GetSystemPromptAsync(_currentModelId);
                    _logger.LogInformation($"[AiChatService] Loaded system prompt for model {_currentModelId}");
                }
                else
                {
                    _systemPrompt = "You are a helpful AI assistant.";
                }
                
                _logger.LogInformation($"[AiChatService] Model loaded successfully: {_currentModelName}");
                _logger.LogInformation($"[AiChatService] IsModelLoaded: {IsModelLoaded()}");
                _logger.LogInformation($"[AiChatService] GPU Enabled: {_gpuEnabled}, Layers: {_currentGpuLayerCount}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[AiChatService] Failed to load model from {modelPath}");
                _logger.LogError($"[AiChatService] Exception type: {ex.GetType().Name}");
                _logger.LogError($"[AiChatService] Exception message: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"[AiChatService] Inner exception: {ex.InnerException.Message}");
                }
                DisposeModel();
                return false;
            }
            finally
            {
                _modelLock.Release();
            }
        }

        public async Task<string> ChatAsync(string prompt)
        {
            if (!IsModelLoaded())
            {
                throw new InvalidOperationException("No model loaded. Please load a model first.");
            }

            await _modelLock.WaitAsync();
            try
            {
                // Prepend system prompt if configured
                var fullPrompt = string.IsNullOrEmpty(_systemPrompt) 
                    ? prompt 
                    : $"System: {_systemPrompt}\n\nUser: {prompt}\n\nAssistant:";

                var inferenceParams = new InferenceParams()
                {
                    Temperature = _aiConfig.Models.Temperature,
                    TopK = 40,
                    TopP = 0.95f,
                    MaxTokens = _aiConfig.Models.MaxTokens,
                    AntiPrompts = new List<string> { "User:", "\nUser:", "\n\nUser:" },
                    SamplingPipeline = new DefaultSamplingPipeline() // Required to avoid NullReferenceException
                };

                var response = string.Empty;
                await foreach (var text in _executor.InferAsync(fullPrompt, inferenceParams))
                {
                    response += text;
                }

                return response;
            }
            finally
            {
                _modelLock.Release();
            }
        }

        public async IAsyncEnumerable<string> StreamChatAsync(string prompt, [EnumeratorCancellation] CancellationToken ct = default)
        {
            _logger.LogInformation($"[AiChatService] StreamChatAsync called with prompt: {prompt}");
            _logger.LogInformation($"[AiChatService] _executor is null? {_executor == null}");
            _logger.LogInformation($"[AiChatService] _context is null? {_context == null}");
            _logger.LogInformation($"[AiChatService] _model is null? {_model == null}");
            
            if (!IsModelLoaded())
            {
                _logger.LogWarning("[AiChatService] IsModelLoaded returned false");
                yield return "⚠️ No model loaded. Please load a model first.";
                yield break;
            }

            if (_executor == null)
            {
                _logger.LogError("[AiChatService] Executor is null despite model being loaded!");
                yield return "Error: Executor not initialized";
                yield break;
            }

            await _modelLock.WaitAsync(ct);
            try
            {
                // Prepend system prompt if configured
                var fullPrompt = string.IsNullOrEmpty(_systemPrompt) 
                    ? prompt 
                    : $"System: {_systemPrompt}\n\nUser: {prompt}\n\nAssistant:";

                var inferenceParams = new InferenceParams()
                {
                    Temperature = _aiConfig.Models.Temperature,
                    TopK = 40,
                    TopP = 0.95f,
                    MaxTokens = _aiConfig.Models.MaxTokens,
                    AntiPrompts = new List<string> { "User:", "\nUser:", "\n\nUser:" },
                    SamplingPipeline = new DefaultSamplingPipeline() // Required to avoid NullReferenceException
                };

                _logger.LogInformation($"[AiChatService] Starting inference with executor...");
                await foreach (var text in _executor.InferAsync(fullPrompt, inferenceParams, ct))
                {
                    if (ct.IsCancellationRequested)
                        break;
                    
                    yield return text;
                }
            }
            finally
            {
                _modelLock.Release();
            }
        }

        private void DisposeModel()
        {
            try
            {
                // InteractiveExecutor doesn't implement IDisposable in newer versions
                _executor = null;
                
                _context?.Dispose();
                _context = null;
                
                _model?.Dispose();
                _model = null;
                
                _currentModelPath = null;
                _currentModelName = null;
                _currentModelId = null;
                _systemPrompt = null;
                _currentGpuLayerCount = 0;
                _gpuEnabled = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing model");
            }
        }

        public void Dispose()
        {
            DisposeModel();
            _modelLock?.Dispose();
        }

        public GpuInfo GetGpuInfo()
        {
            return _currentGpuInfo ?? new GpuInfo { Status = "No GPU information available" };
        }

        public bool IsGpuEnabled()
        {
            return _gpuEnabled;
        }

        public int GetGpuLayerCount()
        {
            return _currentGpuLayerCount;
        }
    }
}