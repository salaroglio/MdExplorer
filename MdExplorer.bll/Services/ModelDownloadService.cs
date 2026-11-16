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

        /// <summary>
        /// A cosa serve il modello: <c>chat</c>, <c>embedding</c>, <c>speech</c>, <c>ocr</c>. La UI
        /// la usa per mostrare a ogni funzione solo i modelli che le servono.
        /// </summary>
        public string Category { get; set; } = "chat";

        /// <summary>
        /// File che il modello pretende accanto al principale (per un modello che legge immagini, il
        /// proiettore della vista). Il modello è installato solo se ci sono TUTTI: con un file solo
        /// partirebbe e fallirebbe alla prima pagina.
        /// </summary>
        public ModelFile[] Companions { get; set; } = Array.Empty<ModelFile>();

        /// <summary>Quanto si scarica in tutto: file principale più quelli che lo accompagnano.</summary>
        public long TotalSize => FileSize + Companions.Sum(c => c.FileSize);
    }

    public class ModelFile
    {
        public string Url { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
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

        /// <summary>
        /// Percorso del miglior modello di dettatura (Whisper) installato, o <c>null</c> se non ce n'è
        /// nessuno. Preferisce <c>small</c> a <c>base</c>.
        /// </summary>
        string GetInstalledSpeechModelPath();

        /// <summary>
        /// Il modello del catalogo con questo id, con <c>IsInstalled</c> e <c>LocalPath</c> aggiornati,
        /// o <c>null</c> se il catalogo non lo conosce.
        /// </summary>
        ModelInfo FindModel(string modelId);
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
                ["gemma4-e2b"] = new ModelInfo
                {
                    Id = "gemma4-e2b",
                    Name = "Gemma 4 E2B Instruct Q4_K_M",
                    Description = "Google Gemma 4 E2B (2.3B effective params) - on-device multimodal model, 128K context, 140+ languages",
                    Url = "https://huggingface.co/unsloth/gemma-4-E2B-it-GGUF/resolve/main/gemma-4-E2B-it-Q4_K_M.gguf",
                    FileName = "gemma-4-E2B-it-Q4_K_M.gguf",
                    FileSize = 3_340_000_000, // ~3.11 GB
                    ContextLength = 128000,
                    Parameters = "E2B"
                },
                ["gemma4-e4b"] = new ModelInfo
                {
                    Id = "gemma4-e4b",
                    Name = "Gemma 4 E4B Instruct Q4_K_M",
                    Description = "Google Gemma 4 E4B (4.5B effective params) - on-device multimodal model, 128K context, 140+ languages",
                    Url = "https://huggingface.co/unsloth/gemma-4-E4B-it-GGUF/resolve/main/gemma-4-E4B-it-Q4_K_M.gguf",
                    FileName = "gemma-4-E4B-it-Q4_K_M.gguf",
                    FileSize = 5_350_000_000, // ~4.98 GB
                    ContextLength = 128000,
                    Parameters = "E4B"
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
                },
                // Riconoscimento vocale (Whisper). Stessa cartella, stesso download e stessa UI degli
                // altri modelli: per chi li scarica sono tutti "modelli che MdExplorer usa in locale".
                // Numeri misurati il 22/09/2026 su 25 s di parlato italiano, Core Ultra 7 155H.
                ["whisper-small"] = new ModelInfo
                {
                    Id = "whisper-small",
                    Name = "Whisper Small (dettatura)",
                    Description = "Riconoscimento vocale offline, multilingua. L'italiano esce corretto; " +
                                  "trascrive 25 s di parlato in 6 s. È il modello consigliato per dettare.",
                    Url = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-small.bin",
                    FileName = "ggml-small.bin",
                    FileSize = 487_000_000,
                    Parameters = "244M"
                },
                ["whisper-base"] = new ModelInfo
                {
                    Id = "whisper-base",
                    Name = "Whisper Base (dettatura leggera)",
                    Description = "Riconoscimento vocale offline più piccolo e più veloce (25 s di parlato " +
                                  "in 2,6 s), ma sbaglia qualche parola in italiano. Per chi ha poco disco.",
                    Url = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.bin",
                    FileName = "ggml-base.bin",
                    FileSize = 148_000_000,
                    Parameters = "74M"
                },
                // Lettura dei pdf (OCR). Numeri misurati il 15/11/2026 su una pagina A4 a 150 dpi,
                // sola CPU, 8 thread: 27 s a pagina, 1,6 GB di RAM. È l'unico dei tre provati
                // (con PaddleOCR-VL 1.6 e MiniCPM-OCR 1.0) che da una pagina intera restituisce
                // markdown con titoli e tabelle: gli altri due danno testo piatto finché non c'è un
                // modello di impaginazione che ritaglia la pagina in zone.
                ["xiaomi-ocr-0"] = new ModelInfo
                {
                    Id = "xiaomi-ocr-0",
                    Name = "Xiaomi-OCR-0 (lettura dei pdf)",
                    Description = "Legge le pagine di un pdf e le restituisce in markdown, con titoli, " +
                                  "elenchi, tabelle e formule. Lavora in locale, anche senza scheda grafica.",
                    Url = "https://huggingface.co/prithivMLmods/Xiaomi-OCR-0-GGUF/resolve/main/Xiaomi-OCR-0.Q4_K_M.gguf",
                    FileName = "Xiaomi-OCR-0.Q4_K_M.gguf",
                    FileSize = 529_297_664,
                    Parameters = "0.8B",
                    ContextLength = 8192,
                    Category = "ocr",
                    Companions = new[]
                    {
                        new ModelFile
                        {
                            Url = "https://huggingface.co/prithivMLmods/Xiaomi-OCR-0-GGUF/resolve/main/Xiaomi-OCR-0.mmproj-bf16.gguf",
                            FileName = "Xiaomi-OCR-0.mmproj-bf16.gguf",
                            FileSize = 207_346_624
                        }
                    }
                }
            };

            foreach (var id in new[] { "nomic-embed-text", "multilingual-e5-large-instruct" })
            {
                _availableModels[id].Category = "embedding";
            }
            foreach (var id in new[] { "whisper-small", "whisper-base" })
            {
                _availableModels[id].Category = "speech";
            }
        }

        public async Task<ModelInfo[]> GetAvailableModelsAsync()
        {
            var models = _availableModels.Values.ToArray();
            
            // Check which models are installed
            foreach (var model in models)
            {
                model.IsInstalled = IsModelInstalled(model.FileName)
                    && model.Companions.All(c => IsModelInstalled(c.FileName));
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

        public ModelInfo FindModel(string modelId)
        {
            if (modelId == null || !_availableModels.TryGetValue(modelId, out var model))
            {
                return null;
            }
            model.IsInstalled = IsModelInstalled(model.FileName)
                && model.Companions.All(c => IsModelInstalled(c.FileName));
            model.LocalPath = model.IsInstalled ? GetModelPath(model.FileName) : null;
            return model;
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
                // Un modello fatto di più file si cancella intero: il proiettore rimasto solo
                // occuperebbe disco senza che nessuna schermata lo mostri.
                var files = new List<string> { modelName };
                var entry = _availableModels.Values.FirstOrDefault(m => m.FileName == modelName);
                if (entry != null)
                {
                    files.AddRange(entry.Companions.Select(c => c.FileName));
                }

                var deleted = false;
                foreach (var file in files)
                {
                    foreach (var path in new[] { GetModelPath(file), GetModelPath(file) + ".download" })
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                            deleted = true;
                        }
                    }
                }
                if (deleted)
                {
                    _logger.LogInformation($"Deleted model: {modelName}");
                }
                return Task.FromResult(deleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete model: {modelName}");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// I modelli di dettatura, dal migliore al più leggero. <c>small</c> prima di <c>base</c> perché
        /// in italiano <c>base</c> sbaglia parole (misurato: «l'Encyclopedia Libra» invece di
        /// «l'Enciclopedia libera»), e un testo da correggere a mano vale meno dei secondi risparmiati.
        /// </summary>
        private static readonly string[] SpeechModelFiles = new[]
        {
            "ggml-small.bin",
            "ggml-base.bin"
        };

        /// <summary>
        /// Il percorso del miglior modello di dettatura installato, o <c>null</c> se non c'è: chi chiama
        /// deve poter dire all'utente «scaricalo», non fallire e basta.
        /// </summary>
        public string GetInstalledSpeechModelPath()
        {
            foreach (var fileName in SpeechModelFiles)
            {
                var path = GetModelPath(fileName);
                if (File.Exists(path))
                {
                    return path;
                }
            }
            return null;
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

            if (modelInfo.Companions.Length == 0)
            {
                return await DownloadFileAsync(modelId, modelInfo.Name, modelInfo.Url, modelInfo.FileName, modelInfo.FileSize, progress, ct);
            }

            // Più file: l'avanzamento che arriva alla UI è quello dell'insieme, altrimenti la barra
            // tornerebbe a zero a ogni file e «Complete» arriverebbe col primo.
            var files = new List<ModelFile>
            {
                new ModelFile { Url = modelInfo.Url, FileName = modelInfo.FileName, FileSize = modelInfo.FileSize }
            };
            files.AddRange(modelInfo.Companions);
            var total = modelInfo.TotalSize;
            long done = 0;
            for (var n = 0; n < files.Count; n++)
            {
                var file = files[n];
                var offset = done;
                var last = n == files.Count - 1;
                var partial = new SynchronousProgress(p =>
                {
                    var failed = p.Status == "Cancelled" || (p.Status?.StartsWith("Error") ?? false);
                    var finished = p.Status == "Complete" || p.Status == "Already installed";
                    progress?.Report(new DownloadProgress
                    {
                        ModelId = modelId,
                        BytesDownloaded = failed ? 0 : Math.Min(total, offset + (finished ? file.FileSize : p.BytesDownloaded)),
                        TotalBytes = failed ? 0 : total,
                        Status = failed || (finished && last) ? p.Status : "Downloading"
                    });
                });
                if (!await DownloadFileAsync(modelId, modelInfo.Name, file.Url, file.FileName, file.FileSize, partial, ct))
                {
                    return false;
                }
                done += file.FileSize;
            }
            return true;
        }

        /// <summary>
        /// <c>Progress&lt;T&gt;</c> consegna sul pool di thread, quindi fuori ordine: qui l'ordine conta
        /// (l'ultimo stato deve essere davvero l'ultimo), perciò la consegna è immediata.
        /// </summary>
        private sealed class SynchronousProgress : IProgress<DownloadProgress>
        {
            private readonly Action<DownloadProgress> _report;
            public SynchronousProgress(Action<DownloadProgress> report) => _report = report;
            public void Report(DownloadProgress value) => _report(value);
        }

        private async Task<bool> DownloadFileAsync(string modelId, string displayName, string url, string fileName,
            long fileSize, IProgress<DownloadProgress> progress, CancellationToken ct)
        {
            var modelPath = GetModelPath(fileName);
            var tempPath = modelPath + ".download";

            try
            {
                _logger.LogInformation($"Starting download of {displayName} from {url}");
                
                // Check if model already exists
                if (File.Exists(modelPath))
                {
                    _logger.LogInformation($"Model {displayName} already exists at {modelPath}");
                    progress?.Report(new DownloadProgress
                    {
                        ModelId = modelId,
                        BytesDownloaded = fileSize,
                        TotalBytes = fileSize,
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
                    if (startByte >= fileSize)
                    {
                        _logger.LogInformation($"Temp file appears complete or corrupted (size: {startByte}, expected: {fileSize})");
                        
                        // If size matches, just rename it
                        if (startByte == fileSize)
                        {
                            _logger.LogInformation($"Temp file is complete, moving to final location");
                            File.Move(tempPath, modelPath);
                            progress?.Report(new DownloadProgress
                            {
                                ModelId = modelId,
                                BytesDownloaded = fileSize,
                                TotalBytes = fileSize,
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
                        _logger.LogInformation($"Resuming download from byte {startByte} of {fileSize}");
                    }
                }

                // Create HTTP request with range header for resume support
                var request = new HttpRequestMessage(HttpMethod.Get, url);
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
                    var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
                    using var headResponse = await _httpClient.SendAsync(headRequest, ct);
                    var actualSize = headResponse.Content.Headers.ContentLength ?? fileSize;
                    
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
                        return await DownloadFileAsync(modelId, displayName, url, fileName, fileSize, progress, ct);
                    }
                }
                
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? fileSize;
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

                _logger.LogInformation($"Successfully downloaded {displayName}");
                
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
                _logger.LogInformation($"Download cancelled for {displayName}");
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
                _logger.LogError(ex, $"Error downloading {displayName}");
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