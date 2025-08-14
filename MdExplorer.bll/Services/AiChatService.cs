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
        Task<bool> LoadModelAsync(string modelPath);
        string GetCurrentModelName();
    }

    public class AiChatService : IAiChatService, IDisposable
    {
        private readonly ILogger<AiChatService> _logger;
        private LLamaWeights _model;
        private LLamaContext _context;
        private InteractiveExecutor _executor;
        private string _currentModelPath;
        private string _currentModelName;
        private readonly SemaphoreSlim _modelLock = new SemaphoreSlim(1, 1);

        public AiChatService(ILogger<AiChatService> logger)
        {
            _logger = logger;
            _logger.LogInformation($"[AiChatService] Service initialized");
            
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

        public async Task<bool> LoadModelAsync(string modelPath)
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

                // Load new model
                _logger.LogInformation($"[AiChatService] Creating ModelParams...");
                var parameters = new ModelParams(modelPath)
                {
                    ContextSize = 4096,
                    GpuLayerCount = 0 // CPU only for compatibility
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
                
                _logger.LogInformation($"[AiChatService] Model loaded successfully: {_currentModelName}");
                _logger.LogInformation($"[AiChatService] IsModelLoaded: {IsModelLoaded()}");
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
                var inferenceParams = new InferenceParams()
                {
                    Temperature = 0.7f,
                    TopK = 40,
                    TopP = 0.95f,
                    MaxTokens = 512,
                    AntiPrompts = new List<string> { "User:", "\nUser:", "\n\nUser:" },
                    SamplingPipeline = new DefaultSamplingPipeline() // Required to avoid NullReferenceException
                };

                var response = string.Empty;
                await foreach (var text in _executor.InferAsync(prompt, inferenceParams))
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
                var inferenceParams = new InferenceParams()
                {
                    Temperature = 0.7f,
                    TopK = 40,
                    TopP = 0.95f,
                    MaxTokens = 512,
                    AntiPrompts = new List<string> { "User:", "\nUser:", "\n\nUser:" },
                    SamplingPipeline = new DefaultSamplingPipeline() // Required to avoid NullReferenceException
                };

                _logger.LogInformation($"[AiChatService] Starting inference with executor...");
                await foreach (var text in _executor.InferAsync(prompt, inferenceParams, ct))
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
    }
}