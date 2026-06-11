using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    /// The only content carried over between runs is the user "safe zone" — a marked
    /// region at the top of the file whose notes are read back and re-emitted verbatim
    /// on every regeneration (see <see cref="ExtractSafeZoneContent"/>).
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

                // Subfolders are listed in the table too. Hidden/system folders (.md,
                // .git, .github, .mde-doc, …) are skipped — they aren't part of the
                // documentation a reader navigates.
                var subFolders = Directory.GetDirectories(directoryPath)
                    .Where(d => !Path.GetFileName(d).StartsWith(".", StringComparison.Ordinal))
                    .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                _logger.LogInformation($"[TocGeneration] Found {subFolders.Count} subfolder(s) and {mdFiles.Count} markdown file(s) to process");

                // The index is emitted as a raw HTML <table> block — NOT a markdown pipe
                // table — so Markdig passes the whole thing through untouched. Cell text
                // is HTML-encoded (a total, standard function): markdown-active characters
                // like *, _, |, backtick, newline are then literal inside cells, with no
                // escape rules to maintain. The deliberate HTML (<details>, <ul>, <li>)
                // is just regular HTML nested in a regular <td>.
                var tableContent = new StringBuilder();
                tableContent.AppendLine("<table class=\"table\">");
                tableContent.AppendLine("<thead><tr><th>Titolo</th><th>TL;DR</th><th>Hash</th><th>Link</th></tr></thead>");
                tableContent.AppendLine("<tbody>");

                int total = subFolders.Count + mdFiles.Count;
                int processed = 0;

                // ---- Subfolders first (file-explorer convention) ----
                // A folder's description comes from the "Area appunti utente" safe zone
                // of its own TOC file (<subfolder>/<subfolder>.md.directory).
                foreach (var folder in subFolders)
                {
                    if (ct.IsCancellationRequested)
                    {
                        _logger.LogWarning("[TocGeneration] Cancellation requested");
                        break;
                    }

                    try
                    {
                        var folderName = Path.GetFileName(folder);
                        var folderTocPath = Path.Combine(folder, folderName + ".md.directory");
                        var notes = ExtractSafeZoneContent(folderTocPath);

                        // Only a note the user actually wrote counts as a description;
                        // a missing TOC or the untouched default placeholder does not.
                        var descCell = (string.IsNullOrWhiteSpace(notes) || notes == DefaultSafeZoneNote)
                            ? "<em>(nessuna descrizione — apri la cartella e compila «Area appunti utente»)</em>"
                            : MakeTableCellSafe(notes);

                        // Link to the subfolder's own TOC so the index stays navigable;
                        // when it has no TOC yet, show the bare folder name (no link).
                        string linkCell;
                        if (File.Exists(folderTocPath))
                        {
                            var folderTocRel = GetRelativePath(directoryPath, folderTocPath);
                            linkCell = $"<a href=\"{WebUtility.HtmlEncode(EncodePathForMarkdownUrl(folderTocRel))}\">{WebUtility.HtmlEncode(folderName)}/</a>";
                        }
                        else
                        {
                            linkCell = $"{WebUtility.HtmlEncode(folderName)}/";
                        }

                        tableContent.AppendLine($"<tr><td>📁 {WebUtility.HtmlEncode(folderName)}</td><td>{descCell}</td><td>—</td><td>{linkCell}</td></tr>");

                        processed++;
                        NotifyProgress(directoryPath, processed, total, $"Processing folder: {folderName}");
                    }
                    catch (Exception folderEx)
                    {
                        _logger.LogError($"[TocGeneration] Error processing folder {folder}: {folderEx.Message}");
                    }
                }

                // ---- Markdown files ----
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
                        var hashShort = ComputeShortHash(file);
                        var tldr = ExtractTldrFromFile(file);

                        var summaryCell = string.IsNullOrEmpty(tldr)
                            ? "<em>(TL;DR mancante — aggiungi <code>## TL;DR</code> al documento secondo la skill mde-doc)</em>"
                            : tldr;

                        tableContent.AppendLine($"<tr><td>{WebUtility.HtmlEncode(title)}</td><td>{summaryCell}</td><td><code>{hashShort}</code></td><td><a href=\"{WebUtility.HtmlEncode(urlPath)}\">{WebUtility.HtmlEncode(fileName)}</a></td></tr>");

                        processed++;
                        NotifyProgress(directoryPath, processed, total, $"Processing: {fileName}");
                    }
                    catch (Exception fileEx)
                    {
                        _logger.LogError($"[TocGeneration] Error processing file {file}: {fileEx.Message}");
                    }
                }

                tableContent.AppendLine("</tbody>");
                tableContent.AppendLine("</table>");

                await WriteTocFileAsync(tocFilePath, directoryPath, tableContent.ToString(), ct);

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

        // ============================ Safe zone (user notes) ============================
        //
        // The TOC file is regenerated from scratch on every call, so anything the user
        // adds would normally be lost. To keep their notes, a single "safe zone" delimited
        // by HTML-comment markers is carved out at the top of the document. Its content is
        // read back before each regeneration and re-emitted verbatim. Everything outside
        // the markers is overwritten.

        // Stable prefixes used to LOCATE the markers when reading an existing file.
        // Kept short so extra descriptive text inside the comment doesn't break matching.
        private const string SafeZoneStartTag = "<!-- MDE:SAFE-ZONE:START";
        private const string SafeZoneEndTag = "<!-- MDE:SAFE-ZONE:END";

        // Full marker lines emitted into the generated file.
        private const string SafeZoneStartLine =
            "<!-- MDE:SAFE-ZONE:START · area appunti utente — il testo fino a "
            + "MDE:SAFE-ZONE:END viene preservato a ogni rigenerazione · NON rimuovere questo marcatore -->";

        private const string SafeZoneEndLine =
            "<!-- MDE:SAFE-ZONE:END · NON rimuovere questo marcatore -->";

        // Visible heading rendered above the safe zone (regenerated every run, so it
        // can't be accidentally deleted by the user).
        private const string SafeZoneCallout = "> ✏️ **Area appunti utente**";

        // Seeded into the safe zone only on the very first generation (or for a TOC
        // produced before this feature existed, which has no markers at all).
        private const string DefaultSafeZoneNote =
            "_Scrivi qui le tue annotazioni su questa cartella: è l'unica parte del "
            + "documento che non viene sovrascritta._";

        private async Task WriteTocFileAsync(string tocFilePath, string directoryPath, string tableContent, CancellationToken ct)
        {
            var directoryName = Path.GetFileName(directoryPath);
            var yamlHeader = _yamlDefaultGenerator.GenerateDefaultYaml(directoryPath);

            // Carry over the user's notes. ExtractSafeZoneContent returns null only when
            // the existing file has no markers at all — then we seed the default note.
            var userNotes = ExtractSafeZoneContent(tocFilePath) ?? DefaultSafeZoneNote;

            var content = new StringBuilder();
            content.AppendLine(yamlHeader);
            content.AppendLine($"# {directoryName}");
            content.AppendLine();

            // ---- Safe zone: the only user-editable part of this generated file ----
            content.AppendLine(SafeZoneCallout);
            content.AppendLine();
            content.AppendLine(SafeZoneStartLine);
            content.AppendLine();
            content.AppendLine(userNotes);
            content.AppendLine();
            content.AppendLine(SafeZoneEndLine);
            content.AppendLine();
            // ---- everything below is regenerated on every run ----

            content.AppendLine("## 📑 Indice Rapido");
            content.AppendLine();
            content.Append(tableContent);

            await File.WriteAllTextAsync(tocFilePath, content.ToString(), Encoding.UTF8, ct);
        }

        /// <summary>
        /// Instance wrapper around <see cref="ReadSafeZone"/> used during TOC regeneration.
        /// Logs (rather than throws) on an IO failure: a read error here would otherwise
        /// silently drop the user's notes on the next write.
        /// </summary>
        private string ExtractSafeZoneContent(string tocFilePath)
        {
            try
            {
                return ReadSafeZone(tocFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[TocGeneration] Could not read safe-zone notes from '{Path}'", tocFilePath);
                return null;
            }
        }

        /// <summary>
        /// Reads back the user "safe zone" from an existing TOC file: the text between the
        /// <c>MDE:SAFE-ZONE:START</c> and <c>MDE:SAFE-ZONE:END</c> comment markers, trimmed.
        /// Returns <c>null</c> when the file does not exist or has no markers (first
        /// generation, or a TOC produced before this feature). An empty-but-marked zone is
        /// returned as an empty string. IO errors propagate to the caller.
        /// </summary>
        public static string ReadSafeZone(string tocFilePath)
        {
            if (!File.Exists(tocFilePath)) return null;

            var content = File.ReadAllText(tocFilePath, Encoding.UTF8);

            var startTag = content.IndexOf(SafeZoneStartTag, StringComparison.Ordinal);
            if (startTag < 0) return null;

            // Skip past the end of the START comment ("-->") to the actual notes.
            var afterStart = content.IndexOf("-->", startTag, StringComparison.Ordinal);
            if (afterStart < 0) return null;
            afterStart += "-->".Length;

            var endTag = content.IndexOf(SafeZoneEndTag, afterStart, StringComparison.Ordinal);
            if (endTag < 0) return null;

            return content.Substring(afterStart, endTag - afterStart)
                          .Trim('\r', '\n', ' ', '\t');
        }

        /// <summary>
        /// True when the safe zone is "untouched by a human": no markers / no content, or
        /// exactly the seeded <see cref="DefaultSafeZoneNote"/> placeholder. Used to decide
        /// whether an automated synthesis may fill it (a user-written note is never overwritten).
        /// </summary>
        public static bool IsSafeZoneEmptyOrDefault(string tocFilePath)
        {
            var notes = ReadSafeZone(tocFilePath);
            return string.IsNullOrWhiteSpace(notes) || notes == DefaultSafeZoneNote;
        }

        /// <summary>
        /// Replaces the safe-zone notes of an existing TOC file in place, between the
        /// <c>MDE:SAFE-ZONE:START</c> and <c>MDE:SAFE-ZONE:END</c> markers. Everything
        /// outside the markers (heading, table, YAML) is preserved verbatim. Throws if the
        /// markers are missing — the caller must only invoke this on a TOC it just generated.
        /// </summary>
        public static void WriteSafeZone(string tocFilePath, string newContent)
        {
            var content = File.ReadAllText(tocFilePath, Encoding.UTF8);

            var startTag = content.IndexOf(SafeZoneStartTag, StringComparison.Ordinal);
            if (startTag < 0)
                throw new InvalidOperationException($"Safe-zone start marker not found in '{tocFilePath}'.");

            var afterStart = content.IndexOf("-->", startTag, StringComparison.Ordinal);
            if (afterStart < 0)
                throw new InvalidOperationException($"Malformed safe-zone start marker in '{tocFilePath}'.");
            afterStart += "-->".Length;

            var endTag = content.IndexOf(SafeZoneEndTag, afterStart, StringComparison.Ordinal);
            if (endTag < 0)
                throw new InvalidOperationException($"Safe-zone end marker not found in '{tocFilePath}'.");

            var nl = Environment.NewLine;
            var rebuilt = content.Substring(0, afterStart)
                          + nl + nl + (newContent ?? string.Empty).Trim() + nl + nl
                          + content.Substring(endTag);

            File.WriteAllText(tocFilePath, rebuilt, Encoding.UTF8);
        }

        // ============================ TL;DR extraction ============================

        /// <summary>
        /// Deterministically extracts the TL;DR block from a markdown document produced under the
        /// mde-doc skill. Looks for a heading matching <c>## TL;DR</c> (case-insensitive, also
        /// accepts <c>TLDR</c> and trailing semicolons) and gathers prose + bullet lines until the
        /// next <c>##</c> heading or EOF. The prose becomes the visible cell preview; the whole
        /// bullet list (with its nesting) is always moved into a collapsible <c>&lt;details&gt;</c>
        /// block. Returns a single-line, table-cell-safe string, or null if no TL;DR is found.
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
                var rawBullets = new List<(int Indent, string Text)>();
                bool sawBullet = false;
                // A blank line seen after a bullet does NOT decide on its own — the next
                // non-blank line does. This lets the loop tell apart a hard-wrapped bullet,
                // a "loose" list (blank lines between items) and trailing prose.
                bool pendingBlank = false;

                for (int i = startIdx; i < lines.Length; i++)
                {
                    var raw = lines[i].TrimEnd('\r');
                    var trimmed = raw.Trim();

                    // Next ## heading → the TL;DR section ends here.
                    if (trimmed.StartsWith("##", StringComparison.Ordinal) &&
                        !trimmed.StartsWith("###", StringComparison.Ordinal))
                    {
                        break;
                    }

                    // Blank line: defer the decision to the next non-blank line.
                    if (string.IsNullOrEmpty(trimmed))
                    {
                        if (sawBullet) pendingBlank = true;
                        continue;
                    }

                    // A real markdown bullet is "- "/"* " (marker + space) or a bare "-"/"*".
                    // Anything else following a bullet is continuation text, not a new item.
                    bool isBullet =
                        trimmed.StartsWith("- ", StringComparison.Ordinal) ||
                        trimmed.StartsWith("* ", StringComparison.Ordinal) ||
                        trimmed == "-" || trimmed == "*";

                    // Resolve a deferred blank line now:
                    //   blank + bullet     → a "loose" list gap → keep collecting;
                    //   blank + non-bullet → trailing prose → the TL;DR list is over.
                    if (pendingBlank)
                    {
                        if (!isBullet) break;
                        pendingBlank = false;
                    }

                    if (isBullet)
                    {
                        // Leading whitespace width drives the nesting level (mapped below).
                        int indent = raw.Length - raw.TrimStart(' ', '\t').Length;
                        rawBullets.Add((indent, trimmed.TrimStart('-', '*').Trim()));
                        sawBullet = true;
                    }
                    else if (!sawBullet)
                    {
                        prose.Add(trimmed);
                    }
                    else if (rawBullets.Count > 0)
                    {
                        // Non-bullet line right after a bullet, with no blank line between:
                        // a hard-wrapped continuation → fold it back into the last bullet.
                        var last = rawBullets[rawBullets.Count - 1];
                        rawBullets[rawBullets.Count - 1] = (last.Indent, (last.Text + " " + trimmed).Trim());
                    }
                }

                if (prose.Count == 0 && rawBullets.Count == 0) return null;

                // The TL;DR prose stays visible in the cell; the whole bullet list is
                // always tucked into a collapsible <details> block — collapsed even when
                // it would still fit — so the table stays scannable while the expanded
                // view keeps the original list structure (incl. nested points).
                var bullets = MapBulletLevels(rawBullets);
                var proseText = WebUtility.HtmlEncode(string.Join(" ", prose));
                var bulletHtml = bullets.Count > 0 ? BuildBulletListHtml(bullets) : null;

                return ComposeTldrCell(proseText, bulletHtml, bullets.Count);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Maps each bullet's raw leading-whitespace width to a 0-based nesting level.
        /// Distinct indent widths are sorted ascending and their position becomes the
        /// level, so 2-space, 4-space or tab indentation all collapse cleanly to 0,1,2,…
        /// </summary>
        private static List<(int Level, string Text)> MapBulletLevels(
            List<(int Indent, string Text)> raw)
        {
            var result = new List<(int Level, string Text)>();
            if (raw.Count == 0) return result;

            var distinctIndents = raw.Select(b => b.Indent).Distinct().OrderBy(x => x).ToList();
            foreach (var b in raw)
            {
                result.Add((distinctIndents.IndexOf(b.Indent), b.Text));
            }
            return result;
        }

        /// <summary>
        /// Renders a leveled bullet list as a properly nested HTML <c>&lt;ul&gt;/&lt;li&gt;</c>
        /// tree on a single physical line (no newlines), so it survives inside a markdown
        /// table cell while still rendering as a real, indented list when expanded.
        /// </summary>
        private static string BuildBulletListHtml(List<(int Level, string Text)> bullets)
        {
            var sb = new StringBuilder();
            int prevLevel = -1;

            foreach (var (level, text) in bullets)
            {
                if (level > prevLevel)
                {
                    for (int l = prevLevel; l < level; l++) sb.Append("<ul>");
                }
                else if (level < prevLevel)
                {
                    for (int l = prevLevel; l > level; l--) sb.Append("</li></ul>");
                    sb.Append("</li>");
                }
                else
                {
                    sb.Append("</li>");
                }

                sb.Append("<li>").Append(WebUtility.HtmlEncode(text));
                prevLevel = level;
            }

            for (int l = prevLevel; l >= 0; l--) sb.Append("</li></ul>");
            return sb.ToString();
        }

        /// <summary>
        /// Assembles the final table-cell string: the full TL;DR prose stays inline and
        /// the bullet list goes into an inline collapsible <c>&lt;details&gt;</c>. The cell
        /// never contains a newline.
        /// </summary>
        private static string ComposeTldrCell(string proseText, string bulletHtml, int bulletCount)
        {
            // Nothing to collapse: no bullets → just the prose.
            if (bulletHtml == null)
                return proseText;

            var summary = bulletCount == 1 ? "📋 1 punto" : $"📋 {bulletCount} punti";

            var details = new StringBuilder();
            details.Append("<details style=\"display:inline\">")
                   .Append("<summary style=\"display:inline;color:#0d6efd;cursor:pointer;font-weight:bold\" title=\"Mostra tutto\">")
                   .Append(summary)
                   .Append("</summary>")
                   .Append(bulletHtml)
                   .Append("</details>");

            return string.IsNullOrEmpty(proseText)
                ? details.ToString()
                : proseText + " " + details.ToString();
        }

        // ============================ Hash ============================

        private static string ComputeFileHash(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Short content hash of a file: the first 8 chars of the MD5 hex digest.
        /// This is the exact value rendered in the TOC "Hash" column. It is exposed so
        /// other services (e.g. the Mark folder summarizer) can compare a document
        /// against its recorded TOC hash without re-implementing the algorithm — keeping
        /// a single source of truth and avoiding hash drift.
        /// </summary>
        public static string ComputeShortHash(string filePath)
        {
            var hash = ComputeFileHash(filePath);
            return hash.Substring(0, Math.Min(8, hash.Length));
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

        /// <summary>
        /// Makes an arbitrary (possibly multi-line) string safe to drop into a single
        /// HTML <c>&lt;td&gt;</c>: HTML-encodes the text (so any <c>&lt;</c> / <c>&gt;</c>
        /// / <c>&amp;</c> in the user's notes is literal, not interpreted as a tag) and
        /// converts line breaks into <c>&lt;br&gt;</c>.
        /// </summary>
        private static string MakeTableCellSafe(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var unified = text.Replace("\r\n", "\n").Replace("\r", "\n");
            return WebUtility.HtmlEncode(unified).Replace("\n", "<br>");
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
