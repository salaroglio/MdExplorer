using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Services.AI.OpenCode;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// Provider per <b>opencode</b>, accanto a quelli di Copilot e Claude Code.
    ///
    /// <para>La chat interattiva <b>non passa da qui</b>: quella usa la sessione del
    /// <see cref="OpenCodeSessionPool"/>, che conserva la memoria conversazionale. Questa classe
    /// serve ai chiamanti «una domanda, una risposta» che il resto di MdExplorer fa passare per
    /// <see cref="IAiProvider"/> (messaggio di commit, riassunti…) e a farsi riconoscere come
    /// motore disponibile — senza di lei la chat rispondeva «OpenCode provider is not
    /// available» pur avendo il motore configurato.</para>
    ///
    /// <para>Nessuna API key: le credenziali, se servono, stanno nella configurazione di
    /// opencode. MdExplorer non custodisce segreti per questo provider.</para>
    ///
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Motore-OpenCode.md, fase F4.</para>
    /// </summary>
    public class OpenCodeProvider : IAiProvider
    {
        private readonly ILogger<OpenCodeProvider> _logger;
        private readonly OpenCodeServer _server;

        /// <summary>
        /// Cartella per i chiamanti «una domanda, una risposta»: opencode lavora dentro il
        /// progetto, e senza questa leggerebbe il filesystem sbagliato.
        /// </summary>
        public string WorkingDirectory { get; set; }

        public OpenCodeProvider(ILogger<OpenCodeProvider> logger, OpenCodeServer server)
        {
            _logger = logger;
            _server = server;
        }

        public string GetName() => "opencode";

        public ProviderType GetProviderType() => ProviderType.OpenCode;

        /// <summary>
        /// Installazione, non autenticazione: scansione del PATH, nessun processo avviato.
        /// Accendere il server solo per rispondere «c'è» sarebbe un processo a vuoto, e
        /// confonderebbe due domande diverse — «è installato?» e «parte?».
        /// </summary>
        public bool IsAvailable() => OpenCodeProcessLauncher.IsResolvable();

        public ProviderCapabilities GetCapabilities()
        {
            return new ProviderCapabilities
            {
                SupportsStreaming = true,
                // Function calling nel senso di MdExplorer (tool nostri, eseguiti dal
                // ToolExecutor) NON è supportato: opencode porta i propri strumenti e li usa
                // da sé. Dichiararlo true gli farebbe arrivare un elenco di tool che verrebbe
                // ignorato — la stessa scelta fatta per Claude Code.
                SupportsFunctionCalling = false,
                SupportsEmbeddings = false,
                SupportsVision = false,
                // I limiti dipendono dal modello scelto sul server, non da opencode: qui non
                // si inventa un numero che sarebbe sbagliato per metà dei modelli.
                MaxInputTokens = 0,
                MaxOutputTokens = 0,
                AvailableModels = null
            };
        }

        public async Task<string> ChatAsync(string prompt, string modelId = null, CancellationToken ct = default)
        {
            var text = new StringBuilder();
            await foreach (var piece in StreamChatAsync(prompt, modelId, ct).ConfigureAwait(false))
            {
                text.Append(piece);
            }
            return text.ToString();
        }

        /// <summary>
        /// Un turno isolato: sessione nuova, usata e chiusa. Non tocca le sessioni della chat,
        /// che vivono nel pool e hanno una memoria da conservare.
        /// </summary>
        public async IAsyncEnumerable<string> StreamChatAsync(
            string prompt,
            string modelId = null,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var directory = WorkingDirectory;
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new InvalidOperationException(
                    "opencode va eseguito dentro il progetto: WorkingDirectory non è stata impostata.");
            }

            await using var session = new OpenCodeSession(_logger, _server, directory, modelId);
            await foreach (var chunk in session.PromptAsync(prompt, ct).ConfigureAwait(false))
            {
                if (chunk.Kind == OpenCodeChunk.KindMessage) yield return chunk.Text;
            }
        }

        /// <summary>
        /// opencode i tool li usa <b>da sé</b> (li ha suoi, e legge il progetto in cui gira):
        /// non accetta un elenco di funzioni dall'esterno come fa l'API di un modello. Quindi
        /// qui i tool passati si ignorano — e lo si dice a log, invece di far credere che
        /// siano stati offerti al modello.
        /// </summary>
        public Task<string> ChatWithToolsAsync(
            string prompt,
            List<object> tools,
            Func<string, dynamic, Task<object>> toolExecutor,
            string modelId = null,
            string currentDocumentPath = null,
            List<object> conversationHistory = null,
            CancellationToken ct = default)
        {
            if (tools != null && tools.Count > 0)
            {
                _logger.LogInformation(
                    "[opencode] {Count} tool di MdExplorer ignorati: opencode usa i propri strumenti sul progetto",
                    tools.Count);
            }
            return ChatAsync(prompt, modelId, ct);
        }

        /// <summary>
        /// Il prompt di sistema di opencode sta nei suoi file (<c>AGENTS.md</c>, le sue
        /// istruzioni), non in una manopola di MdExplorer: scriverlo qui vorrebbe dire
        /// promettere un effetto che non c'è.
        /// </summary>
        public Task SetSystemPromptAsync(string systemPrompt)
        {
            throw new NotSupportedException(
                "opencode prende le istruzioni dai file del progetto (AGENTS.md e la cartella .opencode), " +
                "non da un prompt di sistema impostato da MdExplorer.");
        }

        public Task<string> GetSystemPromptAsync() => Task.FromResult<string>(null);

        public Task<string> GetApiKeyAsync() => Task.FromResult("not-required");

        public Task SaveApiKeyAsync(string apiKey) => Task.CompletedTask;

        public Task<bool> TestApiKeyAsync(string apiKey) => Task.FromResult(IsAvailable());
    }
}
