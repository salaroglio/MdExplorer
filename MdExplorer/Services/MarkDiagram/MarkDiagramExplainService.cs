using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.MarkDiagram
{
    /// <summary>
    /// See <see cref="IMarkDiagramExplainService"/>.
    ///
    /// Streams the answer chunk by chunk: Mark's dialog is a typewriter, and the
    /// model's own pace makes a better typewriter than a fixed character delay.
    /// </summary>
    public class MarkDiagramExplainService : IMarkDiagramExplainService
    {
        private const string StreamEvent = "markDiagramExplain";
        private const string DefaultProviderKey = "AI_DefaultProvider";
        private const string DefaultModelKey = "AI_DefaultModel";

        private readonly ILogger<MarkDiagramExplainService> _logger;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly IEnumerable<IAiProvider> _aiProviders;
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>One explanation in flight per connection: a new box supersedes the old one.</summary>
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _running = new();

        public MarkDiagramExplainService(
            ILogger<MarkDiagramExplainService> logger,
            IHubContext<MonitorMDHub> hubContext,
            IEnumerable<IAiProvider> aiProviders,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _hubContext = hubContext;
            _aiProviders = aiProviders;
            _scopeFactory = scopeFactory;
        }

        public async Task ExplainBoxAsync(
            string connectionId,
            MarkDiagramContextDto context,
            string projectPath,
            CancellationToken ct = default)
        {
            // Supersede the previous request for this connection: the user clicked
            // another box, the old answer is already stale on screen.
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            if (_running.TryRemove(connectionId, out var previous))
            {
                try { previous.Cancel(); } catch { /* already gone */ }
                previous.Dispose();
            }
            _running[connectionId] = cts;

            var boxName = context?.Box?.Name;

            try
            {
                await SendAsync(connectionId, new { phase = "start", box = boxName });

                var provider = ResolveConfiguredProvider(out var modelId, out var whyNot);
                if (provider == null)
                {
                    // No silent fallback to "some other provider that happens to work":
                    // the user configured a reference LLM, or did not. Say which.
                    await SendAsync(connectionId, new { phase = "error", box = boxName, message = whyNot });
                    return;
                }

                var documentText = ReadDocument(context, projectPath, out var truncated);

                var systemPrompt = MarkDiagramPromptBuilder.BuildSystemPrompt();
                var userPrompt = MarkDiagramPromptBuilder.BuildUserPrompt(context!, documentText, truncated);

                try
                {
                    await provider.SetSystemPromptAsync(systemPrompt);
                }
                catch (Exception ex)
                {
                    // Not every provider supports a separate system prompt; fold it in.
                    _logger.LogInformation(ex,
                        "[MarkDiagram] Provider '{Provider}' rejected SetSystemPromptAsync, inlining the rules",
                        provider.GetName());
                    userPrompt = systemPrompt + "\n\n" + userPrompt;
                }

                var answer = new StringBuilder();
                await foreach (var chunk in provider.StreamChatAsync(userPrompt, modelId, cts.Token))
                {
                    if (cts.Token.IsCancellationRequested) return;
                    if (string.IsNullOrEmpty(chunk)) continue;
                    answer.Append(chunk);
                    await SendAsync(connectionId, new { phase = "chunk", box = boxName, text = chunk });
                }

                var full = answer.ToString().Trim();

                // The ten-sentence rule is asked for in the prompt and checked here.
                // Deliberately NOT enforced by truncation: a reply cut mid-thought is
                // worse than a long one, and the fix belongs in the prompt.
                var sentences = MarkDiagramPromptBuilder.CountSentences(full);
                if (sentences > MarkDiagramPromptBuilder.MaxSentences)
                {
                    _logger.LogWarning(
                        "[MarkDiagram] Box '{Box}': the model answered with {Count} sentences, limit is {Max}",
                        boxName, sentences, MarkDiagramPromptBuilder.MaxSentences);
                }

                await SendAsync(connectionId, new { phase = "done", box = boxName, text = full, sentences });
            }
            catch (OperationCanceledException)
            {
                // Superseded by a newer box, or the user closed the document. Not an error.
                _logger.LogInformation("[MarkDiagram] Explanation for box '{Box}' cancelled", boxName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MarkDiagram] Explanation for box '{Box}' failed", boxName);
                await SendAsync(connectionId, new { phase = "error", box = boxName, message = ex.Message });
            }
            finally
            {
                if (_running.TryGetValue(connectionId, out var mine) && mine == cts)
                    _running.TryRemove(connectionId, out _);
                cts.Dispose();
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Document
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Reads the document that contains the diagram. The path comes from the page,
        /// so it is validated against the project root before being opened.
        /// </summary>
        private string ReadDocument(MarkDiagramContextDto? context, string projectPath, out bool truncated)
        {
            truncated = false;
            var documentPath = context?.DocumentPath;
            if (string.IsNullOrWhiteSpace(documentPath)) return string.Empty;

            try
            {
                var fullPath = Path.IsPathRooted(documentPath)
                    ? Path.GetFullPath(documentPath)
                    : Path.GetFullPath(Path.Combine(projectPath ?? string.Empty, documentPath.TrimStart('/', '\\')));

                if (!IsInsideProject(fullPath, projectPath))
                {
                    _logger.LogWarning("[MarkDiagram] Document outside the project, refusing to read: {Path}", fullPath);
                    return string.Empty;
                }

                if (!File.Exists(fullPath))
                {
                    _logger.LogWarning("[MarkDiagram] Document not found: {Path}", fullPath);
                    return string.Empty;
                }

                var text = File.ReadAllText(fullPath);
                if (text.Length > MarkDiagramPromptBuilder.MaxDocChars)
                {
                    truncated = true;
                    text = text.Substring(0, MarkDiagramPromptBuilder.MaxDocChars);
                }
                return text;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[MarkDiagram] Could not read document {Path}", documentPath);
                return string.Empty;
            }
        }

        private static bool IsInsideProject(string fullPath, string? projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return false;
            var root = Path.GetFullPath(projectPath);
            if (!root.EndsWith(Path.DirectorySeparatorChar))
                root += Path.DirectorySeparatorChar;
            return fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Provider
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the provider the user configured as their reference LLM — and only
        /// that one. Unlike MarkFolderJobService, which walks a fallback chain, this
        /// feature refuses to answer through a provider the user did not choose:
        /// an explanation is worth only as much as the model behind it, so silently
        /// downgrading would be worse than saying nothing.
        /// </summary>
        private IAiProvider? ResolveConfiguredProvider(out string? modelId, out string? whyNot)
        {
            modelId = null;
            whyNot = null;

            var (preferredKey, preferredModel) = ReadDefaultPreferences();
            if (string.IsNullOrWhiteSpace(preferredKey))
            {
                whyNot = "Non ho un LLM di riferimento configurato. Impostalo nelle preferenze AI e riprova.";
                return null;
            }

            var byKey = _aiProviders?
                .Where(p => p != null)
                .GroupBy(p => ProviderKey(p.GetProviderType()))
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            if (byKey == null || !byKey.TryGetValue(preferredKey, out var provider))
            {
                whyNot = $"Il provider configurato ('{preferredKey}') non risulta registrato in questa installazione.";
                return null;
            }

            try
            {
                if (!provider.IsAvailable())
                {
                    whyNot = $"Il provider configurato ('{preferredKey}') non è al momento disponibile.";
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[MarkDiagram] Availability check failed for '{Key}'", preferredKey);
                whyNot = $"Non riesco a contattare il provider configurato ('{preferredKey}'): {ex.Message}";
                return null;
            }

            modelId = preferredModel;
            return provider;
        }

        private static string ProviderKey(ProviderType type) => type switch
        {
            ProviderType.CopilotCli => "copilotcli",
            ProviderType.Gemini => "gemini",
            ProviderType.OpenAI => "openai",
            ProviderType.Local => "local",
            _ => type.ToString().ToLowerInvariant()
        };

        /// <summary>
        /// IUserSettingsDB is a shared NHibernate session: even a read must sit inside
        /// an explicit transaction, or another controller's Commit() breaks. Hence the
        /// short-lived scope.
        /// </summary>
        private (string? provider, string? model) ReadDefaultPreferences()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetService<IUserSettingsDB>();
                if (db == null) return (null, null);

                db.BeginTransaction();
                var settings = db.GetDal<Setting>().GetList().ToList();
                db.Commit();

                return (
                    settings.FirstOrDefault(s => s.Name == DefaultProviderKey)?.ValueString,
                    settings.FirstOrDefault(s => s.Name == DefaultModelKey)?.ValueString
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[MarkDiagram] Could not read the AI default preferences");
                return (null, null);
            }
        }

        private Task SendAsync(string connectionId, object payload)
            => _hubContext.Clients.Client(connectionId).SendAsync(StreamEvent, payload);
    }
}
