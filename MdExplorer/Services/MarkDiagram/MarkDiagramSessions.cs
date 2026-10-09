using MdExplorer.Features.Services.AI.ClaudeCode;
using MdExplorer.Features.Services.AI.CopilotChat;
using MdExplorer.Features.Services.AI.OpenCode;
using MdExplorer.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Services.MarkDiagram
{
    /// <summary>
    /// The sessions of «spiega il diagramma» (sprint 2026-09-29-Motore-LLM-Unico, D8): one conversation per
    /// document, with the project's engine and model, apart from the MarkAgent tab (it would take the tab, one
    /// question at a time, and fill its context).
    /// <list type="bullet">
    /// <item><description><b>Only the last one is alive</b> per connection: every open document would otherwise
    /// be a CLI process. The others are closed, and their conversation is <b>resumed</b> from its id when the
    /// user comes back to that document.</description></item>
    /// <item><description><b>Read-only</b>: changes go through the proposal the user confirms, which MdExplorer
    /// applies; the session itself can neither write files nor run commands.</description></item>
    /// </list>
    /// </summary>
    public sealed class MarkDiagramSessions : IAsyncDisposable
    {
        /// <summary>Why the session may not write nor run anything, as the refusal tells the model.</summary>
        public const string ReadOnlyReason =
            "la sessione dei diagrammi è in sola lettura: le modifiche si propongono nel blocco mde-edit e le applica MdExplorer quando l'utente conferma";

        private const string ProfileKey = "mark-diagram";

        private readonly ClaudeCodeSessionPool _claude;
        private readonly CopilotChatSessionPool _copilot;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<MarkDiagramSessions> _logger;

        private readonly ConcurrentDictionary<string, State> _states = new();

        private sealed class State
        {
            public readonly SemaphoreSlim Gate = new(1, 1);

            /// <summary>The document and engine of the live session; null when none is alive.</summary>
            public string LiveDocument;
            public MarkAgentEngine LiveEngine;

            /// <summary>opencode has no pool for dedicated sessions: its server and session live here.</summary>
            public OpenCodeServer OpenCodeServer;
            public OpenCodeSession OpenCodeSession;

            /// <summary>The conversation of each document, per engine: a Claude session cannot be resumed by Copilot.</summary>
            public readonly ConcurrentDictionary<string, string> Remembered = new(StringComparer.Ordinal);
        }

        public MarkDiagramSessions(ClaudeCodeSessionPool claude, CopilotChatSessionPool copilot,
            ILoggerFactory loggerFactory, ILogger<MarkDiagramSessions> logger)
        {
            _claude = claude;
            _copilot = copilot;
            _loggerFactory = loggerFactory;
            _logger = logger;
        }

        /// <summary>The key of the live session in the pools: one per connection, whatever the document.</summary>
        private static string PoolKey(string connectionId) => connectionId + "|diagram";

        private static string Remember(string document, MarkAgentEngine engine) => document + "|" + MarkAgentEngines.IdOf(engine);

        /// <summary>
        /// Asks <paramref name="prompt"/> in the conversation of <paramref name="document"/>: the live session if it
        /// is that document's, otherwise the live one is closed and the document's conversation is resumed (or
        /// started). Yields the text of the answer.
        /// </summary>
        public async IAsyncEnumerable<string> AskAsync(string connectionId, string projectPath, string document,
            MarkAgentEngine engine, string model, string prompt, [EnumeratorCancellation] CancellationToken ct)
        {
            if (engine is not (MarkAgentEngine.Claude or MarkAgentEngine.Copilot or MarkAgentEngine.OpenCode))
                throw new InvalidOperationException("Il progetto non ha un motore di MarkAgent: sceglilo nelle impostazioni del progetto.");

            var state = _states.GetOrAdd(connectionId, _ => new State());
            await state.Gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (state.LiveDocument != null && (state.LiveDocument != document || state.LiveEngine != engine))
                    await CloseLiveAsync(connectionId, state).ConfigureAwait(false);

                state.Remembered.TryGetValue(Remember(document, engine), out var resume);
                if (state.LiveDocument == null)
                    _logger.LogInformation("[MarkDiagramSessions] {Connection}: sessione per {Document} con {Engine} ({Mode})",
                        connectionId, document, MarkAgentEngines.IdOf(engine), resume != null ? "ripresa" : "nuova");
                state.LiveDocument = document;
                state.LiveEngine = engine;

                switch (engine)
                {
                    case MarkAgentEngine.Claude:
                    {
                        var options = LlmSessions.ReadOnlyEngineSessions.ClaudeOptions(ProfileKey, resume);
                        var session = await _claude.GetOrCreateAsync(PoolKey(connectionId), projectPath, model, options, ct).ConfigureAwait(false);
                        await foreach (var chunk in session.PromptAsync(prompt, ct).ConfigureAwait(false))
                            if (chunk.Kind == ClaudeCodeChunk.KindMessage) yield return chunk.Text;
                        if (session.SessionId != null) state.Remembered[Remember(document, engine)] = session.SessionId;
                        break;
                    }
                    case MarkAgentEngine.Copilot:
                    {
                        var profile = LlmSessions.ReadOnlyEngineSessions.CopilotProfile(ProfileKey, ReadOnlyReason, resume);
                        var session = await _copilot.GetOrCreateAsync(PoolKey(connectionId), projectPath, model, profile, ct).ConfigureAwait(false);
                        await foreach (var chunk in session.PromptAsync(prompt, ct).ConfigureAwait(false))
                            if (chunk.Kind == CopilotChatChunk.KindMessage) yield return chunk.Text;
                        if (session.SessionId != null) state.Remembered[Remember(document, engine)] = session.SessionId;
                        break;
                    }
                    case MarkAgentEngine.OpenCode:
                    {
                        if (state.OpenCodeSession == null)
                        {
                            // A server of its own: opencode's permissions are fixed per server, and the shared one has the
                            // project's (edit allowed). Conversations live in opencode's database, shared by every
                            // server: the document's one is resumed by its id.
                            state.OpenCodeServer = new OpenCodeServer(_loggerFactory.CreateLogger<OpenCodeServer>(),
                                new Dictionary<string, string> { ["OPENCODE_CONFIG_CONTENT"] = LlmSessions.ReadOnlyEngineSessions.OpenCodeConfig() });
                            state.OpenCodeSession = new OpenCodeSession(_loggerFactory.CreateLogger<OpenCodeSession>(),
                                state.OpenCodeServer, projectPath, model, resume, rejectPermissions: true);
                        }
                        await foreach (var chunk in state.OpenCodeSession.PromptAsync(prompt, ct).ConfigureAwait(false))
                            if (chunk.Kind == OpenCodeChunk.KindMessage) yield return chunk.Text;
                        if (state.OpenCodeSession.SessionId != null) state.Remembered[Remember(document, engine)] = state.OpenCodeSession.SessionId;
                        break;
                    }
                }
            }
            finally
            {
                state.Gate.Release();
            }
        }

        /// <summary>
        /// «Nuova conversazione» on <paramref name="document"/>: its conversation is forgotten, and the live session is
        /// closed if it is that document's. The next question starts from scratch.
        /// </summary>
        public async Task NewConversationAsync(string connectionId, string document)
        {
            if (!_states.TryGetValue(connectionId, out var state)) return;
            await state.Gate.WaitAsync().ConfigureAwait(false);
            try
            {
                foreach (var engine in new[] { MarkAgentEngine.Claude, MarkAgentEngine.Copilot, MarkAgentEngine.OpenCode })
                    state.Remembered.TryRemove(Remember(document, engine), out _);
                if (state.LiveDocument == document) await CloseLiveAsync(connectionId, state).ConfigureAwait(false);
                _logger.LogInformation("[MarkDiagramSessions] {Connection}: nuova conversazione su {Document}", connectionId, document);
            }
            finally
            {
                state.Gate.Release();
            }
        }

        /// <summary>The connection is gone (the app was closed or reloaded): its sessions go with it.</summary>
        public async Task ForgetConnectionAsync(string connectionId)
        {
            if (!_states.TryRemove(connectionId, out var state)) return;
            await state.Gate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (state.LiveDocument != null) await CloseLiveAsync(connectionId, state).ConfigureAwait(false);
            }
            finally
            {
                state.Gate.Release();
            }
        }

        private async Task CloseLiveAsync(string connectionId, State state)
        {
            switch (state.LiveEngine)
            {
                case MarkAgentEngine.Claude:
                    await _claude.ReleaseAsync(PoolKey(connectionId)).ConfigureAwait(false);
                    break;
                case MarkAgentEngine.Copilot:
                    await _copilot.ReleaseAsync(PoolKey(connectionId)).ConfigureAwait(false);
                    break;
                case MarkAgentEngine.OpenCode:
                    if (state.OpenCodeSession != null) await state.OpenCodeSession.DisposeAsync().ConfigureAwait(false);
                    state.OpenCodeServer?.Dispose();
                    state.OpenCodeSession = null;
                    state.OpenCodeServer = null;
                    break;
            }
            state.LiveDocument = null;
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var connectionId in _states.Keys)
                await ForgetConnectionAsync(connectionId).ConfigureAwait(false);
        }
    }
}
