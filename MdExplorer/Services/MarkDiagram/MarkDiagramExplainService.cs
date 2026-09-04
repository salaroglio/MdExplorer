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
using MdExplorer.Features.Services.AI;
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

                await SendStatusAsync(connectionId, boxName, "Cerco il motore AI configurato...");

                var provider = ResolveConfiguredProvider(projectPath, out var modelId, out var whyNot);
                if (provider == null)
                {
                    // No silent fallback to "some other provider that happens to work":
                    // the user configured a reference LLM, or did not. Say which.
                    await SendAsync(connectionId, new { phase = "error", box = boxName, message = whyNot });
                    return;
                }

                var engineLabel = string.IsNullOrWhiteSpace(modelId)
                    ? provider.GetName()
                    : $"{provider.GetName()} ({modelId})";

                var documentName = System.IO.Path.GetFileName(context?.DocumentPath ?? string.Empty);
                await SendStatusAsync(connectionId, boxName,
                    string.IsNullOrWhiteSpace(documentName)
                        ? "Leggo il documento..."
                        : $"Leggo {documentName}...");

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

                var relationCount = context?.Relations?.Count ?? 0;
                await SendStatusAsync(connectionId, boxName,
                    $"Chiedo a {engineLabel} di spiegare \"{boxName}\" " +
                    $"({relationCount} relazioni, {documentText.Length / 1000} KB di documento)...");

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
        /// Returns the LLM the user actually chose, and only that one.
        ///
        /// MdExplorer expresses that choice in TWO independent places, and both count:
        ///
        ///   1. <c>Setting.AI_DefaultProvider</c> / <c>AI_DefaultModel</c> — global, set
        ///      explicitly from the AI preferences. Wins when present.
        ///   2. <c>Project.UseClaudeCodeAsDefault</c> / <c>UseCopilotCliAsDefault</c> —
        ///      per project, "use this CLI automatically when it is installed".
        ///
        /// Reading only the first one was a bug: <c>UseCopilotCliAsDefault</c> is born
        /// <c>true</c>, so a user who never opened the AI preferences still has a working
        /// engine everywhere else in the app — and MarkAgent alone claimed there was none.
        ///
        /// When both per-project flags are on, Claude Code wins. Its flag is born OFF, so
        /// finding it on is a deliberate choice, while Copilot's may simply be the default
        /// nobody touched: the explicit choice beats the default. Same rule as
        /// MdProjectsController, on purpose — two places must not arbitrate differently.
        ///
        /// What this method still refuses to do is walk a chain of substitutes: if the
        /// chosen engine is missing or unavailable, MarkAgent says so instead of answering
        /// through a model the user never picked.
        /// </summary>
        private IAiProvider? ResolveConfiguredProvider(string projectPath, out string? modelId, out string? whyNot)
        {
            modelId = null;
            whyNot = null;

            var byKey = _aiProviders?
                .Where(p => p != null)
                .GroupBy(p => ProviderKey(p.GetProviderType()))
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            if (byKey == null || byKey.Count == 0)
            {
                whyNot = "Nessun provider AI risulta registrato in questa installazione.";
                return null;
            }

            // 1 ─ Preferenza globale esplicita.
            var (preferredKey, preferredModel) = ReadDefaultPreferences();
            if (!string.IsNullOrWhiteSpace(preferredKey))
            {
                if (!byKey.TryGetValue(preferredKey, out var chosen))
                {
                    whyNot = $"Il provider configurato ('{preferredKey}') non risulta registrato in questa installazione.";
                    return null;
                }
                if (!IsUsable(chosen, projectPath, out var why))
                {
                    whyNot = why;
                    return null;
                }
                modelId = preferredModel;
                return chosen;
            }

            // 2 ─ Auto-select per progetto, con la precedenza di MdProjectsController.
            var (useClaudeCode, useCopilotCli) = ReadProjectAutoSelect(projectPath);

            if (useClaudeCode &&
                byKey.TryGetValue("claudecode", out var claude) &&
                IsUsable(claude, projectPath, out _))
            {
                // Alias, non nome pieno: punta sempre all'ultimo Sonnet e non invecchia.
                modelId = "sonnet";
                return claude;
            }

            if (useCopilotCli && byKey.TryGetValue("copilotcli", out var copilot))
            {
                if (IsUsable(copilot, projectPath, out var whyCopilot))
                {
                    // Nessun modello: il flag --model viene omesso e sceglie il CLI.
                    //
                    // Quali modelli esistano è una proprietà DELL'INSTALLAZIONE, non del
                    // programma: nessuna costante scritta qui può essere giusta ovunque.
                    // Verificato il 04/09/2026 — su questa macchina (Copilot CLI 1.0.82)
                    // claude-sonnet-5, gpt-5 e claude-haiku-4.5 sono tutti rifiutati con
                    // "Model ... is not available" e passa solo 'auto', mentre su altre
                    // installazioni esistono modelli che qui non ci sono. È lo stesso
                    // motivo per cui CopilotCliProvider non ha una costante di default.
                    //
                    // Chi vuole UN modello preciso lo dichiara nelle preferenze AI
                    // (AI_DefaultProvider + AI_DefaultModel): vivono nel DB utente, quindi
                    // hanno la stessa granularità del problema — per installazione. Quel
                    // ramo sta più in alto e vince su questo.
                    modelId = null;
                    return copilot;
                }
                // Il progetto ha scelto Copilot CLI ma non è utilizzabile: dire perché è
                // più utile del generico "nessun LLM configurato".
                whyNot = whyCopilot;
                return null;
            }

            whyNot = "Non ho un LLM di riferimento configurato. Impostalo nelle preferenze AI, "
                   + "oppure attiva un CLI nelle impostazioni del progetto.";
            return null;
        }

        /// <summary>
        /// Availability check. The CLI providers answer differently depending on the
        /// directory they run in, so the project path is handed to them first — the same
        /// thing MdProjectsController does when the project is opened.
        /// </summary>
        private bool IsUsable(IAiProvider provider, string projectPath, out string? whyNot)
        {
            whyNot = null;
            var key = ProviderKey(provider.GetProviderType());
            try
            {
                if (!string.IsNullOrWhiteSpace(projectPath))
                {
                    if (provider is CopilotCliProvider copilot) copilot.WorkingDirectory = projectPath;
                    else if (provider is ClaudeCodeProvider claude) claude.WorkingDirectory = projectPath;
                }

                if (provider.IsAvailable()) return true;

                whyNot = $"Il motore configurato ('{key}') non è al momento disponibile su questa macchina.";
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[MarkDiagram] Availability check failed for '{Key}'", key);
                whyNot = $"Non riesco a contattare il motore configurato ('{key}'): {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Per-project auto-select flags. A project row that cannot be found is treated as
        /// "nothing chosen here" rather than as the entity defaults: the defaults describe a
        /// project that exists, and inventing one would resurrect the silent fallback.
        /// </summary>
        private (bool useClaudeCode, bool useCopilotCli) ReadProjectAutoSelect(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return (false, false);
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetService<IUserSettingsDB>();
                if (db == null) return (false, false);

                db.BeginTransaction();
                var projects = db.GetDal<Project>().GetList().ToList();
                db.Commit();

                var project = projects.FirstOrDefault(p =>
                    string.Equals(p.Path, projectPath, StringComparison.OrdinalIgnoreCase));

                if (project == null)
                {
                    _logger.LogWarning("[MarkDiagram] No Project row for path {Path}", projectPath);
                    return (false, false);
                }

                return (project.UseClaudeCodeAsDefault, project.UseCopilotCliAsDefault);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[MarkDiagram] Could not read the per-project AI auto-select flags");
                return (false, false);
            }
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

        /// <summary>
        /// Racconta all'utente cosa sta succedendo mentre aspetta.
        /// L'attesa senza spiegazione fa sembrare rotto ciò che sta solo lavorando: qui il
        /// primo token può tardare parecchi secondi, perché prima si legge il documento e
        /// poi si avvia un CLI. Ogni riga viene sostituita dalla successiva, e tutte quante
        /// dalla risposta vera.
        /// </summary>
        private Task SendStatusAsync(string connectionId, string box, string message)
            => SendAsync(connectionId, new { phase = "status", box, message });

        private Task SendAsync(string connectionId, object payload)
            => _hubContext.Clients.Client(connectionId).SendAsync(StreamEvent, payload);
    }
}
