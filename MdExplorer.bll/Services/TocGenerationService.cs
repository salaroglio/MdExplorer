using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Yaml.Interfaces;
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
        Task<bool> GenerateTocAsync(string directoryPath, string tocFilePath, CancellationToken ct = default);
        event EventHandler<TocGenerationProgress> ProgressChanged;
        event EventHandler<string> GenerationCompleted;
    }

    /// <summary>
    /// Builds the per-folder TOC file (<c>&lt;dirname&gt;.md.directory</c>) deterministically.
    /// No AI, no cache: every call rebuilds the file from scratch using
    /// <list type="bullet">
    /// <item><description>the file system (list of *.md);</description></item>
    /// <item><description>each document's <c>## TL;DR</c> block (parsed in-process);</description></item>
    /// <item><description>each document's MD5 hash.</description></item>
    /// </list>
    /// After writing the TOC, fires <c>IKgSyncOrchestrator.SyncFolderAsync</c> so any
    /// <c>.mde-doc/*.kg.cypher</c> scripts in the folder get pushed to Neo4j.
    /// </summary>
    public class TocGenerationService : ITocGenerationService
    {
        private readonly ILogger<TocGenerationService> _logger;
        private readonly IYamlDefaultGenerator _yamlDefaultGenerator;
        private readonly KnowledgeGraph.IKgSyncOrchestrator _kgSyncOrchestrator;

        public event EventHandler<TocGenerationProgress> ProgressChanged;
        public event EventHandler<string> GenerationCompleted;

        public TocGenerationService(
            ILogger<TocGenerationService> logger,
            IYamlDefaultGenerator yamlDefaultGenerator,
            KnowledgeGraph.IKgSyncOrchestrator kgSyncOrchestrator)
        {
            _logger = logger;
            _yamlDefaultGenerator = yamlDefaultGenerator;
            _kgSyncOrchestrator = kgSyncOrchestrator;
        }

        public async Task<bool> GenerateTocAsync(string directoryPath, string tocFilePath, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation($"[TocGeneration] Generating TOC for: {directoryPath}");

                var mdFiles = Directory.GetFiles(directoryPath, "*.md", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.EndsWith(".md.directory", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                _logger.LogInformation($"[TocGeneration] Found {mdFiles.Count} markdown file(s) to process");

                var tableContent = new StringBuilder();
                tableContent.AppendLine("| Titolo | TL;DR | Hash | Link |");
                tableContent.AppendLine("|--------|-------|------|------|");

                int processed = 0;
                foreach (var file in mdFiles)
                {
                    if (ct.IsCancellationRequested)
                    {
                        _logger.LogWarning("[TocGeneration] Cancellation requested");
                        break;
                    }

                    try
                    {
                        var fileName = Path.GetFileName(file);
                        var title = GetFileTitle(file);
                        var relativePath = GetRelativePath(directoryPath, file);
                        var urlPath = EncodePathForMarkdownUrl(relativePath);
                        var fileHash = ComputeFileHash(file);
                        var hashShort = fileHash.Substring(0, Math.Min(8, fileHash.Length));
                        var tldr = ExtractTldrFromFile(file);

                        var summaryCell = string.IsNullOrEmpty(tldr)
                            ? "*(TL;DR mancante — aggiungi `## TL;DR` al documento secondo la skill mde-doc)*"
                            : tldr;

                        tableContent.AppendLine($"| {title} | {summaryCell} | `{hashShort}` | [{fileName}]({urlPath}) |");

                        processed++;
                        NotifyProgress(directoryPath, processed, mdFiles.Count, $"Processing: {fileName}");
                    }
                    catch (Exception fileEx)
                    {
                        _logger.LogError($"[TocGeneration] Error processing file {file}: {fileEx.Message}");
                    }
                }

                await WriteTocFileAsync(tocFilePath, directoryPath, mdFiles.Count, tableContent.ToString(), ct);

                // Auto-sync hook into Neo4j (best-effort; orchestrator swallows errors and
                // honors ProjectNeo4jSettings.Enabled + SyncOnTocGeneration).
                try
                {
                    var outcome = await _kgSyncOrchestrator.SyncFolderAsync(directoryPath, KnowledgeGraph.KgSyncTrigger.TocGeneration, ct);
                    if (outcome.Triggered)
                    {
                        _logger.LogInformation("[TocGeneration] KG auto-sync: {Ok} ok, {Sk} skipped, {Fail} failed",
                            outcome.SucceededFiles, outcome.SkippedFiles, outcome.FailedFiles);
                    }
                }
                catch (Exception kgEx)
                {
                    _logger.LogWarning(kgEx, "[TocGeneration] KG auto-sync threw — TOC generation considered successful");
                }

                NotifyCompletion(directoryPath);
                _logger.LogInformation($"[TocGeneration] TOC generation completed for: {directoryPath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[TocGeneration] Error generating TOC: {ex.Message}");
                NotifyCompletion(directoryPath);
                return false;
            }
        }

        private async Task WriteTocFileAsync(string tocFilePath, string directoryPath, int fileCount, string tableContent, CancellationToken ct)
        {
            var directoryName = Path.GetFileName(directoryPath);
            var yamlHeader = _yamlDefaultGenerator.GenerateDefaultYaml(directoryPath);

            var content = new StringBuilder();
            content.AppendLine(yamlHeader);
            content.AppendLine($"# {directoryName}");
            content.AppendLine();
            content.AppendLine($"> 📁 **File analizzati**: {fileCount}");
            content.AppendLine($"> 📅 **Ultimo aggiornamento**: {DateTime.Now:yyyy-MM-dd HH:mm}");
            content.AppendLine();
            content.AppendLine("## 📑 Indice Rapido");
            content.AppendLine();
            content.Append(tableContent);

            await File.WriteAllTextAsync(tocFilePath, content.ToString(), Encoding.UTF8, ct);
        }

        // ============================ TL;DR extraction ============================

        /// <summary>
        /// Deterministically extracts the TL;DR block from a markdown document produced under the
        /// mde-doc skill. Looks for a heading matching <c>## TL;DR</c> (case-insensitive, also
        /// accepts <c>TLDR</c> and trailing semicolons) and gathers prose + bullet lines until the
        /// next <c>##</c> heading or EOF. Returns a single-line, table-cell-safe string, or null
        /// if no TL;DR section is found.
        /// </summary>
        private static string ExtractTldrFromFile(string filePath)
        {
            try
            {
                var content = File.ReadAllText(filePath, Encoding.UTF8);
                var lines = content.Split('\n');

                int startIdx = -1;
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].TrimEnd('\r').Trim();
                    if (!line.StartsWith("##", StringComparison.Ordinal)) continue;
                    if (line.StartsWith("###", StringComparison.Ordinal)) continue;

                    var heading = line.TrimStart('#').Trim().Replace(";", string.Empty).Trim();
                    if (heading.Equals("TLDR", StringComparison.OrdinalIgnoreCase))
                    {
                        startIdx = i + 1;
                        break;
                    }
                }

                if (startIdx < 0) return null;

                var prose = new List<string>();
                var bullets = new List<string>();
                bool sawBullet = false;

                for (int i = startIdx; i < lines.Length; i++)
                {
                    var trimmed = lines[i].TrimEnd('\r').Trim();

                    if (trimmed.StartsWith("##", StringComparison.Ordinal) &&
                        !trimmed.StartsWith("###", StringComparison.Ordinal))
                    {
                        break;
                    }

                    if (string.IsNullOrEmpty(trimmed))
                    {
                        if (sawBullet && bullets.Count > 0) break;
                        continue;
                    }

                    if (trimmed.StartsWith("-") || trimmed.StartsWith("*"))
                    {
                        bullets.Add(trimmed.TrimStart('-', '*').Trim());
                        sawBullet = true;
                    }
                    else if (!sawBullet)
                    {
                        prose.Add(trimmed);
                    }
                    else
                    {
                        break;
                    }
                }

                if (prose.Count == 0 && bullets.Count == 0) return null;

                var sb = new StringBuilder();
                if (prose.Count > 0) sb.Append(string.Join(" ", prose));
                if (bullets.Count > 0)
                {
                    if (sb.Length > 0) sb.Append(" — ");
                    sb.Append(string.Join(" · ", bullets));
                }

                var compact = sb.ToString().Replace("|", "\\|");

                // Preview length: text beyond this is moved into a collapsible
                // <details> block so the table cell stays compact without losing content.
                const int maxLen = 280;
                if (compact.Length > maxLen)
                {
                    int cut = compact.LastIndexOf(' ', maxLen - 1);
                    if (cut < maxLen / 2) cut = maxLen - 1;   // no decent word break → hard cut
                    var head = compact.Substring(0, cut).TrimEnd();
                    var tail = compact.Substring(cut).Trim();
                    // Inline <details> so the collapsed toggle stays on the same line
                    // as the preview text (block display would push it to a new line).
                    // The summary is styled blue + pointer cursor so it reads as clickable.
                    compact = head
                        + "<details style=\"display:inline\">"
                        + "<summary style=\"display:inline;color:#0d6efd;cursor:pointer;font-weight:bold\" title=\"Mostra tutto\">…</summary>"
                        + tail + "</details>";
                }

                return compact;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // ============================ Hash ============================

        private static string ComputeFileHash(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        // ============================ File utilities ============================

        private static string GetFileTitle(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);
                foreach (var raw in lines)
                {
                    var line = raw.Trim();
                    if (line.StartsWith("# ", StringComparison.Ordinal))
                    {
                        return line.Substring(2).Trim();
                    }
                }
                return Path.GetFileNameWithoutExtension(filePath);
            }
            catch
            {
                return Path.GetFileNameWithoutExtension(filePath);
            }
        }

        private static string GetRelativePath(string basePath, string fullPath)
        {
            var rel = Path.GetRelativePath(basePath, fullPath);
            return rel.Replace('\\', '/');
        }

        /// <summary>
        /// Percent-encodes a POSIX-style relative path for use as the URL part of a markdown
        /// link ([text](url)). Each segment is encoded individually so that '/' is preserved
        /// as separator while spaces, parentheses, brackets, etc. become %20/%28/...
        /// </summary>
        private static string EncodePathForMarkdownUrl(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            return string.Join('/', path.Split('/').Select(Uri.EscapeDataString));
        }

        // ============================ Notification helpers ============================

        private void NotifyProgress(string directoryPath, int processed, int total, string status)
        {
            ProgressChanged?.Invoke(this, new TocGenerationProgress
            {
                Directory = directoryPath,
                Processed = processed,
                Total = total,
                Status = status,
                PercentComplete = total > 0 ? (processed * 100) / total : 100
            });
        }

        private void NotifyCompletion(string directoryPath)
        {
            GenerationCompleted?.Invoke(this, directoryPath);
        }

    }
}
