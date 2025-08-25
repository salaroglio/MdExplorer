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
        private readonly IConfiguration _configuration;
        private readonly IYamlDefaultGenerator _yamlDefaultGenerator;
        private const int DEFAULT_BATCH_SIZE = 10;
        private const int DEFAULT_CACHE_DAYS = 30;

        public event EventHandler<TocGenerationProgress> ProgressChanged;
        public event EventHandler<string> GenerationCompleted;

        public TocGenerationService(
            ILogger<TocGenerationService> logger,
            IUserSettingsDB userSettingsDB,
            IAiChatService aiChatService,
            IConfiguration configuration,
            IYamlDefaultGenerator yamlDefaultGenerator)
        {
            _logger = logger;
            _userSettingsDB = userSettingsDB;
            _aiChatService = aiChatService;
            _configuration = configuration;
            _yamlDefaultGenerator = yamlDefaultGenerator;
        }

        public async Task<bool> GenerateTocWithAIAsync(string directoryPath, string tocFilePath, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation($"[TocGeneration] Starting AI TOC generation for: {directoryPath}");

                // Check if AI is available
                if (!_aiChatService.IsModelLoaded())
                {
                    _logger.LogWarning("[TocGeneration] AI model not loaded, falling back to simple TOC");
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
                        
                        // Get description (from cache or AI)
                        var description = await GetFileDescriptionAsync(filePath, prompt, enableCache);
                        
                        // Add to table
                        tableContent.AppendLine($"| [{fileName}]({relativePath}) | {description} |");
                        
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
            return await GenerateTocWithAIAsync(directoryPath, tocFilePath, ct);
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
                
                // Truncate if too long
                const int maxContentLength = 2000;
                if (content.Length > maxContentLength)
                {
                    content = content.Substring(0, maxContentLength) + "...";
                }

                // Prepare prompt
                var fileName = Path.GetFileName(filePath);
                var aiPrompt = prompt.Replace("{filename}", fileName).Replace("{content}", content);
                
                // If prompt doesn't have placeholders, append content
                if (!prompt.Contains("{content}"))
                {
                    aiPrompt = $"{prompt}\n\nFile: {fileName}\nContenuto:\n{content}";
                }

                // Get AI description
                var description = await _aiChatService.ChatAsync(aiPrompt);
                
                // Ensure description is not too long
                if (description.Length > 200)
                {
                    description = description.Substring(0, 197) + "...";
                }

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
                    existing.ModelUsed = _aiChatService.GetCurrentModelName();
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
                        ModelUsed = _aiChatService.GetCurrentModelName()
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
            content.AppendLine($"> 🤖 **Modello AI**: {_aiChatService.GetCurrentModelName()}");
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
            content.AppendLine($"> 🤖 **Modello AI**: {_aiChatService.GetCurrentModelName()}");
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
                "Analizza questo documento markdown e genera una descrizione concisa di massimo 50 parole. Focus su: scopo principale, contenuti chiave, target audience.";
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
    }
}