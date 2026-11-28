using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Services.AI.Codex;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// Provider per <b>OpenAI Codex</b>, accanto a quelli di Copilot, Claude Code e opencode.
    /// <para>
    /// La chat <b>non passa da qui</b>: usa la sessione del <see cref="CodexSessionPool"/>, che conserva la conversazione.
    /// Questa classe serve a farsi riconoscere come motore disponibile (l'hub cerca un <see cref="IAiProvider"/> del tipo del
    /// motore prima di instradare il turno, e senza rispondeva «provider is not available») e ai pochi chiamanti «una
    /// domanda, una risposta», in sola lettura.
    /// </para>
    /// <para>Nessuna API key in MdExplorer: l'accesso è quello di <c>codex login</c>.</para>
    /// <para>Sprint: docs-internal/Sprints/2026-11-28-Motore-Codex.md, fase F1.</para>
    /// </summary>
    public class CodexProvider : IAiProvider
    {
        private readonly ILogger<CodexProvider> _logger;

        /// <summary>Cartella per i chiamanti «una domanda, una risposta»: Codex lavora dentro il progetto.</summary>
        public string WorkingDirectory { get; set; }

        public CodexProvider(ILogger<CodexProvider> logger) => _logger = logger;

        public string GetName() => "Codex";

        public ProviderType GetProviderType() => ProviderType.Codex;

        /// <summary>Installazione, non accesso: scansione del PATH, nessun processo avviato.</summary>
        public bool IsAvailable() => CodexProcessLauncher.IsResolvable();

        public ProviderCapabilities GetCapabilities() => new()
        {
            SupportsStreaming = true,
            // Codex usa i propri strumenti: un elenco di funzioni di MdExplorer verrebbe ignorato (come Claude e opencode).
            SupportsFunctionCalling = false,
            SupportsEmbeddings = false,
            SupportsVision = false,
            MaxInputTokens = 0,
            MaxOutputTokens = 0,
            AvailableModels = null,
        };

        public async Task<string> ChatAsync(string prompt, string modelId = null, CancellationToken ct = default)
        {
            var text = new StringBuilder();
            await foreach (var piece in StreamChatAsync(prompt, modelId, ct).ConfigureAwait(false)) text.Append(piece);
            return text.ToString();
        }

        /// <summary>Un turno isolato in sola lettura: sessione nuova, usata e chiusa; non tocca quelle della chat.</summary>
        public async IAsyncEnumerable<string> StreamChatAsync(string prompt, string modelId = null, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var directory = WorkingDirectory;
            if (string.IsNullOrWhiteSpace(directory))
                throw new InvalidOperationException(UiText.Say("Codex va eseguito dentro il progetto: WorkingDirectory non è stata impostata.",
                    "Codex must run inside the project: WorkingDirectory was not set."));

            await using var session = new CodexSession(_logger, directory, modelId, new CodexSessionOptions { Sandbox = CodexSandbox.ReadOnly });
            await session.StartAsync(ct).ConfigureAwait(false);
            await foreach (var chunk in session.PromptAsync(prompt, ct).ConfigureAwait(false))
                if (chunk.Kind == CodexChunk.KindMessage) yield return chunk.Text;
        }

        public Task<string> ChatWithToolsAsync(string prompt, List<object> tools, Func<string, dynamic, Task<object>> toolExecutor,
            string modelId = null, string currentDocumentPath = null, List<object> conversationHistory = null, CancellationToken ct = default)
        {
            if (tools != null && tools.Count > 0)
                _logger.LogInformation("[Codex] {Count} tool di MdExplorer ignorati: Codex usa i propri strumenti sul progetto", tools.Count);
            return ChatAsync(prompt, modelId, ct);
        }

        /// <summary>Le istruzioni di Codex stanno in <c>AGENTS.md</c> e in <c>.codex/</c>, non in una manopola di MdExplorer.</summary>
        public Task SetSystemPromptAsync(string systemPrompt) => throw new NotSupportedException(UiText.Say(
            "Codex prende le istruzioni dai file del progetto (AGENTS.md e la cartella .codex), non da un prompt di sistema impostato da MdExplorer.",
            "Codex takes its instructions from the project files (AGENTS.md and the .codex folder), not from a system prompt set by MdExplorer."));

        public Task<string> GetSystemPromptAsync() => Task.FromResult<string>(null);
        public Task<string> GetApiKeyAsync() => Task.FromResult("not-required");
        public Task SaveApiKeyAsync(string apiKey) => Task.CompletedTask;
        public Task<bool> TestApiKeyAsync(string apiKey) => Task.FromResult(IsAvailable());
    }
}
