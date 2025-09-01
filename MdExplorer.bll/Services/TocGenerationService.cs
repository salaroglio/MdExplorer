using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Services;
using MdExplorer.Features.Yaml.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services
{
    public class TocGenerationProgress
    {
        public string Directory { get; set; }
        public int Processed { get; set; }
        public int Total { get; set; }
        public string Status { get; set; }
        public int PercentComplete { get; set; }
    }

    public interface ITocGenerationService
    {
        Task<bool> GenerateTocWithAIAsync(string directoryPath, string tocFilePath, CancellationToken ct = default);
        Task<bool> ForceRegenerateTocAsync(string directoryPath, string tocFilePath, CancellationToken ct = default);
        Task<bool> RefreshTocAsync(string tocFilePath, CancellationToken ct = default);
        Task<string> GenerateQuickTocAsync(string directoryPath, string tocFilePath);
        event EventHandler<TocGenerationProgress> ProgressChanged;
        event EventHandler<string> GenerationCompleted;
    }

    public class TocGenerationService : ITocGenerationService
    {
        private readonly ILogger<TocGenerationService> _logger;
        private readonly IUserSettingsDB _userSettingsDB;
        private readonly IAiChatService _aiChatService;
        private readonly IGeminiApiService _geminiService;
        private readonly IConfiguration _configuration;
        private readonly IYamlDefaultGenerator _yamlDefaultGenerator;
        private const int DEFAULT_BATCH_SIZE = 10;
        private const int DEFAULT_CACHE_DAYS = 30;
        private bool _useGemini = false;
        private string _geminiModel = "gemini-1.5-flash";

        public event EventHandler<TocGenerationProgress> ProgressChanged;
        public event EventHandler<string> GenerationCompleted;

        public TocGenerationService(
            ILogger<TocGenerationService> logger,
            IUserSettingsDB userSettingsDB,
            IAiChatService aiChatService,
            IGeminiApiService geminiService,
            IConfiguration configuration,
            IYamlDefaultGenerator yamlDefaultGenerator)
        {
            _logger = logger;
            _userSettingsDB = userSettingsDB;
            _aiChatService = aiChatService;
            _geminiService = geminiService;
            _configuration = configuration;
            _yamlDefaultGenerator = yamlDefaultGenerator;
        }

        public async Task<bool> GenerateTocWithAIAsync(string directoryPath, string tocFilePath, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation($"[TocGeneration] Starting AI TOC generation for: {directoryPath}");

                // Check if TOC file already exists
                if (File.Exists(tocFilePath))
                {
                    _logger.LogInformation($"[TocGeneration] TOC file already exists at: {tocFilePath}, skipping generation");
                    
                    // Notify completion even when skipping generation
                    NotifyCompletion(directoryPath);
                    
                    return true; // Return true since the file exists
                }

                // Check if AI is available (either local model or Gemini)
                var isAiAvailable = await IsAiAvailableAsync();
                if (!isAiAvailable)
                {
                    _logger.LogWarning("[TocGeneration] No AI model available (neither local nor Gemini), falling back to simple TOC");
                    await GenerateSimpleTocWithWarning(directoryPath, tocFilePath);
                    return false;
                }

                // Get configuration - now with fresh session
                var batchSize = GetBatchSize();
                var enableCache = IsCacheEnabled();
                var prompt = GetTocPrompt();

                // Get all .md files (excluding .md.directory files)
                var mdFiles = Directory.GetFiles(directoryPath, "*.md", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.EndsWith(".md.directory"))
                    .ToList();

                _logger.LogInformation($"[TocGeneration] Found {mdFiles.Count} markdown files to process");

                // Initialize TOC file with header
                await InitializeTocFile(tocFilePath, directoryPath, mdFiles.Count);

                // Process files in batches
                var tableContent = new StringBuilder();
                tableContent.AppendLine("| Documento | Descrizione |");
                tableContent.AppendLine("|-----------|-------------|");

                var cardsContent = new StringBuilder();
                cardsContent.AppendLine("\n## 📚 Dettagli Documenti\n");

                for (int i = 0; i < mdFiles.Count; i += batchSize)
                {
                    if (ct.IsCancellationRequested)
                    {
                        _logger.LogInformation("[TocGeneration] Generation cancelled by user");
                        return false;
                    }

                    var batch = mdFiles.Skip(i).Take(batchSize).ToList();
                    var batchNumber = (i / batchSize) + 1;
                    var totalBatches = (mdFiles.Count + batchSize - 1) / batchSize;

                    // Send progress notification
                    NotifyProgress(directoryPath, i, mdFiles.Count, $"Analizzando batch {batchNumber}/{totalBatches}");

                    // Process batch
                    foreach (var filePath in batch)
                    {
                        var fileName = Path.GetFileName(filePath);
                        var relativePath = GetRelativePath(directoryPath, filePath);
                        
                        _logger.LogInformation($"[TocGeneration] Processing file: {fileName}");
                        
                        // Get description (from cache or AI)
                        var description = await GetFileDescriptionAsync(filePath, prompt, enableCache);
                        
                        _logger.LogInformation($"[TocGeneration] Got description for {fileName}: {description?.Substring(0, Math.Min(description?.Length ?? 0, 100))}...");
                        
                        // Add to table (ensure description is single-line for table format)
                        var tableDescription = description?.Replace("\n", " ")?.Replace("\r", " ")?.Replace("|", "\\|");
                        tableContent.AppendLine($"| [{fileName}]({relativePath}) | {tableDescription} |");
                        
                        // Add to cards
                        cardsContent.AppendLine(GenerateFileCard(filePath, relativePath, description));
                    }
                }

                // Write complete TOC file
                await FinalizeTocFile(tocFilePath, tableContent.ToString(), cardsContent.ToString());

                // Send completion notification
                NotifyCompletion(directoryPath);

                _logger.LogInformation($"[TocGeneration] TOC generation completed successfully for: {directoryPath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocGeneration] Error generating TOC: {ex.Message}", ex);
                
                // Send completion notification even on error to close the progress dialog
                NotifyCompletion(directoryPath);
                
                await GenerateErrorToc(tocFilePath, ex.Message);
                return false;
            }
        }

        public async Task<bool> ForceRegenerateTocAsync(string directoryPath, string tocFilePath, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation($"[TocGeneration] Force regenerating TOC for: {directoryPath}");

                // Delete existing file if present
                if (File.Exists(tocFilePath))
                {
                    _logger.LogInformation($"[TocGeneration] Deleting existing TOC file: {tocFilePath}");
                    File.Delete(tocFilePath);
                }

                // Now regenerate from scratch
                return await GenerateTocWithAIInternalAsync(directoryPath, tocFilePath, ct, forceNoCache: false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocGeneration] Error force regenerating TOC: {ex.Message}", ex);
                await GenerateErrorToc(tocFilePath, ex.Message);
                return false;
            }
        }

        public async Task<bool> RefreshTocAsync(string tocFilePath, CancellationToken ct = default)
        {
            if (!File.Exists(tocFilePath))
            {
                _logger.LogWarning($"[TocGeneration] TOC file not found: {tocFilePath}");
                return false;
            }

            var directoryPath = Path.GetDirectoryName(tocFilePath);
            
            // For now, still regenerate completely but preserve custom content in future
            // TODO: Implement logic to preserve user modifications
            _logger.LogInformation($"[TocGeneration] Refreshing TOC with AI (bypassing cache): {tocFilePath}");
            
            // Delete and regenerate, but force bypass cache for refresh
            File.Delete(tocFilePath);
            
            // Temporarily disable cache for refresh operation
            var originalCacheState = IsCacheEnabled();
            if (originalCacheState)
            {
                _logger.LogInformation("[TocGeneration] Temporarily disabling cache for refresh operation");
                // We need a way to disable cache temporarily
                // For now, we'll modify the internal method to accept a cache override
            }
            
            return await GenerateTocWithAIInternalAsync(directoryPath, tocFilePath, ct, forceNoCache: true);
        }

        private async Task<bool> GenerateTocWithAIInternalAsync(string directoryPath, string tocFilePath, CancellationToken ct = default, bool forceNoCache = false)
        {
            try
            {
                // Check if AI is available (either local model or Gemini)
                var isAiAvailable = await IsAiAvailableAsync();
                if (!isAiAvailable)
                {
                    _logger.LogWarning("[TocGeneration] No AI model available (neither local nor Gemini), falling back to simple TOC");
                    await GenerateSimpleTocWithWarning(directoryPath, tocFilePath);
                    return false;
                }

                // Get configuration - now with fresh session
                var batchSize = GetBatchSize();
                var enableCache = IsCacheEnabled() && !forceNoCache; // Respect forceNoCache flag
                var prompt = GetTocPrompt();
                
                if (forceNoCache)
                {
                    _logger.LogInformation("[TocGeneration] Cache disabled for this operation (forced refresh)");
                }

                // Get all .md files (excluding .md.directory files)
                var mdFiles = Directory.GetFiles(directoryPath, "*.md", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.EndsWith(".md.directory"))
                    .ToList();

                _logger.LogInformation($"[TocGeneration] Found {mdFiles.Count} markdown files to process");

                // Initialize TOC file with header
                await InitializeTocFile(tocFilePath, directoryPath, mdFiles.Count);

                // Process files in batches
                var tableContent = new StringBuilder();
                tableContent.AppendLine("| Titolo | Descrizione | Link |");
                tableContent.AppendLine("|--------|-------------|------|");

                var cardsContent = new StringBuilder();
                cardsContent.AppendLine();
                cardsContent.AppendLine("## 📚 Dettagli Documenti");
                cardsContent.AppendLine();

                int processedCount = 0;
                for (int i = 0; i < mdFiles.Count; i += batchSize)
                {
                    if (ct.IsCancellationRequested)
                    {
                        _logger.LogWarning("[TocGeneration] Cancellation requested, stopping TOC generation");
                        break;
                    }

                    var batch = mdFiles.Skip(i).Take(batchSize).ToList();
                    _logger.LogDebug($"[TocGeneration] Processing batch {i / batchSize + 1}: {batch.Count} files");

                    foreach (var file in batch)
                    {
                        try
                        {
                            var fileName = Path.GetFileName(file);
                            var title = GetFileTitle(file);
                            var relativePath = GetRelativePath(directoryPath, file);

                            // Get AI description
                            var fileContent = await GetFileContentForAI(file);
                            var filePrompt = string.Format(prompt, title, fileContent);
                            
                            _logger.LogInformation($"[TocGeneration] Processing file: {fileName}, cache enabled: {enableCache}, forceNoCache: {forceNoCache}");
                            var description = await GetCachedOrGenerateDescription(file, filePrompt, enableCache);

                            // Add to table
                            tableContent.AppendLine($"| {title} | {description} | [{fileName}]({relativePath}) |");

                            // Add card
                            cardsContent.Append(GenerateFileCard(file, relativePath, description));

                            processedCount++;

                            // Update progress
                            var progress = new TocGenerationProgress
                            {
                                Directory = directoryPath,
                                Processed = processedCount,
                                Total = mdFiles.Count,
                                Status = $"Processing: {fileName}",
                                PercentComplete = (processedCount * 100) / mdFiles.Count
                            };

                            NotifyProgress(progress);
                        }
                        catch (Exception fileEx)
                        {
                            _logger.LogError($"[TocGeneration] Error processing file {file}: {fileEx.Message}");
                        }
                    }

                    // Small delay between batches to avoid overwhelming the AI
                    if (i + batchSize < mdFiles.Count)
                    {
                        await Task.Delay(500, ct);
                    }
                }

                // Write complete TOC file
                await FinalizeTocFile(tocFilePath, tableContent.ToString(), cardsContent.ToString());

                // Send completion notification
                NotifyCompletion(directoryPath);

                _logger.LogInformation($"[TocGeneration] TOC generation completed successfully for: {directoryPath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocGeneration] Error generating TOC: {ex.Message}", ex);
                
                // Send completion notification even on error to close the progress dialog
                NotifyCompletion(directoryPath);
                
                await GenerateErrorToc(tocFilePath, ex.Message);
                return false;
            }
        }

        public async Task<string> GenerateQuickTocAsync(string directoryPath, string tocFilePath)
        {
            try
            {
                _logger.LogInformation($"[TocGeneration] Generating quick TOC for: {directoryPath}");

                var mdFiles = Directory.GetFiles(directoryPath, "*.md", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.EndsWith(".md.directory"))
                    .OrderBy(f => f)
                    .ToList();

                var content = new StringBuilder();
                var directoryName = Path.GetFileName(directoryPath);
                var yamlHeader = _yamlDefaultGenerator.GenerateDefaultYaml(directoryPath);
                
                content.AppendLine(yamlHeader);
                content.AppendLine($"# {directoryName}");
                content.AppendLine();
                content.AppendLine($"> 📊 **Stato**: Lista semplice (senza AI)");
                content.AppendLine($"> 📁 **File trovati**: {mdFiles.Count}");
                content.AppendLine($"> 📅 **Generato**: {DateTime.Now:yyyy-MM-dd HH:mm}");
                content.AppendLine();
                content.AppendLine("## 📑 Elenco Documenti");
                content.AppendLine();

                foreach (var file in mdFiles)
                {
                    var fileName = Path.GetFileName(file);
                    var relativePath = GetRelativePath(directoryPath, file);
                    var fileInfo = new FileInfo(file);
                    
                    content.AppendLine($"- [{fileName}]({relativePath}) - {FormatFileSize(fileInfo.Length)} - {fileInfo.LastWriteTime:yyyy-MM-dd}");
                }

                await File.WriteAllTextAsync(tocFilePath, content.ToString(), Encoding.UTF8);
                
                _logger.LogInformation($"[TocGeneration] Quick TOC generated successfully");
                return content.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocGeneration] Error generating quick TOC: {ex.Message}", ex);
                throw;
            }
        }

        private async Task<string> GetFileDescriptionAsync(string filePath, string prompt, bool useCache)
        {
            try
            {
                string fileHash = null;
                
                if (useCache)
                {
                    fileHash = ComputeFileHash(filePath);
                    var cached = GetCachedDescription(filePath, fileHash);
                    if (!string.IsNullOrEmpty(cached))
                    {
                        _logger.LogDebug($"[TocGeneration] Using cached description for: {Path.GetFileName(filePath)}");
                        return cached;
                    }
                }

                // Read file content
                var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                
                // Use a reasonable limit for AI context (10000 chars should be enough for most docs)
                const int maxContentLength = 10000;
                if (content.Length > maxContentLength)
                {
                    // Take first part of document for context
                    content = content.Substring(0, maxContentLength) + "\n\n[... documento troncato per lunghezza ...]";
                }

                // Prepare prompt
                var fileName = Path.GetFileName(filePath);
                var aiPrompt = prompt.Replace("{filename}", fileName).Replace("{content}", content);
                
                // If prompt doesn't have placeholders, append content
                if (!prompt.Contains("{content}"))
                {
                    aiPrompt = $"{prompt}\n\nFile: {fileName}\nContenuto:\n{content}";
                }

                _logger.LogDebug($"[TocGeneration] Sending to AI - prompt length: {aiPrompt.Length} chars");
                
                // Get AI description
                var description = await GetAiDescriptionAsync(aiPrompt);
                
                _logger.LogDebug($"[TocGeneration] Received from AI - description length: {description?.Length ?? 0} chars");
                
                // No truncation - return full description from AI

                // Save to cache if enabled
                if (useCache && !string.IsNullOrEmpty(fileHash))
                {
                    SaveToCache(filePath, fileHash, description);
                }

                return description;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocGeneration] Error getting description for {filePath}: {ex.Message}");
                return $"*Errore nell'analisi del file: {ex.Message}*";
            }
        }

        private string GetCachedDescription(string filePath, string fileHash)
        {
            try
            {
                var cacheDal = _userSettingsDB.GetDal<TocDescriptionCache>();
                var cached = cacheDal.GetList()
                    .Where(c => c.FilePath == filePath && c.FileHash == fileHash)
                    .OrderByDescending(c => c.GeneratedAt)
                    .FirstOrDefault();
                
                if (cached != null)
                {
                    // Check if cache is still valid (within cache days)
                    var cacheValidDays = GetCacheDays();
                    if ((DateTime.Now - cached.GeneratedAt).TotalDays <= cacheValidDays)
                    {
                        return cached.Description;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocGeneration] Error reading cache: {ex.Message}");
            }
            
            return null;
        }

        private void SaveToCache(string filePath, string fileHash, string description)
        {
            try
            {
                var cacheDal = _userSettingsDB.GetDal<TocDescriptionCache>();
                
                // Check if entry already exists
                var existing = cacheDal.GetList()
                    .Where(c => c.FilePath == filePath)
                    .FirstOrDefault();
                
                _userSettingsDB.BeginTransaction(System.Data.IsolationLevel.Unspecified);
                
                if (existing != null)
                {
                    // Update existing entry
                    existing.FileHash = fileHash;
                    existing.Description = description;
                    existing.GeneratedAt = DateTime.Now;
                    existing.ModelUsed = GetCurrentAiModelName();
                    cacheDal.Save(existing);
                }
                else
                {
                    // Create new entry
                    var newCache = new TocDescriptionCache
                    {
                        FilePath = filePath,
                        FileHash = fileHash,
                        Description = description,
                        GeneratedAt = DateTime.Now,
                        ModelUsed = GetCurrentAiModelName()
                    };
                    cacheDal.Save(newCache);
                }
                
                _userSettingsDB.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocGeneration] Error saving to cache: {ex.Message}");
                _userSettingsDB.Rollback();
            }
        }

        private string ComputeFileHash(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private async Task InitializeTocFile(string tocFilePath, string directoryPath, int fileCount)
        {
            var directoryName = Path.GetFileName(directoryPath);
            var yamlHeader = _yamlDefaultGenerator.GenerateDefaultYaml(directoryPath);
            
            var content = new StringBuilder();
            content.AppendLine(yamlHeader);
            content.AppendLine($"# {directoryName}");
            content.AppendLine();
            content.AppendLine($"> 📊 **Stato**: ✅ Generato con AI");
            content.AppendLine($"> 📁 **File analizzati**: 0/{fileCount}");
            content.AppendLine($"> 📅 **Ultimo aggiornamento**: {DateTime.Now:yyyy-MM-dd HH:mm}");
            content.AppendLine($"> 🤖 **Modello AI**: {GetCurrentAiModelName()}");
            content.AppendLine();
            content.AppendLine("## 📑 Indice Rapido");
            content.AppendLine();
            content.AppendLine("*Generazione in corso...*");

            await File.WriteAllTextAsync(tocFilePath, content.ToString(), Encoding.UTF8);
        }

        private async Task FinalizeTocFile(string tocFilePath, string tableContent, string cardsContent)
        {
            var directoryPath = Path.GetDirectoryName(tocFilePath);
            var directoryName = Path.GetFileName(directoryPath);
            var yamlHeader = _yamlDefaultGenerator.GenerateDefaultYaml(directoryPath);
            
            var fileCount = tableContent.Split('\n').Length - 3; // Subtract header lines
            
            var content = new StringBuilder();
            content.AppendLine(yamlHeader);
            content.AppendLine($"# {directoryName}");
            content.AppendLine();
            content.AppendLine($"> 📊 **Stato**: ✅ Generato con AI");
            content.AppendLine($"> 📁 **File analizzati**: {fileCount}/{fileCount}");
            content.AppendLine($"> 📅 **Ultimo aggiornamento**: {DateTime.Now:yyyy-MM-dd HH:mm}");
            content.AppendLine($"> 🤖 **Modello AI**: {GetCurrentAiModelName()}");
            content.AppendLine();
            content.AppendLine("## 📑 Indice Rapido");
            content.AppendLine();
            content.Append(tableContent);
            content.Append(cardsContent);

            await File.WriteAllTextAsync(tocFilePath, content.ToString(), Encoding.UTF8);
        }

        private string GenerateFileCard(string filePath, string relativePath, string description)
        {
            var fileInfo = new FileInfo(filePath);
            var fileName = Path.GetFileName(filePath);
            
            var card = new StringBuilder();
            card.AppendLine($"### 📄 {fileName}");
            card.AppendLine($"**Percorso**: `{relativePath}`  ");
            card.AppendLine($"**Ultima modifica**: {fileInfo.LastWriteTime:yyyy-MM-dd}  ");
            card.AppendLine($"**Dimensione**: {FormatFileSize(fileInfo.Length)}");
            card.AppendLine();
            card.AppendLine($"> {description}");
            card.AppendLine();
            card.AppendLine("---");
            card.AppendLine();

            return card.ToString();
        }

        private async Task GenerateSimpleTocWithWarning(string directoryPath, string tocFilePath)
        {
            var content = await GenerateQuickTocAsync(directoryPath, tocFilePath);
            
            // Add warning at the beginning
            var lines = content.Split('\n').ToList();
            var yamlEndIndex = lines.FindIndex(l => l.StartsWith("---") && lines.IndexOf(l) > 0) + 1;
            
            lines.Insert(yamlEndIndex + 2, "");
            lines.Insert(yamlEndIndex + 3, "> ⚠️ **ATTENZIONE**: AI non disponibile - Generata lista semplice senza descrizioni");
            lines.Insert(yamlEndIndex + 4, "> Per abilitare le descrizioni AI, caricare un modello dalla gestione modelli");
            
            await File.WriteAllTextAsync(tocFilePath, string.Join('\n', lines), Encoding.UTF8);
        }

        private async Task GenerateErrorToc(string tocFilePath, string errorMessage)
        {
            var directoryPath = Path.GetDirectoryName(tocFilePath);
            var directoryName = Path.GetFileName(directoryPath);
            var yamlHeader = _yamlDefaultGenerator.GenerateDefaultYaml(directoryPath);
            
            var content = new StringBuilder();
            content.AppendLine(yamlHeader);
            content.AppendLine($"# {directoryName}");
            content.AppendLine();
            content.AppendLine($"> ❌ **ERRORE**: Generazione TOC fallita");
            content.AppendLine($"> 📅 **Timestamp**: {DateTime.Now:yyyy-MM-dd HH:mm}");
            content.AppendLine($"> 🔍 **Dettaglio errore**: {errorMessage}");
            content.AppendLine();
            content.AppendLine("## Risoluzione");
            content.AppendLine();
            content.AppendLine("Provare a:");
            content.AppendLine("1. Verificare che il modello AI sia caricato");
            content.AppendLine("2. Controllare i permessi sulla directory");
            content.AppendLine("3. Riprovare con 'Refresh Toc' dal menu contestuale");
            content.AppendLine("4. Utilizzare 'Toc Rapida' per una lista semplice senza AI");

            await File.WriteAllTextAsync(tocFilePath, content.ToString(), Encoding.UTF8);
        }

        private void NotifyProgress(string directoryPath, int processed, int total, string status)
        {
            ProgressChanged?.Invoke(this, new TocGenerationProgress
            {
                Directory = directoryPath,
                Processed = processed,
                Total = total,
                Status = status,
                PercentComplete = (processed * 100) / total
            });
        }

        private void NotifyCompletion(string directoryPath)
        {
            GenerationCompleted?.Invoke(this, directoryPath);
        }

        private string GetTocPrompt()
        {
            try
            {
                var settingDal = _userSettingsDB.GetDal<Setting>();
                var promptSetting = settingDal.GetList()
                    .Where(s => s.Name == "TOC_Generation_Prompt")
                    .FirstOrDefault();
                
                if (promptSetting != null && !string.IsNullOrEmpty(promptSetting.ValueString))
                {
                    return promptSetting.ValueString;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocGeneration] Error reading prompt from database: {ex.Message}");
            }
            
            // Fallback to configuration
            return _configuration["TocGeneration:DefaultPrompt"] ?? 
                "Analizza questo documento markdown e genera una descrizione dettagliata (100-200 parole). Focus su: scopo principale, contenuti chiave, target audience, informazioni rilevanti.";
        }

        private int GetBatchSize()
        {
            try
            {
                var settingDal = _userSettingsDB.GetDal<Setting>();
                var batchSizeSetting = settingDal.GetList()
                    .Where(s => s.Name == "TOC_Generation_BatchSize")
                    .FirstOrDefault();
                
                if (batchSizeSetting != null && batchSizeSetting.ValueInt.HasValue && batchSizeSetting.ValueInt.Value > 0)
                {
                    return batchSizeSetting.ValueInt.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocGeneration] Error reading batch size from database: {ex.Message}");
            }
            
            // Fallback to configuration or default
            var configValue = _configuration.GetValue<int>("TocGeneration:BatchSize");
            return configValue > 0 ? configValue : DEFAULT_BATCH_SIZE;
        }

        private bool IsCacheEnabled()
        {
            try
            {
                var settingDal = _userSettingsDB.GetDal<Setting>();
                var cacheSetting = settingDal.GetList()
                    .Where(s => s.Name == "TOC_Generation_EnableCache")
                    .FirstOrDefault();
                
                if (cacheSetting != null && cacheSetting.ValueInt.HasValue)
                {
                    // ValueInt is used as boolean (0 = false, 1 = true)
                    return cacheSetting.ValueInt.Value == 1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocGeneration] Error reading cache enabled from database: {ex.Message}");
            }
            
            // Fallback to configuration
            return _configuration.GetValue<bool>("TocGeneration:EnableAI", true);
        }
        
        private int GetCacheDays()
        {
            try
            {
                var settingDal = _userSettingsDB.GetDal<Setting>();
                var cacheDaysSetting = settingDal.GetList()
                    .Where(s => s.Name == "TOC_Generation_CacheDays")
                    .FirstOrDefault();
                
                if (cacheDaysSetting != null && cacheDaysSetting.ValueInt.HasValue && cacheDaysSetting.ValueInt.Value > 0)
                {
                    return cacheDaysSetting.ValueInt.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocGeneration] Error reading cache days from database: {ex.Message}");
            }
            
            // Fallback to configuration or default
            var configValue = _configuration.GetValue<int>("TocGeneration:CacheDays");
            return configValue > 0 ? configValue : DEFAULT_CACHE_DAYS;
        }

        private string GetRelativePath(string basePath, string fullPath)
        {
            var fileName = Path.GetFileName(fullPath);
            return $"./{fileName}";
        }

        private string GetFileTitle(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                // Look for the first H1 heading
                foreach (var line in lines.Take(20)) // Check first 20 lines
                {
                    if (line.TrimStart().StartsWith("# "))
                    {
                        return line.TrimStart().Substring(2).Trim();
                    }
                }
                // Fallback to filename without extension
                return Path.GetFileNameWithoutExtension(filePath);
            }
            catch
            {
                return Path.GetFileNameWithoutExtension(filePath);
            }
        }

        private async Task<string> GetFileContentForAI(string filePath)
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                
                // Use a reasonable limit for AI context (10000 chars should be enough for most docs)
                const int maxContentLength = 10000;
                if (content.Length > maxContentLength)
                {
                    // Take first part of document for context
                    content = content.Substring(0, maxContentLength) + "\n\n[... documento troncato per lunghezza ...]";
                }
                
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocGeneration] Error reading file {filePath}: {ex.Message}");
                return string.Empty;
            }
        }

        private async Task<string> GetCachedOrGenerateDescription(string filePath, string prompt, bool useCache)
        {
            try
            {
                string fileHash = null;
                var fileName = Path.GetFileName(filePath);
                
                if (useCache)
                {
                    fileHash = ComputeFileHash(filePath);
                    var cached = GetCachedDescription(filePath, fileHash);
                    if (!string.IsNullOrEmpty(cached))
                    {
                        _logger.LogInformation($"[TocGeneration] Using cached description for: {fileName}");
                        return cached;
                    }
                    else
                    {
                        _logger.LogInformation($"[TocGeneration] No valid cache found for: {fileName}, will generate new description");
                    }
                }
                else
                {
                    _logger.LogInformation($"[TocGeneration] Cache disabled, generating fresh description for: {fileName}");
                }

                _logger.LogInformation($"[TocGeneration] Calling AI for: {fileName}, prompt length: {prompt.Length} chars");
                
                // Get AI description
                var description = await GetAiDescriptionAsync(prompt);
                
                _logger.LogInformation($"[TocGeneration] Received AI description for {fileName} - length: {description?.Length ?? 0} chars");

                // Save to cache if enabled and description is not empty
                if (useCache && !string.IsNullOrEmpty(fileHash) && !string.IsNullOrEmpty(description))
                {
                    _logger.LogInformation($"[TocGeneration] Saving description to cache for: {fileName}");
                    SaveToCache(filePath, fileHash, description);
                }
                else if (string.IsNullOrEmpty(description))
                {
                    _logger.LogWarning($"[TocGeneration] Empty description received for {fileName}, not caching");
                }

                return description;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocGeneration] Error getting description for {filePath}: {ex.Message}");
                return $"*Errore nell'analisi del file: {ex.Message}*";
            }
        }

        private void NotifyProgress(TocGenerationProgress progress)
        {
            ProgressChanged?.Invoke(this, progress);
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
        
        private async Task<bool> IsAiAvailableAsync()
        {
            // Check which AI system is currently active
            var settings = GetAiModeSettings();
            
            if (settings.useGemini)
            {
                _useGemini = true;
                _geminiModel = settings.geminiModel;
                var isConfigured = _geminiService.IsConfigured();
                _logger.LogInformation($"[TocGeneration] Using Gemini API: {isConfigured}, Model: {_geminiModel}");
                return isConfigured;
            }
            else
            {
                _useGemini = false;
                var isLoaded = _aiChatService.IsModelLoaded();
                _logger.LogInformation($"[TocGeneration] Using local AI model: {isLoaded}");
                return isLoaded;
            }
        }
        
        private async Task<string> GetAiDescriptionAsync(string prompt)
        {
            if (_useGemini)
            {
                _logger.LogInformation($"[TocGeneration] Getting description from Gemini model: {_geminiModel}");
                try
                {
                    var result = await _geminiService.ChatAsync(prompt, _geminiModel);
                    _logger.LogInformation($"[TocGeneration] Gemini response received, length: {result?.Length ?? 0}");
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[TocGeneration] Error calling Gemini API: {ex.Message}", ex);
                    throw;
                }
            }
            else
            {
                _logger.LogInformation("[TocGeneration] Getting description from local AI model");
                try
                {
                    var result = await _aiChatService.ChatAsync(prompt);
                    _logger.LogInformation($"[TocGeneration] Local AI response received, length: {result?.Length ?? 0}");
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[TocGeneration] Error calling local AI: {ex.Message}", ex);
                    throw;
                }
            }
        }
        
        private string GetCurrentAiModelName()
        {
            if (_useGemini)
            {
                return $"Gemini: {_geminiModel}";
            }
            else
            {
                return _aiChatService.GetCurrentModelName();
            }
        }
        
        private (bool useGemini, string geminiModel) GetAiModeSettings()
        {
            // Check for settings in the database
            try
            {
                _logger.LogInformation("[TocGeneration] Reading AI mode settings from database");
                var settingsDal = _userSettingsDB.GetDal<Setting>();
                var useGeminiSetting = settingsDal.GetList()
                    .FirstOrDefault(s => s.Name == "TocGeneration_UseGemini");
                var geminiModelSetting = settingsDal.GetList()
                    .FirstOrDefault(s => s.Name == "TocGeneration_GeminiModel");
                
                _logger.LogInformation($"[TocGeneration] Found settings - UseGemini: {useGeminiSetting?.ValueInt}, Model: {geminiModelSetting?.ValueString}");
                
                var useGemini = (useGeminiSetting?.ValueInt ?? 0) == 1;
                var geminiModel = geminiModelSetting?.ValueString ?? "gemini-1.5-flash";
                
                _logger.LogInformation($"[TocGeneration] AI Mode - UseGemini: {useGemini}, Model: {geminiModel}");
                return (useGemini, geminiModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TocGeneration] Error reading AI mode settings, defaulting to local");
                return (false, "gemini-1.5-flash");
            }
        }
        
        public void SetAiMode(bool useGemini, string geminiModel = null)
        {
            _logger.LogInformation($"[TocGeneration] SetAiMode called - UseGemini: {useGemini}, Model: {geminiModel}");
            
            _useGemini = useGemini;
            if (!string.IsNullOrEmpty(geminiModel))
            {
                _geminiModel = geminiModel;
            }
            
            // Save to database for persistence
            try
            {
                _logger.LogInformation("[TocGeneration] Saving AI mode to database");
                var settingsDal = _userSettingsDB.GetDal<Setting>();
                _userSettingsDB.BeginTransaction();
                
                // Save UseGemini setting
                var useGeminiSetting = settingsDal.GetList()
                    .FirstOrDefault(s => s.Name == "TocGeneration_UseGemini");
                if (useGeminiSetting == null)
                {
                    useGeminiSetting = new Setting
                    {
                        Name = "TocGeneration_UseGemini",
                        Description = "Use Gemini API for TOC generation"
                    };
                }
                useGeminiSetting.ValueInt = useGemini ? 1 : 0;
                settingsDal.Save(useGeminiSetting);
                
                // Save GeminiModel setting
                if (!string.IsNullOrEmpty(geminiModel))
                {
                    var geminiModelSetting = settingsDal.GetList()
                        .FirstOrDefault(s => s.Name == "TocGeneration_GeminiModel");
                    if (geminiModelSetting == null)
                    {
                        geminiModelSetting = new Setting
                        {
                            Name = "TocGeneration_GeminiModel",
                            Description = "Gemini model for TOC generation"
                        };
                    }
                    geminiModelSetting.ValueString = geminiModel;
                    settingsDal.Save(geminiModelSetting);
                }
                
                _userSettingsDB.Commit();
                _logger.LogInformation($"[TocGeneration] AI mode set to: {(useGemini ? $"Gemini ({geminiModel})" : "Local")}");
            }
            catch (Exception ex)
            {
                _userSettingsDB.Rollback();
                _logger.LogError(ex, "[TocGeneration] Error saving AI mode settings");
            }
        }
    }
}