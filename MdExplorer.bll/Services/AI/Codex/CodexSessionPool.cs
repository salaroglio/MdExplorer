using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI.Codex
{
    /// <summary>
    /// Tiene viva una <see cref="CodexSession"/> per chiave (di norma la connectionId di SignalR), così i turni dopo
    /// riusano il processo e il thread. Stessa politica e stessa superficie di <c>ClaudeCodeSessionPool</c> (un gate per
    /// chiave, sweep degli inattivi, tetto di sessioni, Stop, ripresa dalla cronologia): l'hub li tratta allo stesso modo.
    /// <para>Sprint: docs-internal/Sprints/2026-11-28-Motore-Codex.md, fase F1.</para>
    /// </summary>
    public sealed class CodexSessionPool : IAsyncDisposable
    {
        private static readonly TimeSpan DefaultIdleTimeout = MdExplorer.Features.Agents.AgentTurnLimits.SessionIdle;
        private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(2);
        private const int DefaultMaxSessions = 16;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<CodexSessionPool> _logger;
        private readonly ConcurrentDictionary<string, CodexSession> _sessions = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _gates = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<Guid, Task> _pendingReleases = new();
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _activePrompts = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, string> _pendingResume = new(StringComparer.Ordinal);
        private readonly Timer _sweepTimer;
        private readonly MdExplorer.Features.Commands.PlantumlServer _plantumlServer;
        private volatile bool _disposed;

        public CodexSessionPool(ILoggerFactory loggerFactory, ILogger<CodexSessionPool> logger,
            MdExplorer.Features.Commands.PlantumlServer plantumlServer = null)
        {
            _loggerFactory = loggerFactory;
            _logger = logger;
            _plantumlServer = plantumlServer;
            _sweepTimer = new Timer(_ => SweepIdleSessions(), null, SweepInterval, SweepInterval);
        }

        /// <summary>La prossima sessione creata per <paramref name="connectionId"/> riprende il thread <paramref name="threadId"/>; null lo toglie.</summary>
        public void ResumeNextSession(string connectionId, string threadId)
        {
            if (string.IsNullOrEmpty(connectionId)) return;
            if (string.IsNullOrWhiteSpace(threadId)) _pendingResume.TryRemove(connectionId, out _);
            else _pendingResume[connectionId] = threadId;
        }

        /// <summary>
        /// La sessione della chiave se combacia per cartella e configurazione; se cambia solo il modello, lo cambia sulla
        /// sessione viva (vale dal turno dopo, la conversazione resta). Una configurazione diversa (profilo e2e, gruppi
        /// MCP, radici, web) riavvia il processo riprendendo lo stesso thread.
        /// </summary>
        public async Task<CodexSession> GetOrCreateAsync(string connectionId, string workingDirectory, string modelId,
            CodexSessionOptions options = null, CancellationToken ct = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(CodexSessionPool));
            if (string.IsNullOrEmpty(connectionId)) throw new ArgumentException(UiText.Say("connectionId obbligatoria", "connectionId is required"), nameof(connectionId));
            options ??= CodexSessionOptions.Default;

            var gate = _gates.GetOrAdd(connectionId, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_sessions.TryGetValue(connectionId, out var existing))
                {
                    var sameFolder = string.Equals(existing.WorkingDirectory, workingDirectory, StringComparison.OrdinalIgnoreCase);
                    if (existing.IsAlive && sameFolder && Fits(existing, options))
                    {
                        if (!string.IsNullOrWhiteSpace(modelId) && !string.Equals(existing.ModelId, modelId, StringComparison.Ordinal))
                        {
                            await existing.SetModelAsync(modelId, ct).ConfigureAwait(false);
                            _logger.LogInformation("[CodexSessionPool] {ConnectionId}: modello → {Model} sulla sessione viva", connectionId, modelId);
                        }
                        return existing;
                    }
                    if (existing.SessionId != null && sameFolder && options.ResumeThreadId == null)
                        options = options.WithResume(existing.SessionId);
                    _logger.LogInformation("[CodexSessionPool] {ConnectionId}: configurazione diversa o sessione morta, riavvio {Resume}",
                        connectionId, options.ResumeThreadId != null ? "riprendendo il thread" : "da capo");
                    await ReleaseAsync(connectionId).ConfigureAwait(false);
                }

                if (_sessions.Count >= DefaultMaxSessions) EvictOldest();

                if (options.ResumeThreadId == null && _pendingResume.TryRemove(connectionId, out var pending))
                {
                    options = options.WithResume(pending);
                    _logger.LogInformation("[CodexSessionPool] {ConnectionId}: riprendo il thread {Thread} della cronologia", connectionId, pending);
                }

                var session = new CodexSession(_loggerFactory.CreateLogger<CodexSession>(), workingDirectory, modelId, options);
                if (_plantumlServer != null) session.DiagramVerifier = new MdExplorer.Features.Diagrams.PlantumlBlockVerifier(_plantumlServer.CheckAsync);
                try
                {
                    await session.StartAsync(ct).ConfigureAwait(false);
                }
                catch
                {
                    await session.DisposeAsync().ConfigureAwait(false);
                    throw;
                }
                _sessions[connectionId] = session;
                _logger.LogInformation("[CodexSessionPool] Creata sessione per {ConnectionId} (totale={Total})", connectionId, _sessions.Count);
                return session;
            }
            finally
            {
                gate.Release();
            }
        }

        /// <summary>
        /// Una sessione partita per un lancio dei test e2e torna alla configurazione della chat, riprendendo lo stesso
        /// thread. Niente da fare se la sessione non ha un profilo di test.
        /// </summary>
        public async Task RestoreChatAsync(string connectionId, CodexSessionOptions chatOptions, CancellationToken ct = default)
        {
            if (!_sessions.TryGetValue(connectionId, out var current) || current.ProfileKey == null) return;
            await GetOrCreateAsync(connectionId, current.WorkingDirectory, current.ModelId, chatOptions, ct).ConfigureAwait(false);
        }

        private static bool Fits(CodexSession session, CodexSessionOptions options) =>
            string.Equals(session.ProfileKey, options.ProfileKey, StringComparison.Ordinal) &&
            (options.McpGroups == null || string.Equals(session.McpGroups ?? string.Empty, options.McpGroups, StringComparison.Ordinal)) &&
            (options.ExtraRootsKey == null || string.Equals(session.ExtraRootsKey, options.ExtraRootsKey, StringComparison.Ordinal)) &&
            session.WebSearch == options.WebSearch;

        public void RegisterActivePrompt(string connectionId, CancellationTokenSource cts)
        {
            if (string.IsNullOrEmpty(connectionId) || cts == null) return;
            if (_activePrompts.TryRemove(connectionId, out var old) && !ReferenceEquals(old, cts))
            {
                try { old.Cancel(); } catch { }
            }
            _activePrompts[connectionId] = cts;
        }

        public void UnregisterActivePrompt(string connectionId, CancellationTokenSource cts)
        {
            if (string.IsNullOrEmpty(connectionId) || cts == null) return;
            if (_activePrompts.TryGetValue(connectionId, out var cur) && ReferenceEquals(cur, cts))
                _activePrompts.TryRemove(connectionId, out _);
        }

        /// <summary>Annulla il turno in volo (Stop). <c>true</c> solo se c'era davvero qualcosa in volo.</summary>
        public bool CancelActivePrompt(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId)) return false;
            if (!_activePrompts.TryGetValue(connectionId, out var cts)) return false;
            try { cts.Cancel(); } catch { }
            return true;
        }

        public async Task ReleaseAsync(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId)) return;
            if (_sessions.TryRemove(connectionId, out var session))
            {
                _logger.LogInformation("[CodexSessionPool] Rilascio la sessione di {ConnectionId} (restanti={Remaining})", connectionId, _sessions.Count);
                await session.DisposeAsync().ConfigureAwait(false);
            }
        }

        private void TrackRelease(string connectionId)
        {
            var token = Guid.NewGuid();
            var task = ReleaseAsync(connectionId);
            _pendingReleases.TryAdd(token, task);
            _ = task.ContinueWith(_ => _pendingReleases.TryRemove(token, out _), TaskScheduler.Default);
        }

        private void EvictOldest()
        {
            var oldest = _sessions.OrderBy(kv => kv.Value.LastUsedUtc).FirstOrDefault();
            if (oldest.Key == null) return;
            _logger.LogInformation("[CodexSessionPool] Sfratto la sessione più vecchia {ConnectionId} (tetto={Cap})", oldest.Key, DefaultMaxSessions);
            TrackRelease(oldest.Key);
        }

        private void SweepIdleSessions()
        {
            if (_disposed) return;
            try
            {
                var cutoff = DateTime.UtcNow - DefaultIdleTimeout;
                foreach (var id in _sessions.Where(kv => !kv.Value.IsAlive || kv.Value.LastUsedUtc < cutoff).Select(kv => kv.Key).ToList())
                {
                    _logger.LogInformation("[CodexSessionPool] Spazzo la sessione inattiva/morta {ConnectionId}", id);
                    TrackRelease(id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[CodexSessionPool] Sweep fallito");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;
            try { _sweepTimer?.Dispose(); } catch { }
            foreach (var key in _sessions.Keys.ToList()) await ReleaseAsync(key).ConfigureAwait(false);
            try { await Task.WhenAll(_pendingReleases.Values).ConfigureAwait(false); } catch { }
            foreach (var gate in _gates.Values)
            {
                try { gate.Dispose(); } catch { }
            }
            _gates.Clear();
        }
    }
}
