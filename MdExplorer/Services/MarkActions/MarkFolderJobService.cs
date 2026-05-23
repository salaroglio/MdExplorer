using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.AI;
using MdExplorer.Hubs;
using MdExplorer.Service.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.MarkActions
{
    /// <summary>
    /// Singleton implementation of the Mark folder-summarizer job. See
    /// <see cref="IMarkFolderJobService"/>. Fire-and-forget, one job per SignalR
    /// connection; progress is streamed via <c>markFolderProgress</c>.
    /// </summary>
    public class MarkFolderJobService : IMarkFolderJobService
    {
        private const string ProgressEvent = "markFolderProgress";
        private const string DefaultProviderKey = "AI_DefaultProvider";

        // Soft cap on the document text sent to the LLM. Larger documents are truncated
        // (the TL;DR only needs the gist); a note event is emitted when this happens.
        private const int MaxDocChars = 24000;

        private readonly ILogger<MarkFolderJobService> _logger;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly IEnumerable<IAiProvider> _aiProviders;
        private readonly LocalLlamaProvider _localProvider;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FoldersIgnoreService _foldersIgnoreService;

        private readonly ConcurrentDictionary<string, CancellationTokenSource> _running = new();

        public MarkFolderJobService(
            ILogger<MarkFolderJobService> logger,
            IHubContext<MonitorMDHub> hubContext,
            IEnumerable<IAiProvider> aiProviders,
            LocalLlamaProvider localProvider,
            IServiceScopeFactory scopeFactory,
            FoldersIgnoreService foldersIgnoreService)
        {
            _logger = logger;
            _hubContext = hubContext;
            _aiProviders = aiProviders;
            _localProvider = localProvider;
            _scopeFactory = scopeFactory;
            _foldersIgnoreService = foldersIgnoreService;
        }

        // NOTE: deliberately NOT async — the registry check must run synchronously so the
        // controller can catch the "already running" exception before the request returns.
        public Task RunSummarizeAsync(string connectionId, string folderFullPath, string projectPath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("connectionId is required", nameof(connectionId));

            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            if (!_running.TryAdd(connectionId, cts))
            {
                cts.Dispose();
                throw new InvalidOperationException(
                    "Un'operazione di Mark è già in corso per questa sessione.");
            }

            return RunInternalAsync(connectionId, folderFullPath, projectPath, cts);
        }

        public void Cancel(string connectionId)
        {
            if (!string.IsNullOrWhiteSpace(connectionId) && _running.TryGetValue(connectionId, out var cts))
            {
                try { cts.Cancel(); }
                catch (ObjectDisposedException) { /* job already finished */ }
            }
        }

        private async Task RunInternalAsync(
            string connectionId, string folderFullPath, string projectPath, CancellationTokenSource cts)
        {
            var ct = cts.Token;
            var summarized = 0;
            var skipped = 0;
            var tocs = 0;
            var synthesized = 0;
            try
            {
                _logger.LogInformation(
                    "[MarkFolderJob] STARTED folder='{Folder}' connectionId='{Conn}'", folderFullPath, connectionId);

                if (!Directory.Exists(folderFullPath))
                    throw new DirectoryNotFoundException($"Folder not found: {folderFullPath}");

                // ---- Resolve the active AI provider (fixed for the whole job) ----
                var provider = ResolveProvider();
                if (provider == null)
                {
                    await SendAsync(connectionId, new
                    {
                        phase = "error",
                        message = "no-provider"
                    });
                    _logger.LogWarning("[MarkFolderJob] No AI provider available — aborting");
                    return;
                }
                _logger.LogInformation("[MarkFolderJob] Using AI provider '{Provider}'", provider.GetName());
                if (provider is CopilotCliProvider copilot && Directory.Exists(projectPath))
                {
                    copilot.WorkingDirectory = projectPath;
                }

                // ---- Load the precooked prompts (deployed by MdeSkillUpdater) ----
                var summarizePrompt = LoadPromptBody(projectPath, "mde-mark-summarize.prompt.md");
                var synthesisPrompt = LoadPromptBody(projectPath, "mde-mark-folder-synthesis.prompt.md");

                // ---- Enumerate the subtree bottom-up (leaf folders first) ----
                var folders = EnumerateFoldersBottomUp(folderFullPath, projectPath);
                _logger.LogInformation("[MarkFolderJob] {Count} folder(s) to process (bottom-up)", folders.Count);

                await SendAsync(connectionId, new
                {
                    phase = "started",
                    provider = provider.GetName(),
                    folderTotal = folders.Count
                });

                for (var fi = 0; fi < folders.Count; fi++)
                {
                    ct.ThrowIfCancellationRequested();
                    var folder = folders[fi];
                    var folderName = Path.GetFileName(folder);
                    var tocPath = Path.Combine(folder, folderName + ".md.directory");

                    // (a) Documents — summarize the ones whose content changed.
                    var docs = Directory.GetFiles(folder, "*.md", SearchOption.TopDirectoryOnly)
                        .Where(f => !f.EndsWith(".md.directory", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var tocHashes = ReadTocHashes(tocPath);

                    for (var di = 0; di < docs.Count; di++)
                    {
                        ct.ThrowIfCancellationRequested();
                        var docPath = docs[di];
                        var docName = Path.GetFileName(docPath);

                        string currentHash;
                        try { currentHash = TocGenerationService.ComputeShortHash(docPath); }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "[MarkFolderJob] Cannot hash '{Doc}'", docPath);
                            continue;
                        }

                        if (tocHashes.TryGetValue(docName, out var recorded) && recorded == currentHash)
                        {
                            skipped++;
                            await SendAsync(connectionId, new
                            {
                                phase = "skipped-doc",
                                folderName,
                                docName,
                                docIndex = di + 1,
                                docTotal = docs.Count,
                                folderIndex = fi + 1,
                                folderTotal = folders.Count
                            });
                            continue;
                        }

                        await SendAsync(connectionId, new
                        {
                            phase = "summarizing-doc",
                            folderName,
                            docName,
                            docIndex = di + 1,
                            docTotal = docs.Count,
                            folderIndex = fi + 1,
                            folderTotal = folders.Count
                        });

                        try
                        {
                            var docText = await File.ReadAllTextAsync(docPath, ct);
                            if (docText.Length > MaxDocChars)
                                docText = docText.Substring(0, MaxDocChars);

                            var prompt = summarizePrompt
                                         + Environment.NewLine + Environment.NewLine
                                         + "---- DOCUMENTO ----" + Environment.NewLine
                                         + docText;

                            var answer = await provider.ChatAsync(prompt, null, ct);
                            var tldrBlock = TldrDocumentWriter.NormalizeTldrBlock(answer);
                            if (tldrBlock == null)
                            {
                                _logger.LogWarning("[MarkFolderJob] Empty/invalid TL;DR for '{Doc}' — left unchanged", docPath);
                                continue;
                            }

                            TldrDocumentWriter.UpsertTldrBlock(docPath, tldrBlock);
                            summarized++;
                        }
                        catch (OperationCanceledException) { throw; }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "[MarkFolderJob] Summarization failed for '{Doc}' — continuing", docPath);
                        }
                    }

                    // (b) Regenerate the folder TOC.
                    ct.ThrowIfCancellationRequested();
                    await SendAsync(connectionId, new
                    {
                        phase = "generating-toc",
                        folderName,
                        folderIndex = fi + 1,
                        folderTotal = folders.Count
                    });

                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var toc = scope.ServiceProvider.GetRequiredService<TocGenerationService>();
                        await toc.GenerateTocAsync(folder, tocPath, ct);
                        tocs++;
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[MarkFolderJob] TOC generation failed for '{Folder}'", folder);
                        continue; // can't synthesize a safe zone without a TOC
                    }

                    // (c) Fill the "Area appunti utente" when the user hasn't annotated it.
                    ct.ThrowIfCancellationRequested();
                    if (TocGenerationService.IsSafeZoneEmptyOrDefault(tocPath))
                    {
                        var digest = BuildTldrDigest(tocPath);
                        if (!string.IsNullOrWhiteSpace(digest))
                        {
                            await SendAsync(connectionId, new
                            {
                                phase = "synthesizing-folder",
                                folderName,
                                folderIndex = fi + 1,
                                folderTotal = folders.Count
                            });

                            try
                            {
                                var prompt = synthesisPrompt
                                             + Environment.NewLine + Environment.NewLine
                                             + "---- TL;DR DEI DOCUMENTI ----" + Environment.NewLine
                                             + digest;

                                var answer = await provider.ChatAsync(prompt, null, ct);
                                var synthesis = SanitizeSynthesis(answer);
                                if (synthesis != null)
                                {
                                    TocGenerationService.WriteSafeZone(tocPath, synthesis);
                                    synthesized++;
                                }
                                else
                                {
                                    _logger.LogWarning("[MarkFolderJob] Empty synthesis for '{Folder}' — safe zone left default", folder);
                                }
                            }
                            catch (OperationCanceledException) { throw; }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "[MarkFolderJob] Folder synthesis failed for '{Folder}'", folder);
                            }
                        }
                    }
                }

                await SendAsync(connectionId, new
                {
                    phase = "done",
                    summarized,
                    skipped,
                    tocs,
                    synthesized
                });
                _logger.LogInformation(
                    "[MarkFolderJob] COMPLETED folder='{Folder}' summarized={S} skipped={K} tocs={T} synthesized={Y}",
                    folderFullPath, summarized, skipped, tocs, synthesized);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("[MarkFolderJob] CANCELLED folder='{Folder}'", folderFullPath);
                await SendAsync(connectionId, new { phase = "cancelled" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MarkFolderJob] ERROR folder='{Folder}'", folderFullPath);
                await SendAsync(connectionId, new { phase = "error", message = ex.Message });
            }
            finally
            {
                _running.TryRemove(connectionId, out _);
                cts.Dispose();
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // AI provider resolution — mirrors GitCommitAiService: AI_DefaultProvider
        // setting → copilotcli → gemini → openai → local, first one IsAvailable().
        // ─────────────────────────────────────────────────────────────────────
        private IAiProvider ResolveProvider()
        {
            var byKey = (_aiProviders ?? Enumerable.Empty<IAiProvider>())
                .Concat(new IAiProvider[] { _localProvider })
                .Where(p => p != null)
                .GroupBy(p => ProviderKey(p.GetProviderType()))
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var key in BuildProviderOrder(ReadDefaultProviderSetting()))
            {
                if (!byKey.TryGetValue(key, out var provider)) continue;
                try
                {
                    if (provider.IsAvailable()) return provider;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[MarkFolderJob] Provider '{Key}' availability check failed", key);
                }
            }
            return null;
        }

        private static string ProviderKey(ProviderType type) => type switch
        {
            ProviderType.CopilotCli => "copilotcli",
            ProviderType.Gemini => "gemini",
            ProviderType.OpenAI => "openai",
            ProviderType.Local => "local",
            _ => type.ToString().ToLowerInvariant()
        };

        private static IEnumerable<string> BuildProviderOrder(string preferred)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(preferred) && seen.Add(preferred.ToLowerInvariant()))
                yield return preferred.ToLowerInvariant();
            foreach (var fallback in new[] { "copilotcli", "gemini", "openai", "local" })
                if (seen.Add(fallback)) yield return fallback;
        }

        private string ReadDefaultProviderSetting()
        {
            // IUserSettingsDB is a shared NHibernate session — even reads must sit inside
            // an explicit transaction or other controllers' Commit() breaks. Resolve it
            // from a short-lived scope.
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetService<IUserSettingsDB>();
                if (db == null) return null;
                db.BeginTransaction();
                var settings = db.GetDal<Setting>().GetList().ToList();
                db.Commit();
                return settings.FirstOrDefault(s => s.Name == DefaultProviderKey)?.ValueString;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[MarkFolderJob] Could not read {Key} setting (non-fatal)", DefaultProviderKey);
                return null;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Folder enumeration — post-order DFS (children before parent).
        // ─────────────────────────────────────────────────────────────────────
        private List<string> EnumerateFoldersBottomUp(string root, string projectPath)
        {
            var result = new List<string>();
            void Recurse(string dir)
            {
                IEnumerable<string> subs;
                try
                {
                    subs = Directory.GetDirectories(dir)
                        .Where(d => !Path.GetFileName(d).StartsWith(".", StringComparison.Ordinal))
                        .Where(d => !_foldersIgnoreService.ShouldIgnoreFolderForProject(d, projectPath))
                        .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[MarkFolderJob] Cannot list subfolders of '{Dir}'", dir);
                    subs = Enumerable.Empty<string>();
                }
                foreach (var sub in subs) Recurse(sub);
                result.Add(dir); // self after children → leaves first, root last
            }
            Recurse(root);
            return result;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Precooked prompt loading.
        // ─────────────────────────────────────────────────────────────────────
        private static string LoadPromptBody(string projectPath, string fileName)
        {
            var path = Path.Combine(projectPath, ".github", "prompts", fileName);
            if (!File.Exists(path))
                throw new FileNotFoundException(
                    $"Prompt precotto mancante: '{path}'. Riapri il progetto per rigenerare i file in .github/prompts/.");

            var text = File.ReadAllText(path);

            // Strip leading YAML frontmatter.
            var fm = Regex.Match(text, @"^---[ \t]*\r?\n.*?\r?\n---[ \t]*\r?\n", RegexOptions.Singleline);
            if (fm.Success && fm.Index == 0) text = text.Substring(fm.Length);

            // Strip HTML comment blocks (the MdE-managed banner).
            text = Regex.Replace(text, @"<!--.*?-->", string.Empty, RegexOptions.Singleline);

            // Strip any leftover bare "---" separator lines at the top.
            text = text.Trim();
            while (text.StartsWith("---", StringComparison.Ordinal))
            {
                var nl = text.IndexOf('\n');
                if (nl < 0) break;
                if (text.Substring(0, nl).Trim() != "---") break;
                text = text.Substring(nl + 1).TrimStart();
            }
            return text.Trim();
        }

        // ─────────────────────────────────────────────────────────────────────
        // TOC table parsing.
        //
        // The TOC index is emitted as a raw HTML <table> block (see TocGenerationService),
        // with each body row on a single physical line — so a simple per-line regex over
        // <tr>…</tr> with non-greedy <td>…</td> cells is enough; no markdown escaping to
        // un-do, no risk of cell counts shifting under emphasis spanning.
        // ─────────────────────────────────────────────────────────────────────
        private static readonly Regex RowRegex = new Regex(
            @"<tr>(.*?)</tr>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex CellRegex = new Regex(
            @"<td[^>]*>(.*?)</td>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex HashRegex = new Regex(
            @"<code[^>]*>([^<]+)</code>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex LinkRegex = new Regex(
            @"<a\b[^>]*>([^<]+)</a>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Dictionary<string, string> ReadTocHashes(string tocFilePath)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(tocFilePath)) return map;

            foreach (var line in File.ReadLines(tocFilePath))
            {
                var row = RowRegex.Match(line);
                if (!row.Success) continue;

                var cells = CellRegex.Matches(row.Groups[1].Value);
                if (cells.Count < 4) continue;

                // Column layout: 0=Titolo, 1=TL;DR, 2=Hash (<code>…</code>), 3=Link (<a>…</a>)
                var hashMatch = HashRegex.Match(cells[2].Groups[1].Value);
                if (!hashMatch.Success) continue;
                var hash = hashMatch.Groups[1].Value.Trim();
                if (hash.Length != 8 || !hash.All(Uri.IsHexDigit)) continue; // folder row ('—') or noise

                var linkMatch = LinkRegex.Match(cells[3].Groups[1].Value);
                if (!linkMatch.Success) continue;
                var fileName = WebUtility.HtmlDecode(linkMatch.Groups[1].Value).Trim();
                if (fileName.EndsWith("/", StringComparison.Ordinal)) continue; // folder link

                map[fileName] = hash;
            }
            return map;
        }

        private static string BuildTldrDigest(string tocFilePath)
        {
            var sb = new StringBuilder();
            foreach (var line in File.ReadLines(tocFilePath))
            {
                var row = RowRegex.Match(line);
                if (!row.Success) continue;

                var cells = CellRegex.Matches(row.Groups[1].Value);
                if (cells.Count < 4) continue;

                var title = StripHtml(cells[0].Groups[1].Value);
                if (title.StartsWith("📁", StringComparison.Ordinal))
                    title = title.Substring("📁".Length).TrimStart();

                var tldr = StripHtml(cells[1].Groups[1].Value);

                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(tldr)) continue;
                // Skip the "no description" / "missing TL;DR" placeholders — they're "(...)" after tag-strip.
                if (tldr.StartsWith("(", StringComparison.Ordinal)) continue;

                sb.Append("- ").Append(title).Append(": ").AppendLine(tldr);
            }
            return sb.ToString().Trim();
        }

        /// <summary>Strips HTML tags and decodes entities; collapses whitespace; trims.</summary>
        private static string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            var noTags = Regex.Replace(html, @"<[^>]+>", " ");
            var decoded = WebUtility.HtmlDecode(noTags);
            return Regex.Replace(decoded, @"\s+", " ").Trim();
        }

        // ─────────────────────────────────────────────────────────────────────
        // LLM output sanitization.
        // ─────────────────────────────────────────────────────────────────────
        private static string SanitizeSynthesis(string raw)
        {
            var t = StripCodeFence(raw);
            if (string.IsNullOrWhiteSpace(t)) return null;

            // The safe zone wants a single readable paragraph.
            t = Regex.Replace(t, @"\s+", " ").Trim();
            // Strip any leading bullet/heading marker the LLM prepended despite the prompt.
            t = Regex.Replace(t, @"^(?:[-*+•●‣▪·–—>]+|#{1,6})\s+", string.Empty).Trim();
            return t.Length == 0 ? null : t;
        }

        private static string StripCodeFence(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var t = raw.Trim();
            if (!t.StartsWith("```", StringComparison.Ordinal)) return t;

            var firstNl = t.IndexOf('\n');
            if (firstNl >= 0) t = t.Substring(firstNl + 1);
            var lastFence = t.LastIndexOf("```", StringComparison.Ordinal);
            if (lastFence >= 0) t = t.Substring(0, lastFence);
            return t.Trim();
        }

        // ─────────────────────────────────────────────────────────────────────
        private async Task SendAsync(string connectionId, object payload)
        {
            try
            {
                await _hubContext.Clients.Client(connectionId).SendAsync(ProgressEvent, payload);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[MarkFolderJob] SignalR send failed");
            }
        }
    }
}
