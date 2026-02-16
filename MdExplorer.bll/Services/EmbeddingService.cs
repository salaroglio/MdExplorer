using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.Services;
using LLama;
using LLama.Common;

namespace MdExplorer.Features.Services
{
    /// <summary>
    /// Embedding service using LLamaSharp for local embedding generation.
    /// Uses the same LLamaSharp infrastructure as AiChatService.
    /// </summary>
    public class EmbeddingService : IEmbeddingService, IDisposable
    {
        private readonly ILogger<EmbeddingService> _logger;
        private LLamaWeights _model;
        private LLamaEmbedder _embedder;
        private string _currentModelPath;
        private readonly SemaphoreSlim _modelLock = new SemaphoreSlim(1, 1);
        private int _embeddingDimension;

        public EmbeddingService(ILogger<EmbeddingService> logger)
        {
            _logger = logger;
        }

        public bool IsModelLoaded() => _model != null && _embedder != null;

        public int GetEmbeddingDimension() => _embeddingDimension;

        public string GetCurrentModelPath() => _currentModelPath;

        public Task<bool> LoadModelAsync(string modelPath)
        {
            return LoadModelAsync(modelPath, 4096, 4096, 12000);
        }

        public async Task<bool> LoadModelAsync(string modelPath, int contextSize, int batchSize, int maxEmbeddingChars)
        {
            await _modelLock.WaitAsync();
            try
            {
                _logger.LogInformation("[EmbeddingService] Loading embedding model from: {ModelPath}", modelPath);

                if (!File.Exists(modelPath))
                {
                    _logger.LogError("[EmbeddingService] Model file not found: {ModelPath}", modelPath);
                    return false;
                }

                // Dispose existing model
                DisposeModel();

                var parameters = new ModelParams(modelPath)
                {
                    ContextSize = (uint)contextSize,
                    BatchSize = (uint)batchSize,
                    UBatchSize = (uint)batchSize,
                    GpuLayerCount = 0,
                    Embeddings = true
                };

                _maxEmbeddingChars = maxEmbeddingChars;

                _logger.LogInformation("[EmbeddingService] Loading weights from file...");
                _model = LLamaWeights.LoadFromFile(parameters);

                _logger.LogInformation("[EmbeddingService] Creating embedder...");
                _embedder = new LLamaEmbedder(_model, parameters);
                _currentModelPath = modelPath;

                // Determine embedding dimension by generating a test embedding
                _logger.LogInformation("[EmbeddingService] Generating test embedding to determine dimension...");
                var testResult = await _embedder.GetEmbeddings("test");
                _embeddingDimension = testResult[0].Length;

                _logger.LogInformation("[EmbeddingService] Model loaded successfully. Dimension: {Dim}", _embeddingDimension);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EmbeddingService] Failed to load model: {Message}", ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError("[EmbeddingService] Inner exception: {Inner}", ex.InnerException.Message);
                }
                DisposeModel();
                return false;
            }
            finally
            {
                _modelLock.Release();
            }
        }

        // Max characters to send to the embedding model (~3 chars/token worst case)
        private int _maxEmbeddingChars = 12000;

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            if (!IsModelLoaded())
                throw new InvalidOperationException("Embedding model not loaded. Call LoadModelAsync first.");

            // Truncate text that would exceed the model's context window
            if (text.Length > _maxEmbeddingChars)
            {
                _logger.LogWarning("[EmbeddingService] Truncating text from {Original} to {Max} chars", text.Length, _maxEmbeddingChars);
                text = text.Substring(0, _maxEmbeddingChars);
            }

            // Retry with progressive truncation if batch size is exceeded
            // (token count depends on content type - COBOL/code can have ~2 chars/token)
            const int maxRetries = 3;
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                await _modelLock.WaitAsync();
                try
                {
                    var result = await _embedder.GetEmbeddings(text);
                    return result[0];
                }
                catch (ArgumentException ex) when (attempt < maxRetries && ex.Message.Contains("batch"))
                {
                    // Reduce text by 30% and retry
                    var newLength = (int)(text.Length * 0.7);
                    _logger.LogWarning("[EmbeddingService] Batch size exceeded ({Len} chars), retrying with {NewLen} chars (attempt {Attempt}/{Max})",
                        text.Length, newLength, attempt + 1, maxRetries);
                    text = text.Substring(0, newLength);
                }
                finally
                {
                    _modelLock.Release();
                }
            }

            // Should not reach here, but just in case
            throw new InvalidOperationException("Failed to generate embedding after retries");
        }

        public async Task<float[][]> GenerateEmbeddingsAsync(string[] texts)
        {
            if (!IsModelLoaded())
                throw new InvalidOperationException("Embedding model not loaded. Call LoadModelAsync first.");

            var results = new float[texts.Length][];
            for (int i = 0; i < texts.Length; i++)
            {
                results[i] = await GenerateEmbeddingAsync(texts[i]);
            }
            return results;
        }

        private void DisposeModel()
        {
            try
            {
                _embedder?.Dispose();
                _embedder = null;
                _model?.Dispose();
                _model = null;
                _currentModelPath = null;
                _embeddingDimension = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EmbeddingService] Error disposing embedding model");
            }
        }

        public void Dispose()
        {
            DisposeModel();
            _modelLock?.Dispose();
        }
    }
}
