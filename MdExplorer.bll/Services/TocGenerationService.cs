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
    /// <item><description>each document's MD5 hash;</description></item>
    /// <item><description>the sibling <c>.mde-doc/*.kg.md</c> payloads aggregated into the
    ///   "Grafo aggregato" sections.</description></item>
    /// </list>
    /// </summary>
    public class TocGenerationService : ITocGenerationService
    {
        private readonly ILogger<TocGenerationService> _logger;
        private readonly IYamlDefaultGenerator _yamlDefaultGenerator;

        public event EventHandler<TocGenerationProgress> ProgressChanged;
        public event EventHandler<string> GenerationCompleted;

        public TocGenerationService(
            ILogger<TocGenerationService> logger,
            IYamlDefaultGenerator yamlDefaultGenerator)
        {
            _logger = logger;
            _yamlDefaultGenerator = yamlDefaultGenerator;
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
                        var fileHash = ComputeFileHash(file);
                        var hashShort = fileHash.Substring(0, Math.Min(8, fileHash.Length));
                        var tldr = ExtractTldrFromFile(file);

                        var summaryCell = string.IsNullOrEmpty(tldr)
                            ? "*(TL;DR mancante — aggiungi `## TL;DR` al documento secondo la skill mde-doc)*"
                            : tldr;

                        tableContent.AppendLine($"| {title} | {summaryCell} | `{hashShort}` | [{fileName}]({relativePath}) |");

                        processed++;
                        NotifyProgress(directoryPath, processed, mdFiles.Count, $"Processing: {fileName}");
                    }
                    catch (Exception fileEx)
                    {
                        _logger.LogError($"[TocGeneration] Error processing file {file}: {fileEx.Message}");
                    }
                }

                var aggregateRelativePath = await WriteAggregateKgFileAsync(directoryPath, ct);
                await WriteTocFileAsync(tocFilePath, directoryPath, mdFiles.Count, tableContent.ToString(), aggregateRelativePath, ct);

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

        private async Task WriteTocFileAsync(string tocFilePath, string directoryPath, int fileCount, string tableContent, string aggregateRelativePath, CancellationToken ct)
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

            if (!string.IsNullOrEmpty(aggregateRelativePath))
            {
                content.AppendLine();
                content.AppendLine($"> 🧠 **Grafo aggregato dei concetti**: [{aggregateRelativePath}]({aggregateRelativePath})");
            }

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

                const int maxLen = 280;
                if (compact.Length > maxLen)
                {
                    compact = compact.Substring(0, maxLen - 1) + "…";
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

        // ============================ Aggregated concepts from .mde-doc/*.kg.md ============================

        // Matches PlantUML component-diagram arrows like:  [Concept A] --> [Concept B] : free-form label
        private static readonly Regex PlantUmlArrowRegex = new(
            @"^\s*\[([^\]]+)\]\s*-->\s*\[([^\]]+)\]\s*(?::\s*(.+?))?\s*$",
            RegexOptions.Compiled);

        private const string AggregateKgFileName = "_aggregate.kg.md";

        /// <summary>
        /// Scans <c>{directoryPath}/.mde-doc/*.kg.md</c> (excluding <c>_aggregate.kg.md</c> itself to
        /// avoid self-reference) and writes a deduplicated aggregate to <c>.mde-doc/_aggregate.kg.md</c>.
        /// The aggregate uses the same schema as the per-document <c>.kg.md</c> files (PlantUML graph +
        /// Neo4j Concepts/Relationships tables), so the v2 Cypher pipeline can process singles and
        /// aggregates with one parser. Returns the relative path of the aggregate file (relative to
        /// <paramref name="directoryPath"/>) if it was written, or <c>null</c> if there were no source
        /// payloads. If sources disappear, the stale aggregate file is removed.
        /// </summary>
        private async Task<string> WriteAggregateKgFileAsync(string directoryPath, CancellationToken ct)
        {
            try
            {
                var mdeDocDir = Path.Combine(directoryPath, ".mde-doc");
                var aggregatePath = Path.Combine(mdeDocDir, AggregateKgFileName);

                if (!Directory.Exists(mdeDocDir))
                {
                    _logger.LogDebug($"[TocGeneration] No .mde-doc/ folder in {directoryPath}; no aggregate to write");
                    return null;
                }

                var kgFiles = Directory.GetFiles(mdeDocDir, "*.kg.md", SearchOption.TopDirectoryOnly)
                    .Where(f => !string.Equals(Path.GetFileName(f), AggregateKgFileName, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (kgFiles.Count == 0)
                {
                    if (File.Exists(aggregatePath))
                    {
                        try { File.Delete(aggregatePath); _logger.LogInformation($"[TocGeneration] Removed stale {aggregatePath} (no source .kg.md files)"); }
                        catch (Exception ex) { _logger.LogWarning($"[TocGeneration] Could not delete stale aggregate: {ex.Message}"); }
                    }
                    return null;
                }

                var plantumlEdges = new Dictionary<(string From, string To), HashSet<string>>();
                var concepts = new Dictionary<string, SortedSet<string>>(StringComparer.Ordinal);
                var relationships = new Dictionary<(string From, string Type, string To), SortedSet<string>>();

                foreach (var kgFile in kgFiles)
                {
                    if (ct.IsCancellationRequested) return null;

                    try
                    {
                        var content = await File.ReadAllTextAsync(kgFile, ct);
                        // Strip both .md and .kg suffixes:  foo.kg.md -> foo.kg -> foo
                        var docName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(kgFile));
                        ParseKgFile(content, docName, plantumlEdges, concepts, relationships);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"[TocGeneration] Failed to parse {kgFile}: {ex.Message}");
                    }
                }

                if (plantumlEdges.Count == 0 && concepts.Count == 0 && relationships.Count == 0)
                {
                    _logger.LogInformation($"[TocGeneration] {kgFiles.Count} .kg.md file(s) yielded no parseable graph; aggregate not written");
                    return null;
                }

                var folderName = Path.GetFileName(directoryPath);
                var aggregateContent = BuildAggregateKgDocument(folderName, plantumlEdges, concepts, relationships);
                await File.WriteAllTextAsync(aggregatePath, aggregateContent, Encoding.UTF8, ct);

                _logger.LogInformation($"[TocGeneration] Wrote aggregate from {kgFiles.Count} .kg.md file(s) to {aggregatePath}");
                return $".mde-doc/{AggregateKgFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocGeneration] Aggregate generation failed for {directoryPath}: {ex.Message}");
                return null;
            }
        }

        private static void ParseKgFile(
            string content,
            string sourceDoc,
            Dictionary<(string From, string To), HashSet<string>> plantumlEdges,
            Dictionary<string, SortedSet<string>> concepts,
            Dictionary<(string From, string Type, string To), SortedSet<string>> relationships)
        {
            var lines = content.Split('\n');

            bool inPlantumlFence = false;
            string currentTable = null;       // "concepts" | "relationships" | null
            bool tableHeaderConsumed = false;

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');
                var trimmed = line.Trim();

                if (trimmed.StartsWith("```plantuml", StringComparison.OrdinalIgnoreCase))
                {
                    inPlantumlFence = true;
                    continue;
                }
                if (inPlantumlFence && trimmed.StartsWith("```"))
                {
                    inPlantumlFence = false;
                    continue;
                }
                if (inPlantumlFence)
                {
                    var m = PlantUmlArrowRegex.Match(line);
                    if (m.Success)
                    {
                        var from = m.Groups[1].Value.Trim();
                        var to = m.Groups[2].Value.Trim();
                        var label = m.Groups[3].Success ? m.Groups[3].Value.Trim() : string.Empty;

                        var key = (from, to);
                        if (!plantumlEdges.TryGetValue(key, out var labels))
                        {
                            labels = new HashSet<string>(StringComparer.Ordinal);
                            plantumlEdges[key] = labels;
                        }
                        if (!string.IsNullOrEmpty(label)) labels.Add(label);
                    }
                    continue;
                }

                if (trimmed.StartsWith("## ") || trimmed == "##")
                {
                    currentTable = null;
                    tableHeaderConsumed = false;
                    continue;
                }
                if (trimmed.StartsWith("### "))
                {
                    var headingText = trimmed.TrimStart('#').Trim();
                    if (headingText.Equals("Concepts", StringComparison.OrdinalIgnoreCase) ||
                        headingText.Equals("Concetti", StringComparison.OrdinalIgnoreCase))
                    {
                        currentTable = "concepts";
                        tableHeaderConsumed = false;
                    }
                    else if (headingText.Equals("Relationships", StringComparison.OrdinalIgnoreCase) ||
                             headingText.Equals("Relazioni", StringComparison.OrdinalIgnoreCase))
                    {
                        currentTable = "relationships";
                        tableHeaderConsumed = false;
                    }
                    else
                    {
                        currentTable = null;
                    }
                    continue;
                }

                if (currentTable != null && trimmed.StartsWith("|") && trimmed.EndsWith("|"))
                {
                    bool isSeparator = trimmed.All(c => c == '-' || c == '|' || c == ' ' || c == ':');
                    if (isSeparator) continue;

                    if (!tableHeaderConsumed)
                    {
                        tableHeaderConsumed = true;
                        continue;
                    }

                    var cells = trimmed
                        .Trim('|')
                        .Split('|')
                        .Select(c => c.Trim().Replace("\\|", "|"))
                        .ToList();

                    if (currentTable == "concepts" && cells.Count >= 1)
                    {
                        var name = cells[0];
                        if (!string.IsNullOrEmpty(name))
                        {
                            if (!concepts.TryGetValue(name, out var docs))
                            {
                                docs = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                                concepts[name] = docs;
                            }
                            docs.Add(sourceDoc);
                        }
                    }
                    else if (currentTable == "relationships" && cells.Count >= 3)
                    {
                        var from = cells[0];
                        var type = cells[1];
                        var to = cells[2];
                        if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(to))
                        {
                            var key = (from, type, to);
                            if (!relationships.TryGetValue(key, out var docs))
                            {
                                docs = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                                relationships[key] = docs;
                            }
                            docs.Add(sourceDoc);
                        }
                    }
                }
                else if (currentTable != null && !string.IsNullOrEmpty(trimmed))
                {
                    currentTable = null;
                    tableHeaderConsumed = false;
                }
            }
        }

        private static string BuildAggregateKgDocument(
            string folderName,
            Dictionary<(string From, string To), HashSet<string>> plantumlEdges,
            Dictionary<string, SortedSet<string>> concepts,
            Dictionary<(string From, string Type, string To), SortedSet<string>> relationships)
        {
            // Schema-compatible with the per-document .kg.md files produced by the mde-doc skill,
            // plus an extra "Source documents" column that records which siblings each
            // concept/relationship came from.
            var sb = new StringBuilder();

            sb.AppendLine($"# Knowledge graph — {folderName} (aggregate)");
            sb.AppendLine();
            sb.AppendLine("> *Auto-generated by MdExplorer from sibling `.kg.md` files. Do not edit manually — regenerated on every TOC refresh.*");
            sb.AppendLine();

            sb.AppendLine("## 🖼️ Graph (PlantUML)");
            sb.AppendLine();
            if (plantumlEdges.Count == 0)
            {
                sb.AppendLine("*No PlantUML graph found in source `.kg.md` files.*");
            }
            else
            {
                sb.AppendLine("```plantuml");
                sb.AppendLine("@startuml");
                // Mandatory MdExplorer styling preamble — must match the one in
                // skills/mde-doc/SKILL.md so the aggregated graph looks identical to the
                // per-document graphs the AI produces.
                sb.AppendLine("!theme plain");
                sb.AppendLine("skinparam backgroundColor #FAFAFA");
                sb.AppendLine("skinparam shadowing false");
                sb.AppendLine("skinparam roundCorner 8");
                sb.AppendLine("skinparam DefaultFontName \"Segoe UI\"");
                sb.AppendLine("skinparam ArrowColor #5B6B7F");
                sb.AppendLine("skinparam ArrowFontColor #4B5563");
                sb.AppendLine("skinparam component {");
                sb.AppendLine("  BackgroundColor #EAF1F8");
                sb.AppendLine("  BorderColor #5B6B7F");
                sb.AppendLine("  FontColor #1F2937");
                sb.AppendLine("}");
                foreach (var kvp in plantumlEdges
                    .OrderBy(k => k.Key.From, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(k => k.Key.To, StringComparer.OrdinalIgnoreCase))
                {
                    var from = kvp.Key.From;
                    var to = kvp.Key.To;
                    var label = kvp.Value.Count == 0
                        ? string.Empty
                        : string.Join(" / ", kvp.Value.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
                    sb.AppendLine(string.IsNullOrEmpty(label)
                        ? $"[{from}] --> [{to}]"
                        : $"[{from}] --> [{to}] : {label}");
                }
                sb.AppendLine("@enduml");
                sb.AppendLine("```");
            }

            sb.AppendLine();
            sb.AppendLine("## 🗃️ Graph (Neo4j)");
            sb.AppendLine();
            sb.AppendLine("### Concepts");
            sb.AppendLine();
            if (concepts.Count == 0)
            {
                sb.AppendLine("*No concepts found.*");
            }
            else
            {
                sb.AppendLine("| Name | Source documents |");
                sb.AppendLine("|------|------------------|");
                foreach (var kvp in concepts.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
                {
                    var docs = string.Join(", ", kvp.Value);
                    sb.AppendLine($"| {EscapeTableCell(kvp.Key)} | {EscapeTableCell(docs)} |");
                }
            }

            sb.AppendLine();
            sb.AppendLine("### Relationships");
            sb.AppendLine();
            if (relationships.Count == 0)
            {
                sb.AppendLine("*No relationships found.*");
            }
            else
            {
                sb.AppendLine("| From | Type | To | Source documents |");
                sb.AppendLine("|------|------|----|------------------|");
                foreach (var kvp in relationships
                    .OrderBy(k => k.Key.From, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(k => k.Key.Type, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(k => k.Key.To, StringComparer.OrdinalIgnoreCase))
                {
                    var (from, type, to) = kvp.Key;
                    var docs = string.Join(", ", kvp.Value);
                    sb.AppendLine($"| {EscapeTableCell(from)} | {EscapeTableCell(type)} | {EscapeTableCell(to)} | {EscapeTableCell(docs)} |");
                }
            }

            return sb.ToString();
        }

        private static string EscapeTableCell(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Replace("|", "\\|").Replace("\r", "").Replace("\n", " ");
        }
    }
}
