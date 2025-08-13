---
author: Carlo Salaroglio
document_type: Document
email: salaroglio@hotmail.com
title: 
date: 10/08/2025
word_section:
  write_toc: false
  document_header: ''
  template_section:
    inherit_from_template: ''
    custom_template: ''
    template_type: default
  predefined_pages: 
---
# 🚀 Sprint 2025-08-10: Chat AI con Qwen3-8B e Download Manager

## 🎯 EVOLUZIONE: Da Chat a AI Agent (Sprint Successivo)

### Architettura Proposta - Approccio Ibrido Pragmatico

**Decisione Architetturale Critica**: Iniziare con tools interni, aggiungere MCP Bridge quando maturo.

#### Fase 1: Tool System Interno (1-2 giorni)
**Obiettivo**: Creare infrastruttura base per tools senza overhead MCP

1. **Interfaccia ITool base**:
   - `ExecuteAsync(params)` con JSON schema per parametri
   - Metadata (nome, descrizione, parametri)
   - Risultati strutturati

2. **ToolExecutor con function calling**:
   - Parser per identificare tool calls nel testo LLM
   - Esecuzione asincrona tools
   - Formattazione risultati per context

3. **Tools nativi prioritari per MdExplorer**:
   - `SearchMarkdownTool` - cerca contenuti/metadata nei file MD
   - `EditYamlHeaderTool` - modifica front matter YAML  
   - `GeneratePlantUMLTool` - crea diagrammi dal contesto
   - `GitAnalysisTool` - analizza history/diff per un file
   - `RefactorLinksTool` - aggiorna link quando sposti file

#### Fase 2: RAG con SQLite Vector Store (2-3 giorni)
**Obiettivo**: Indicizzazione automatica markdown per context enhancement

1. **Embedding Service con ONNX Runtime**:
   - Modello: all-MiniLM-L6-v2 (384 dimensions, 22M parameters)
   - Generazione embeddings per chunks di testo
   - Batch processing per efficienza

2. **SQLite con sqlite-vss extension**:
   - Estensione vettoriale per SQLite esistente
   - Indicizzazione automatica all'apertura progetto
   - Search semantica con top-K retrieval
   - Metadata filtering (per project, date, tags)

3. **RAG Pipeline**:
   - Chunking intelligente dei markdown (rispetta headers)
   - Retrieval context per query
   - Injection nel prompt con relevance scoring

#### Fase 3: Agent Orchestrator (1 settimana)
**Obiettivo**: Coordinamento intelligente tools e generazione risposte

1. **Tool Selection Logic**:
   - Analisi intent utente con pattern matching
   - Selezione tools appropriati
   - Chaining di tools multipli
   - Fallback strategies

2. **Context Management**:
   - Window sliding per modelli piccoli (4K-8K context)
   - Summarization conversazioni lunghe
   - Caching risultati tools
   - History tracking per undo/redo

3. **UI Enhancement**:
   - Visualizzazione real-time tool execution
   - Progress indicators con ETA
   - Error recovery graceful
   - Tool approval mode (opzionale)

#### Fase 4: MCP Bridge (futuro - quando appropriato)
**Trigger**: Quando abbiamo 5+ tools stabili e testati

- Wrapper per server MCP esterni come tools interni
- Discovery dinamica capabilities
- Unified interface per tools interni ed esterni
- Supporto per filesystem, database, API servers MCP

### Architettura Tecnica Proposta

```
AiAgentService
├── ToolSystem/
│   ├── ITool.cs
│   ├── ToolExecutor.cs
│   ├── ToolRegistry.cs
│   └── NativeTools/
│       ├── SearchMarkdownTool.cs
│       ├── EditYamlHeaderTool.cs
│       ├── GeneratePlantUMLTool.cs
│       └── GitAnalysisTool.cs
├── RAG/
│   ├── EmbeddingService.cs (ONNX)
│   ├── VectorStore.cs (SQLite-vss)
│   ├── DocumentChunker.cs
│   └── RetrievalPipeline.cs
├── Orchestration/
│   ├── AgentOrchestrator.cs
│   ├── IntentAnalyzer.cs
│   ├── ContextManager.cs
│   └── ConversationHistory.cs
└── MCP/ (futuro)
    ├── McpBridge.cs
    └── McpServerProxy.cs
```

### Database Schema per Vector Store

```sql
CREATE VIRTUAL TABLE markdown_embeddings USING vss0(
    embedding(384),  -- all-MiniLM-L6-v2 dimension
    chunk_id TEXT PRIMARY KEY,
    file_path TEXT,
    project_id TEXT,
    chunk_text TEXT,
    chunk_position INTEGER,
    metadata JSON
);

CREATE TABLE tool_executions (
    id INTEGER PRIMARY KEY,
    tool_name TEXT,
    parameters JSON,
    result JSON,
    execution_time_ms INTEGER,
    timestamp DATETIME,
    conversation_id TEXT
);
```

### Decisioni Critiche

1. **NO LangChain.NET** - Troppo overhead, meglio controllo diretto
2. **Function Calling locale** - Parser JSON custom per tool calls
3. **Streaming obbligatorio** - User experience real-time
4. **Context window management** - Critico con modelli locali piccoli
5. **SQLite per tutto** - Già presente, no dipendenze extra

### Vantaggi dell'Approccio

✅ **Velocità**: Implementazione incrementale funzionante
✅ **Controllo**: Totale ownership del comportamento
✅ **Debug**: Stack trace chiaro, no black boxes
✅ **Performance**: Ottimizzabile per use case specifico
✅ **Estensibilità**: MCP aggiungibile senza refactoring

### Rischi Mitigati

✅ Evita overhead MCP iniziale
✅ Non dipende da librerie esterne instabili
✅ Compatibile con modelli locali piccoli
✅ Performance prevedibili e ottimizzabili
✅ Fallback graceful se AI non disponibile

### Success Metrics

- Tool execution < 500ms per chiamata
- RAG retrieval < 100ms
- Context relevance > 80% precision
- User task completion rate > 90%
- Zero data loss su tool failures

---

## ✅ IMPLEMENTAZIONE COMPLETATA - SPRINT 1

*Tutto quanto segue è stato completato con successo nel primo sprint (10/08/2025)*

## Obiettivo Sprint

Implementare una chat AI completa in MdExplorer con:

1. Download manager per scaricare modelli da HuggingFace
2. Gestione modelli AI nel componente Settings esistente
3. Chat con Qwen3-8B (5.03 GB)
4. Integrazione con editor per inserire risposte AI

## Scope Sprint

* ✅ Download manager con progress bar
* ✅ Settings component per gestione modelli
* ✅ Chat AI con streaming
* ✅ Pulsante copia/inserisci risposte
* ✅ Support per resume download
* ❌ NO sicurezza avanzata (per ora)
* ❌ NO ottimizzazioni complesse
* ❌ NO MCP (per ora)

## 📦 FASE 1: Setup Backend (45 minuti)

### Task 1.1: Aggiungere pacchetti NuGet necessari

```Shell
cd MdExplorer
dotnet add package LLamaSharp --version 0.18.0
dotnet add package LLamaSharp.Backend.Cpu --version 0.18.0
```

### Task 1.2: Configurare directory e .gitignore

```Shell
# Creare directory modelli (fuori da git)
mkdir -p ~/MdExplorer-Models

# Aggiungere a .gitignore
echo "*.gguf" >> .gitignore
echo "*.ggml" >> .gitignore
echo "AI-Models/" >> .gitignore
echo "~/MdExplorer-Models/" >> .gitignore
```

## 📝 FASE 2: Backend - Download Manager e AI Service (2 ore)

### Task 2.1: Servizio Download Manager

**File**: `MdExplorer.bll/Services/ModelDownloadService.cs`

```C#
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services
{
    public interface IModelDownloadService
    {
        Task<bool> DownloadModelAsync(string modelId, IProgress<DownloadProgress> progress, CancellationToken ct = default);
        Task<ModelInfo[]> GetAvailableModelsAsync();
        Task<ModelInfo[]> GetInstalledModelsAsync();
        bool IsModelInstalled(string modelName);
        string GetModelPath(string modelName);
        Task<bool> DeleteModelAsync(string modelName);
    }

    public class DownloadProgress
    {
        public long BytesDownloaded { get; set; }
        public long TotalBytes { get; set; }
        public double ProgressPercentage { get; set; }
        public string Status { get; set; }
        public double SpeedMBps { get; set; }
        public string ModelId { get; set; }
    }

    public class ModelInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public string Size { get; set; }
        public long SizeBytes { get; set; }
        public bool IsInstalled { get; set; }
        public string LocalPath { get; set; }
        public string Description { get; set; }
        public DateTime? InstalledDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class ModelDownloadService : IModelDownloadService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ModelDownloadService> _logger;
        private readonly IHubContext<AiChatHub> _hubContext;
        private readonly string _modelsDirectory;
        private readonly Dictionary<string, ModelInfo> _availableModels;

        public ModelDownloadService(
            IHttpClientFactory httpClientFactory,
            ILogger<ModelDownloadService> logger,
            IHubContext<AiChatHub> hubContext,
            IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromHours(3);
            _logger = logger;
            _hubContext = hubContext;
            
            _modelsDirectory = configuration["AI:ModelsDirectory"]?.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)) ?? 
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "MdExplorer-Models");
            
            Directory.CreateDirectory(_modelsDirectory);

            // Definizione modelli disponibili
            _availableModels = new Dictionary<string, ModelInfo>
            {
                ["qwen3-8b-q4"] = new ModelInfo
                {
                    Id = "qwen3-8b-q4",
                    Name = "Qwen3 8B Q4_K_M",
                    FileName = "Qwen_Qwen3-8B-Q4_K_M.gguf",
                    Url = "https://huggingface.co/bartowski/Qwen_Qwen3-8B-GGUF/resolve/main/Qwen_Qwen3-8B-Q4_K_M.gguf",
                    Size = "5.03 GB",
                    SizeBytes = 5398996480, // 5.03 * 1024^3
                    Description = "Latest Qwen3 8B model with excellent reasoning and multilingual support"
                },
                ["qwen25-7b-q4"] = new ModelInfo
                {
                    Id = "qwen25-7b-q4",
                    Name = "Qwen2.5 7B Instruct Q4_K_M",
                    FileName = "Qwen2.5-7B-Instruct-Q4_K_M.gguf",
                    Url = "https://huggingface.co/bartowski/Qwen2.5-7B-Instruct-GGUF/resolve/main/Qwen2.5-7B-Instruct-Q4_K_M.gguf",
                    Size = "4.68 GB",
                    SizeBytes = 5026580480, // 4.68 * 1024^3
                    Description = "Qwen 2.5 with improved instruction following and coding abilities"
                },
                ["phi3-mini-q4"] = new ModelInfo
                {
                    Id = "phi3-mini-q4",
                    Name = "Phi-3 Mini Q4",
                    FileName = "Phi-3-mini-4k-instruct-q4.gguf",
                    Url = "https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-gguf/resolve/main/Phi-3-mini-4k-instruct-q4.gguf",
                    Size = "2.7 GB",
                    SizeBytes = 2899102925, // 2.7 * 1024^3
                    Description = "Small and fast model from Microsoft, good for quick responses"
                }
            };
        }

        public async Task<ModelInfo[]> GetAvailableModelsAsync()
        {
            var models = _availableModels.Values.ToArray();
            
            // Check installation status
            foreach (var model in models)
            {
                model.IsInstalled = IsModelInstalled(model.FileName);
                if (model.IsInstalled)
                {
                    model.LocalPath = GetModelPath(model.FileName);
                    var fileInfo = new FileInfo(model.LocalPath);
                    if (fileInfo.Exists)
                    {
                        model.InstalledDate = fileInfo.CreationTime;
                    }
                }
            }
            
            return await Task.FromResult(models);
        }

        public async Task<ModelInfo[]> GetInstalledModelsAsync()
        {
            var models = await GetAvailableModelsAsync();
            return models.Where(m => m.IsInstalled).ToArray();
        }

        public bool IsModelInstalled(string modelName)
        {
            var path = Path.Combine(_modelsDirectory, modelName);
            return File.Exists(path);
        }

        public string GetModelPath(string modelName)
        {
            return Path.Combine(_modelsDirectory, modelName);
        }

        public async Task<bool> DeleteModelAsync(string modelName)
        {
            try
            {
                var path = GetModelPath(modelName);
                if (File.Exists(path))
                {
                    File.Delete(path);
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

        public async Task<bool> DownloadModelAsync(
            string modelId,
            IProgress<DownloadProgress> progress,
            CancellationToken ct = default)
        {
            if (!_availableModels.TryGetValue(modelId, out var modelInfo))
            {
                _logger.LogError($"Model {modelId} not found");
                return false;
            }

            var destinationPath = GetModelPath(modelInfo.FileName);
            
            // Check if already exists
            if (File.Exists(destinationPath))
            {
                _logger.LogInformation($"Model {modelInfo.Name} already exists");
                progress?.Report(new DownloadProgress 
                { 
                    ProgressPercentage = 100, 
                    Status = "Model already installed",
                    ModelId = modelId
                });
                return true;
            }

            try
            {
                _logger.LogInformation($"Starting download of {modelInfo.Name} from {modelInfo.Url}");
                
                // Support resume
                var tempPath = destinationPath + ".downloading";
                long existingLength = 0;
                
                if (File.Exists(tempPath))
                {
                    existingLength = new FileInfo(tempPath).Length;
                    _logger.LogInformation($"Resuming download from {existingLength / (1024*1024):F1} MB");
                }

                using var request = new HttpRequestMessage(HttpMethod.Get, modelInfo.Url);
                if (existingLength > 0)
                {
                    request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(existingLength, null);
                }

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();

                var totalBytes = modelInfo.SizeBytes;
                
                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(tempPath, existingLength > 0 ? FileMode.Append : FileMode.Create);

                var buffer = new byte[81920]; // 80KB buffer
                var totalRead = existingLength;
                var lastUpdate = DateTime.UtcNow;
                var lastBytes = totalRead;

                while (true)
                {
                    var read = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct);
                    if (read == 0) break;

                    await fileStream.WriteAsync(buffer, 0, read, ct);
                    totalRead += read;

                    // Calculate speed and update progress
                    var now = DateTime.UtcNow;
                    var timeDiff = (now - lastUpdate).TotalSeconds;
                    if (timeDiff > 0.5) // Update every 500ms
                    {
                        var bytesDiff = totalRead - lastBytes;
                        var speedMBps = (bytesDiff / timeDiff) / (1024 * 1024);
                        
                        var progressInfo = new DownloadProgress
                        {
                            BytesDownloaded = totalRead,
                            TotalBytes = totalBytes,
                            ProgressPercentage = (totalRead * 100.0) / totalBytes,
                            Status = $"Downloading {modelInfo.Name}... {totalRead / (1024.0 * 1024 * 1024):F2} GB / {totalBytes / (1024.0 * 1024 * 1024):F2} GB",
                            SpeedMBps = speedMBps,
                            ModelId = modelId
                        };
                        
                        progress?.Report(progressInfo);
                        
                        // Notify via SignalR
                        await _hubContext.Clients.All.SendAsync("ModelDownloadProgress", progressInfo, ct);
                        
                        lastUpdate = now;
                        lastBytes = totalRead;
                    }
                }

                // Move temp file to final location
                File.Move(tempPath, destinationPath, true);
                
                _logger.LogInformation($"Successfully downloaded {modelInfo.Name}");
                
                progress?.Report(new DownloadProgress 
                { 
                    ProgressPercentage = 100, 
                    Status = "Download completed!",
                    ModelId = modelId
                });
                
                await _hubContext.Clients.All.SendAsync("ModelDownloadComplete", modelId, ct);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to download {modelInfo.Name}");
                progress?.Report(new DownloadProgress 
                { 
                    Status = $"Error: {ex.Message}",
                    ModelId = modelId
                });
                
                await _hubContext.Clients.All.SendAsync("ModelDownloadError", modelId, ex.Message, ct);
                return false;
            }
        }
    }
}
```

### Task 2.2: Servizio AI Chat

**File**: `MdExplorer.bll/Services/AiChatService.cs`

```C#
using LLama;
using LLama.Common;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
        private LLamaWeights? _model;
        private LLamaContext? _context;
        private InteractiveExecutor? _executor;
        private ModelParams _modelParams;
        private string _currentModelPath = "";
        private string _currentModelName = "";
        private readonly ILogger<AiChatService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IModelDownloadService _downloadService;

        public AiChatService(
            ILogger<AiChatService> logger, 
            IConfiguration configuration,
            IModelDownloadService downloadService)
        {
            _logger = logger;
            _configuration = configuration;
            _downloadService = downloadService;
            
            // Try to load first available model on startup
            _ = LoadFirstAvailableModel();
        }

        private async Task LoadFirstAvailableModel()
        {
            try
            {
                var installedModels = await _downloadService.GetInstalledModelsAsync();
                if (installedModels.Any())
                {
                    var firstModel = installedModels.First();
                    await LoadModelAsync(firstModel.LocalPath);
                }
                else
                {
                    _logger.LogWarning("No AI models installed. Please download a model from Settings.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load initial model");
            }
        }

        public async Task<bool> LoadModelAsync(string modelPath)
        {
            try
            {
                // Dispose previous model if exists
                DisposeModel();

                if (!File.Exists(modelPath))
                {
                    _logger.LogError($"Model file not found: {modelPath}");
                    return false;
                }

                _logger.LogInformation($"Loading AI model from {modelPath}");
                
                _modelParams = new ModelParams(modelPath)
                {
                    ContextSize = 4096,
                    GpuLayerCount = 0, // CPU only for compatibility
                    Seed = 1337,
                    UseMemorymap = true,
                    UseMemoryLock = false
                };

                _model = LLamaWeights.LoadFromFile(_modelParams);
                _context = _model.CreateContext(_modelParams);
                _executor = new InteractiveExecutor(_context);
                
                _currentModelPath = modelPath;
                _currentModelName = Path.GetFileNameWithoutExtension(modelPath);
                
                _logger.LogInformation($"AI model {_currentModelName} loaded successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to load AI model from {modelPath}");
                return false;
            }
        }

        public async Task<string> ChatAsync(string prompt)
        {
            if (_executor == null || _context == null)
            {
                return "⚠️ No AI model loaded. Please download and select a model from Settings.";
            }

            try
            {
                var result = "";
                var inferenceParams = new InferenceParams
                {
                    Temperature = 0.7f,
                    MaxTokens = 1024,
                    TopK = 40,
                    TopP = 0.95f,
                    RepeatPenalty = 1.1f,
                    AntiPrompts = new List<string> { "User:", "Human:", "\n\n" }
                };

                await foreach (var text in _executor.InferAsync(prompt, inferenceParams))
                {
                    result += text;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during inference");
                return $"❌ Error: {ex.Message}";
            }
        }

        public async IAsyncEnumerable<string> StreamChatAsync(
            string prompt,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            if (_executor == null || _context == null)
            {
                yield return "⚠️ No AI model loaded. Please download and select a model from Settings.";
                yield break;
            }

            var inferenceParams = new InferenceParams
            {
                Temperature = 0.7f,
                MaxTokens = 1024,
                TopK = 40,
                TopP = 0.95f,
                RepeatPenalty = 1.1f,
                AntiPrompts = new List<string> { "User:", "Human:", "\n\n" }
            };

            await foreach (var text in _executor.InferAsync(prompt, inferenceParams, ct))
            {
                yield return text;
            }
        }

        public bool IsModelLoaded() => _executor != null && _context != null;

        public string GetCurrentModelName() => _currentModelName;

        private void DisposeModel()
        {
            _executor?.Dispose();
            _executor = null;
            _context?.Dispose();
            _context = null;
            _model?.Dispose();
            _model = null;
        }

        public void Dispose()
        {
            DisposeModel();
        }
    }
}
```

### Task 2.3: Controller API per gestione modelli

**File**: `MdExplorer/Controllers/AiModelsController.cs`

```C#
using Microsoft.AspNetCore.Mvc;
using MdExplorer.Features.Services;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace MdExplorer.Controllers
{
    [ApiController]
    [Route("api/ai-models")]
    public class AiModelsController : ControllerBase
    {
        private readonly IModelDownloadService _downloadService;
        private readonly IAiChatService _aiService;
        private readonly ILogger<AiModelsController> _logger;

        public AiModelsController(
            IModelDownloadService downloadService,
            IAiChatService aiService,
            ILogger<AiModelsController> logger)
        {
            _downloadService = downloadService;
            _aiService = aiService;
            _logger = logger;
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableModels()
        {
            var models = await _downloadService.GetAvailableModelsAsync();
            var currentModel = _aiService.GetCurrentModelName();
            
            // Mark active model
            foreach (var model in models)
            {
                model.IsActive = model.FileName == currentModel + ".gguf";
            }
            
            return Ok(models);
        }

        [HttpGet("installed")]
        public async Task<IActionResult> GetInstalledModels()
        {
            var models = await _downloadService.GetInstalledModelsAsync();
            var currentModel = _aiService.GetCurrentModelName();
            
            foreach (var model in models)
            {
                model.IsActive = model.FileName == currentModel + ".gguf";
            }
            
            return Ok(models);
        }

        [HttpPost("download/{modelId}")]
        public async Task<IActionResult> DownloadModel(string modelId)
        {
            _logger.LogInformation($"Download request for model: {modelId}");
            
            // Start download in background
            _ = Task.Run(async () =>
            {
                var progress = new Progress<DownloadProgress>(p =>
                {
                    _logger.LogDebug($"Download progress: {p.ProgressPercentage:F1}%");
                });
                
                await _downloadService.DownloadModelAsync(modelId, progress);
            });

            return Ok(new { message = "Download started", modelId });
        }

        [HttpPost("activate")]
        public async Task<IActionResult> ActivateModel([FromBody] ActivateModelRequest request)
        {
            var modelPath = _downloadService.GetModelPath(request.FileName);
            
            if (!System.IO.File.Exists(modelPath))
            {
                return NotFound($"Model file {request.FileName} not found");
            }

            var success = await _aiService.LoadModelAsync(modelPath);
            
            if (success)
            {
                return Ok(new { 
                    message = "Model activated successfully", 
                    modelName = request.FileName 
                });
            }
            else
            {
                return StatusCode(500, new { 
                    message = "Failed to load model" 
                });
            }
        }

        [HttpDelete("{modelId}")]
        public async Task<IActionResult> DeleteModel(string modelId)
        {
            var models = await _downloadService.GetAvailableModelsAsync();
            var model = models.FirstOrDefault(m => m.Id == modelId);
            
            if (model == null)
            {
                return NotFound($"Model {modelId} not found");
            }

            var deleted = await _downloadService.DeleteModelAsync(model.FileName);
            
            if (deleted)
            {
                return Ok(new { message = "Model deleted successfully" });
            }
            else
            {
                return StatusCode(500, new { message = "Failed to delete model" });
            }
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                modelLoaded = _aiService.IsModelLoaded(),
                currentModel = _aiService.GetCurrentModelName(),
                modelsDirectory = _downloadService.GetModelPath("")
            });
        }
    }

    public class ActivateModelRequest
    {
        public string FileName { get; set; }
    }
}
```

### Task 2.4: SignalR Hub per Chat e Download Progress

**File**: `MdExplorer/Hubs/AiChatHub.cs`

```C#
using Microsoft.AspNetCore.SignalR;
using MdExplorer.Features.Services;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Hubs
{
    public class AiChatHub : Hub
    {
        private readonly IAiChatService _aiService;
        private readonly IModelDownloadService _downloadService;
        private readonly ILogger<AiChatHub> _logger;

        public AiChatHub(
            IAiChatService aiService,
            IModelDownloadService downloadService,
            ILogger<AiChatHub> logger)
        {
            _aiService = aiService;
            _downloadService = downloadService;
            _logger = logger;
        }

        public async IAsyncEnumerable<string> StreamChat(
            string message,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Chat request: {message}");

            if (!_aiService.IsModelLoaded())
            {
                yield return "⚠️ No AI model loaded. Please download and activate a model from Settings.";
                yield break;
            }

            // Format prompt
            var prompt = $"User: {message}\nAssistant:";

            await foreach (var chunk in _aiService.StreamChatAsync(prompt, cancellationToken))
            {
                yield return chunk;
            }
        }

        public async Task<object> GetModelsStatus()
        {
            var available = await _downloadService.GetAvailableModelsAsync();
            var installed = await _downloadService.GetInstalledModelsAsync();
            
            return new
            {
                available = available,
                installed = installed,
                currentModel = _aiService.GetCurrentModelName(),
                modelLoaded = _aiService.IsModelLoaded()
            };
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
            
            // Send initial status
            var status = await GetModelsStatus();
            await Clients.Caller.SendAsync("ModelsStatus", status);
            
            await base.OnConnectedAsync();
        }
    }
}
```

### Task 2.5: Aggiornare Startup.cs

**File**: `MdExplorer/Startup.cs`

Aggiungere in `ConfigureServices`:

```C#
// HTTP Client per download
services.AddHttpClient();

// AI Services
services.AddSingleton<IModelDownloadService, ModelDownloadService>();
services.AddSingleton<IAiChatService, AiChatService>();

// SignalR già configurato, aggiungere solo l'hub in Configure:
app.UseEndpoints(endpoints =>
{
    // ... altri endpoints
    endpoints.MapHub<AiChatHub>("/hubs/ai-chat");
});
```

### Task 2.6: Configurazione appsettings.json

**File**: `MdExplorer/appsettings.json`

```JSON
{
  // ... configurazione esistente
  "AI": {
    "Enabled": true,
    "ModelsDirectory": "~/MdExplorer-Models",
    "DefaultModel": "Qwen_Qwen3-8B-Q4_K_M.gguf"
  }
}
```

## 🎨 FASE 3: Frontend - Settings Component e Chat (2.5 ore)

### Task 3.1: Servizio AI per Angular

**File**: `MdExplorer/client2/src/app/services/ai.service.ts`

```TypeScript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { Observable, Subject, BehaviorSubject } from 'rxjs';

export interface ModelInfo {
  id: string;
  name: string;
  fileName: string;
  size: string;
  sizeBytes: number;
  isInstalled: boolean;
  isActive: boolean;
  description: string;
  installedDate?: Date;
}

export interface DownloadProgress {
  bytesDownloaded: number;
  totalBytes: number;
  progressPercentage: number;
  status: string;
  speedMBps: number;
  modelId: string;
}

export interface ChatMessage {
  role: 'user' | 'assistant' | 'system';
  content: string;
  timestamp: Date;
  isStreaming?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AiService {
  private hubConnection: signalR.HubConnection;
  private chatMessages$ = new Subject<ChatMessage>();
  private downloadProgress$ = new Subject<DownloadProgress>();
  private modelsStatus$ = new BehaviorSubject<any>(null);
  private currentStreamMessage: ChatMessage | null = null;

  constructor(private http: HttpClient) {
    this.initializeConnection();
  }

  private initializeConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/ai-chat')
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => console.log('AI Hub connected'))
      .catch(err => console.error('AI Hub connection error:', err));

    // Event handlers
    this.hubConnection.on('Connected', (connectionId: string) => {
      console.log('Connected with ID:', connectionId);
    });

    this.hubConnection.on('ModelsStatus', (status: any) => {
      this.modelsStatus$.next(status);
    });

    this.hubConnection.on('ModelDownloadProgress', (progress: DownloadProgress) => {
      this.downloadProgress$.next(progress);
    });

    this.hubConnection.on('ModelDownloadComplete', (modelId: string) => {
      console.log('Download complete:', modelId);
      this.refreshModels();
    });

    this.hubConnection.on('ModelDownloadError', (modelId: string, error: string) => {
      console.error('Download error:', modelId, error);
    });
  }

  // Models management
  getAvailableModels(): Observable<ModelInfo[]> {
    return this.http.get<ModelInfo[]>('/api/ai-models/available');
  }

  getInstalledModels(): Observable<ModelInfo[]> {
    return this.http.get<ModelInfo[]>('/api/ai-models/installed');
  }

  downloadModel(modelId: string): Observable<any> {
    return this.http.post(`/api/ai-models/download/${modelId}`, {});
  }

  activateModel(fileName: string): Observable<any> {
    return this.http.post('/api/ai-models/activate', { fileName });
  }

  deleteModel(modelId: string): Observable<any> {
    return this.http.delete(`/api/ai-models/${modelId}`);
  }

  getDownloadProgress(): Observable<DownloadProgress> {
    return this.downloadProgress$.asObservable();
  }

  getModelsStatus(): Observable<any> {
    return this.modelsStatus$.asObservable();
  }

  refreshModels(): void {
    this.hubConnection.invoke('GetModelsStatus')
      .then(status => this.modelsStatus$.next(status))
      .catch(err => console.error('Failed to get models status:', err));
  }

  // Chat functionality
  sendMessage(message: string): Observable<ChatMessage> {
    // Add user message
    const userMessage: ChatMessage = {
      role: 'user',
      content: message,
      timestamp: new Date()
    };
    this.chatMessages$.next(userMessage);

    // Prepare assistant message
    this.currentStreamMessage = {
      role: 'assistant',
      content: '',
      timestamp: new Date(),
      isStreaming: true
    };
    this.chatMessages$.next(this.currentStreamMessage);

    // Stream response
    this.hubConnection.stream<string>('StreamChat', message)
      .subscribe({
        next: (chunk: string) => {
          if (this.currentStreamMessage) {
            this.currentStreamMessage.content += chunk;
            this.chatMessages$.next(this.currentStreamMessage);
          }
        },
        complete: () => {
          if (this.currentStreamMessage) {
            this.currentStreamMessage.isStreaming = false;
            this.chatMessages$.next(this.currentStreamMessage);
            this.currentStreamMessage = null;
          }
        },
        error: (err) => {
          console.error('Stream error:', err);
          if (this.currentStreamMessage) {
            this.currentStreamMessage.content += '\n❌ Error during generation';
            this.currentStreamMessage.isStreaming = false;
            this.chatMessages$.next(this.currentStreamMessage);
            this.currentStreamMessage = null;
          }
        }
      });

    return this.chatMessages$.asObservable();
  }

  getChatMessages(): Observable<ChatMessage> {
    return this.chatMessages$.asObservable();
  }
}
```

### Task 3.2: Componente Settings per AI

**File**: `MdExplorer/client2/src/app/components/settings/ai-settings.component.ts`

```TypeScript
import { Component, OnInit, OnDestroy } from '@angular/core';
import { AiService, ModelInfo, DownloadProgress } from '../../services/ai.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-ai-settings',
  templateUrl: './ai-settings.component.html',
  styleUrls: ['./ai-settings.component.scss']
})
export class AiSettingsComponent implements OnInit, OnDestroy {
  availableModels: ModelInfo[] = [];
  downloadProgress: { [key: string]: DownloadProgress } = {};
  subscriptions: Subscription[] = [];
  loading = false;

  constructor(private aiService: AiService) {}

  ngOnInit(): void {
    this.loadModels();
    
    // Subscribe to download progress
    const progressSub = this.aiService.getDownloadProgress().subscribe(progress => {
      this.downloadProgress[progress.modelId] = progress;
    });
    this.subscriptions.push(progressSub);

    // Subscribe to models status updates
    const statusSub = this.aiService.getModelsStatus().subscribe(status => {
      if (status) {
        this.availableModels = status.available;
      }
    });
    this.subscriptions.push(statusSub);
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadModels(): void {
    this.loading = true;
    this.aiService.getAvailableModels().subscribe({
      next: (models) => {
        this.availableModels = models;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load models:', err);
        this.loading = false;
      }
    });
  }

  downloadModel(model: ModelInfo): void {
    if (this.downloadProgress[model.id]) {
      return; // Already downloading
    }

    this.aiService.downloadModel(model.id).subscribe({
      next: (response) => {
        console.log('Download started:', response);
      },
      error: (err) => {
        console.error('Failed to start download:', err);
        delete this.downloadProgress[model.id];
      }
    });
  }

  activateModel(model: ModelInfo): void {
    this.aiService.activateModel(model.fileName).subscribe({
      next: (response) => {
        console.log('Model activated:', response);
        this.loadModels(); // Refresh list
      },
      error: (err) => {
        console.error('Failed to activate model:', err);
      }
    });
  }

  deleteModel(model: ModelInfo): void {
    if (confirm(`Delete model ${model.name}?`)) {
      this.aiService.deleteModel(model.id).subscribe({
        next: (response) => {
          console.log('Model deleted:', response);
          this.loadModels(); // Refresh list
        },
        error: (err) => {
          console.error('Failed to delete model:', err);
        }
      });
    }
  }

  formatBytes(bytes: number): string {
    const gb = bytes / (1024 * 1024 * 1024);
    return `${gb.toFixed(2)} GB`;
  }

  getDownloadSpeed(modelId: string): string {
    const progress = this.downloadProgress[modelId];
    if (progress && progress.speedMBps) {
      return `${progress.speedMBps.toFixed(1)} MB/s`;
    }
    return '';
  }
}
```

### Task 3.3: Template Settings AI

**File**: `MdExplorer/client2/src/app/components/settings/ai-settings.component.html`

```HTML
<div class="ai-settings-container">
  <h2>AI Models Management</h2>
  
  <div class="models-info">
    <p>Download and manage AI models for local chat functionality.</p>
    <p class="models-path">Models directory: ~/MdExplorer-Models/</p>
  </div>

  <div class="models-list" *ngIf="!loading">
    <div class="model-card" *ngFor="let model of availableModels">
      <div class="model-header">
        <h3>{{ model.name }}</h3>
        <span class="model-size">{{ model.size }}</span>
      </div>
      
      <p class="model-description">{{ model.description }}</p>
      
      <div class="model-status">
        <span class="status-badge" [ngClass]="{
          'installed': model.isInstalled,
          'active': model.isActive,
          'not-installed': !model.isInstalled
        }">
          <span *ngIf="model.isActive">✓ Active</span>
          <span *ngIf="model.isInstalled && !model.isActive">Installed</span>
          <span *ngIf="!model.isInstalled">Not Installed</span>
        </span>
        
        <span class="installed-date" *ngIf="model.installedDate">
          Installed: {{ model.installedDate | date:'short' }}
        </span>
      </div>

      <!-- Download Progress -->
      <div class="download-progress" *ngIf="downloadProgress[model.id]">
        <div class="progress-bar">
          <div class="progress-fill" 
               [style.width.%]="downloadProgress[model.id].progressPercentage"></div>
        </div>
        <div class="progress-info">
          <span>{{ downloadProgress[model.id].status }}</span>
          <span class="speed">{{ getDownloadSpeed(model.id) }}</span>
        </div>
      </div>

      <!-- Actions -->
      <div class="model-actions">
        <button class="btn btn-primary" 
                *ngIf="!model.isInstalled && !downloadProgress[model.id]"
                (click)="downloadModel(model)">
          Download
        </button>
        
        <button class="btn btn-success" 
                *ngIf="model.isInstalled && !model.isActive"
                (click)="activateModel(model)">
          Activate
        </button>
        
        <button class="btn btn-danger" 
                *ngIf="model.isInstalled && !model.isActive"
                (click)="deleteModel(model)">
          Delete
        </button>
        
        <button class="btn btn-secondary" 
                *ngIf="downloadProgress[model.id]"
                disabled>
          Downloading...
        </button>
      </div>
    </div>
  </div>

  <div class="loading" *ngIf="loading">
    Loading models...
  </div>
</div>
```

### Task 3.4: Stili Settings AI

**File**: `MdExplorer/client2/src/app/components/settings/ai-settings.component.scss`

```SCSS
.ai-settings-container {
  padding: 20px;
  max-width: 800px;
  margin: 0 auto;

  h2 {
    color: #333;
    margin-bottom: 20px;
  }

  .models-info {
    background: #f5f5f5;
    padding: 15px;
    border-radius: 8px;
    margin-bottom: 20px;

    p {
      margin: 5px 0;
    }

    .models-path {
      font-family: monospace;
      color: #666;
      font-size: 14px;
    }
  }

  .models-list {
    display: flex;
    flex-direction: column;
    gap: 20px;
  }

  .model-card {
    background: white;
    border: 1px solid #ddd;
    border-radius: 8px;
    padding: 20px;
    transition: box-shadow 0.3s;

    &:hover {
      box-shadow: 0 4px 12px rgba(0,0,0,0.1);
    }

    .model-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 10px;

      h3 {
        margin: 0;
        color: #333;
      }

      .model-size {
        background: #e0e0e0;
        padding: 4px 12px;
        border-radius: 12px;
        font-size: 14px;
        font-weight: 500;
      }
    }

    .model-description {
      color: #666;
      margin-bottom: 15px;
      font-size: 14px;
    }

    .model-status {
      display: flex;
      align-items: center;
      gap: 15px;
      margin-bottom: 15px;

      .status-badge {
        padding: 6px 12px;
        border-radius: 6px;
        font-size: 13px;
        font-weight: 500;

        &.installed {
          background: #e8f5e9;
          color: #2e7d32;
        }

        &.active {
          background: #4caf50;
          color: white;
        }

        &.not-installed {
          background: #f5f5f5;
          color: #999;
        }
      }

      .installed-date {
        font-size: 12px;
        color: #999;
      }
    }

    .download-progress {
      margin-bottom: 15px;

      .progress-bar {
        height: 24px;
        background: #f0f0f0;
        border-radius: 12px;
        overflow: hidden;
        margin-bottom: 8px;

        .progress-fill {
          height: 100%;
          background: linear-gradient(90deg, #667eea 0%, #764ba2 100%);
          transition: width 0.3s ease;
        }
      }

      .progress-info {
        display: flex;
        justify-content: space-between;
        font-size: 13px;
        color: #666;

        .speed {
          color: #667eea;
          font-weight: 500;
        }
      }
    }

    .model-actions {
      display: flex;
      gap: 10px;

      .btn {
        padding: 8px 16px;
        border: none;
        border-radius: 6px;
        font-size: 14px;
        font-weight: 500;
        cursor: pointer;
        transition: all 0.3s;

        &.btn-primary {
          background: #667eea;
          color: white;

          &:hover {
            background: #5a67d8;
          }
        }

        &.btn-success {
          background: #4caf50;
          color: white;

          &:hover {
            background: #45a049;
          }
        }

        &.btn-danger {
          background: #f44336;
          color: white;

          &:hover {
            background: #da190b;
          }
        }

        &.btn-secondary {
          background: #ccc;
          color: #666;
          cursor: not-allowed;
        }

        &:disabled {
          opacity: 0.6;
          cursor: not-allowed;
        }
      }
    }
  }

  .loading {
    text-align: center;
    color: #666;
    padding: 40px;
  }
}
```

### Task 3.5: Componente Chat

**File**: `MdExplorer/client2/src/app/components/ai-chat/ai-chat.component.ts`

```TypeScript
import { Component, OnInit, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { AiService, ChatMessage } from '../../services/ai.service';

@Component({
  selector: 'app-ai-chat',
  templateUrl: './ai-chat.component.html',
  styleUrls: ['./ai-chat.component.scss']
})
export class AiChatComponent implements OnInit, AfterViewChecked {
  @ViewChild('scrollContainer') private scrollContainer: ElementRef;
  
  messages: ChatMessage[] = [];
  currentInput = '';
  isGenerating = false;
  showChat = false;

  constructor(private aiService: AiService) {}

  ngOnInit(): void {
    this.aiService.getChatMessages().subscribe(message => {
      const index = this.messages.findIndex(m => 
        m.timestamp === message.timestamp && m.role === message.role
      );
      
      if (index >= 0) {
        this.messages[index] = message;
      } else {
        this.messages.push(message);
      }
      
      this.isGenerating = message.isStreaming || false;
    });
  }

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  toggleChat(): void {
    this.showChat = !this.showChat;
  }

  sendMessage(): void {
    if (!this.currentInput.trim() || this.isGenerating) return;

    const message = this.currentInput;
    this.currentInput = '';
    this.isGenerating = true;
    
    this.aiService.sendMessage(message);
  }

  copyToClipboard(content: string): void {
    navigator.clipboard.writeText(content).then(() => {
      this.showToast('Copied to clipboard!');
    });
  }

  insertIntoDocument(content: string): void {
    // Emit event for document insertion
    window.dispatchEvent(new CustomEvent('insert-ai-content', {
      detail: { content }
    }));
    this.showToast('Content inserted into document');
  }

  private scrollToBottom(): void {
    try {
      if (this.scrollContainer) {
        this.scrollContainer.nativeElement.scrollTop = 
          this.scrollContainer.nativeElement.scrollHeight;
      }
    } catch(err) {}
  }

  private showToast(message: string): void {
    // Simple toast notification (can be replaced with proper toast library)
    console.log(message);
  }
}
```

### Task 3.6: Integrazione in Settings esistente

Aggiungere tab per AI Settings nel componente settings esistente:

```TypeScript
// In settings.component.html, aggiungere:
<mat-tab-group>
  <!-- Altri tab esistenti -->
  <mat-tab label="AI Models">
    <app-ai-settings></app-ai-settings>
  </mat-tab>
</mat-tab-group>
```

## 🧪 FASE 4: Test e Integrazione (1 ora)

### Task 4.1: Test download modello

1. Avviare backend: `dotnet run`
2. Avviare frontend: `npm start`
3. Aprire Settings → AI Models
4. Cliccare "Download" su Qwen3-8B
5. Verificare progress bar e velocità download

### Task 4.2: Test chat

1. Dopo download completato, cliccare "Activate"
2. Aprire chat con FAB button
3. Scrivere: "Hello, can you help me write a README?"
4. Verificare streaming risposta
5. Testare pulsanti Copy e Insert

### Task 4.3: Test resume download

1. Iniziare download
2. Interrompere a metà (chiudere app)
3. Riavviare e riprendere download
4. Verificare che riprenda da dove interrotto

## 📊 Definition of Done

* ✅ Download manager funzionante con progress bar
* ✅ Settings component mostra modelli disponibili/installati
* ✅ Download di Qwen3-8B (5.03 GB) completato
* ✅ Chat AI con streaming funzionante
* ✅ Pulsanti copy/insert operativi
* ✅ Resume download supportato
* ✅ Switching tra modelli funzionante

## 🎯 Success Metrics

* Download speed > 1 MB/s (dipende da connessione)
* Tempo prima risposta chat < 3 secondi
* Streaming fluido senza interruzioni
* Zero crash durante uso normale
* UI responsive durante download

## 📝 Note Finali

**Cosa abbiamo implementato**:

* ✅ Download manager completo con resume
* ✅ Gestione modelli in Settings
* ✅ Chat AI con Qwen3-8B
* ✅ Streaming responses
* ✅ Copy/Insert integration

**Cosa NON abbiamo fatto** (per velocità):

* ❌ GPU acceleration (solo CPU)
* ❌ Conversazioni persistenti
* ❌ Context management avanzato
* ❌ MCP integration
* ❌ Fine-tuning UI

**Troubleshooting comuni**:

1. **"Model file too large"**: Assicurarsi di avere almeno 10GB liberi
2. **"Failed to load model"**: Verificare RAM disponibile (serve almeno 8GB)
3. **"Slow inference"**: Normale su CPU, considerare GPU in futuro
4. **"Download interrupted"**: Riavviare app, riprenderà automaticamente

## 🚦 Ready to Start!

Tempo stimato: **6-7 ore**

* Setup: 45 min
* Backend: 2 ore
* Frontend: 2.5 ore
* Test: 1 ora
* Buffer: 45 min

Iniziare dal Task 1.1 e procedere sequenzialmente. Buon coding! 🎉
