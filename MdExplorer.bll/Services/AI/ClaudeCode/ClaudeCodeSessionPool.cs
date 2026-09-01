using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI.ClaudeCode
{
    /// <summary>
    /// Tiene viva una <see cref="ClaudeCodeSession"/> per chiave di connessione (di norma la
    /// connectionId di SignalR), così i turni successivi riusano il processo già caldo e la
    /// conversazione mantiene la sua memoria.
    ///
    /// <para>Registrato come singleton DI. Gemello di <c>CopilotAcpSessionPool</c>, con la
    /// stessa politica (un gate per connessione, sweep degli inattivi, tetto di sessioni) —
    /// deliberatamente <b>separato</b> invece di generalizzare quello esistente: Copilot ACP
    /// non si tocca.</para>
    ///
    /// <para>Nota su Claude Code: qui il pool pesa meno che su Copilot. Là serviva a evitare i
    /// ~15 secondi di <c>session/new</c> a ogni turno; qui il primo turno costa meno di un
    /// secondo, quindi il pool serve soprattutto a <b>conservare la conversazione</b>, non a
    /// nascondere una latenza.</para>
    /// </summary>
    public sealed class ClaudeCodeSessionPool : IAsyncDisposable
    {
        private static readonly TimeSpan DefaultIdleTimeout = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(2);
        private const int DefaultMaxSessions = 16;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ClaudeCodeSessionPool> _logger;

        private readonly ConcurrentDictionary<string, ClaudeCodeSession> _sessions =
            new ConcurrentDictionary<string, ClaudeCodeSession>(StringComparer.Ordinal);
        // Serializza le GetOrCreateAsync concorrenti sulla stessa connectionId: senza, due
        // thread possono distruggere a vicenda la sessione appena avviata dall'altro.
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _gates =
            new ConcurrentDictionary<string, SemaphoreSlim>(StringComparer.Ordinal);
        // Release in volo, attesi dal DisposeAsync allo spegnimento.
        private readonly ConcurrentDictionary<Guid, Task> _pendingReleases =
            new ConcurrentDictionary<Guid, Task>();
        // Sorgente di annullamento del turno in streaming, per connessione: è ciò che il
        // pulsante Stop cancella.
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _activePrompts =
            new ConcurrentDictionary<string, CancellationTokenSource>(StringComparer.Ordinal);

        private readonly Timer _sweepTimer;
        private readonly TimeSpan _idleTimeout;
        private readonly int _maxSessions;
        private volatile bool _disposed;

        public ClaudeCodeSessionPool(ILoggerFactory loggerFactory, ILogger<ClaudeCodeSessionPool> logger)
        {
            _loggerFactory = loggerFactory;
            _logger = logger;
            _idleTimeout = DefaultIdleTimeout;
            _maxSessions = DefaultMaxSessions;
            _sweepTimer = new Timer(_ => SweepIdleSessions(), null, SweepInterval, SweepInterval);
        }

        /// <summary>
        /// Restituisce la sessione della connessione se combacia per working directory e
        /// modello, altrimenti la sostituisce con una nuova.
        /// </summary>
        public async Task<ClaudeCodeSession> GetOrCreateAsync(
            string connectionId,
            string workingDirectory,
            string modelId,
            ClaudeCodeSessionOptions options = null,
            CancellationToken ct = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ClaudeCodeSessionPool));
            if (string.IsNullOrEmpty(connectionId))
                throw new ArgumentException("connectionId obbligatoria", nameof(connectionId));

            if (_sessions.TryGetValue(connectionId, out var fast) && Matches(fast, workingDirectory, modelId))
            {
                return fast;
            }

            var gate = _gates.GetOrAdd(connectionId, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_sessions.TryGetValue(connectionId, out var existing))
                {
                    if (Matches(existing, workingDirectory, modelId)) return existing;
                    _logger.LogInformation("[ClaudeCodeSessionPool] Sostituisco la sessione di {ConnectionId}", connectionId);
                    await ReleaseAsync(connectionId).ConfigureAwait(false);
                }

                if (_sessions.Count >= _maxSessions) EvictOldest();

                var sessionLogger = _loggerFactory.CreateLogger<ClaudeCodeSession>();
                var session = new ClaudeCodeSession(sessionLogger, workingDirectory, modelId, options);
                try
                {
                    await session.StartAsync(ct).ConfigureAwait(false);
                }
                catch
                {
                    await session.DisposeAsync().ConfigureAwait(false);
                    throw;
                }

                if (!_sessions.TryAdd(connectionId, session))
                {
                    // Non dovrebbe accadere sotto il gate; difensivo.
                    await session.DisposeAsync().ConfigureAwait(false);
                    if (_sessions.TryGetValue(connectionId, out var winner)) return winner;
                    throw new InvalidOperationException("Registrazione della sessione fallita e nessun vincitore trovato");
                }

                _logger.LogInformation("[ClaudeCodeSessionPool] Creata sessione per {ConnectionId} (totale={Total})",
                    connectionId, _sessions.Count);
                return session;
            }
            finally
            {
                gate.Release();
            }
        }

        private static bool Matches(ClaudeCodeSession session, string workingDirectory, string modelId) =>
            session.IsAlive &&
            string.Equals(session.WorkingDirectory, workingDirectory, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(session.ModelId, modelId, StringComparison.Ordinal);

        /// <summary>Registra la sorgente di annullamento del turno in corso per questa connessione.</summary>
        public void RegisterActivePrompt(string connectionId, CancellationTokenSource cts)
        {
            if (string.IsNullOrEmpty(connectionId) || cts == null) return;
            if (_activePrompts.TryRemove(connectionId, out var old) && !ReferenceEquals(old, cts))
            {
                try { old.Cancel(); } catch { }
            }
            _activePrompts[connectionId] = cts;
        }

        /// <summary>
        /// Toglie la registrazione, ma solo se è ancora quella passata: così non si cancella
        /// mai per sbaglio il turno successivo.
        /// </summary>
        public void UnregisterActivePrompt(string connectionId, CancellationTokenSource cts)
        {
            if (string.IsNullOrEmpty(connectionId) || cts == null) return;
            if (_activePrompts.TryGetValue(connectionId, out var cur) && ReferenceEquals(cur, cts))
            {
                _activePrompts.TryRemove(connectionId, out _);
            }
        }

        /// <summary>
        /// Annulla il turno in volo (l'utente ha premuto Stop). Restituisce <c>true</c> solo se
        /// c'era davvero qualcosa in volo: dire "fatto" a vuoto spegnerebbe l'indicatore nella
        /// UI mentre il backend continua.
        /// </summary>
        public bool CancelActivePrompt(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId)) return false;
            if (_activePrompts.TryGetValue(connectionId, out var cts))
            {
                try { cts.Cancel(); } catch { }
                return true;
            }
            return false;
        }

        public async Task ReleaseAsync(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId)) return;
            if (_sessions.TryRemove(connectionId, out var session))
            {
                _logger.LogInformation("[ClaudeCodeSessionPool] Rilascio la sessione di {ConnectionId} (restanti={Remaining})",
                    connectionId, _sessions.Count);
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
            try
            {
                var oldest = _sessions.OrderBy(kv => kv.Value.LastUsedUtc).FirstOrDefault();
                if (oldest.Key != null)
                {
                    _logger.LogInformation("[ClaudeCodeSessionPool] Sfratto la sessione più vecchia {ConnectionId} (tetto={Cap})",
                        oldest.Key, _maxSessions);
                    TrackRelease(oldest.Key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ClaudeCodeSessionPool] Sfratto fallito");
            }
        }

        private void SweepIdleSessions()
        {
            if (_disposed) return;
            try
            {
                var cutoff = DateTime.UtcNow - _idleTimeout;
                List<string> toRelease = null;
                foreach (var kv in _sessions)
                {
                    if (!kv.Value.IsAlive || kv.Value.LastUsedUtc < cutoff)
                    {
                        toRelease ??= new List<string>();
                        toRelease.Add(kv.Key);
                    }
                }
                if (toRelease != null)
                {
                    foreach (var id in toRelease)
                    {
                        _logger.LogInformation("[ClaudeCodeSessionPool] Spazzo la sessione inattiva/morta {ConnectionId}", id);
                        TrackRelease(id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ClaudeCodeSessionPool] Sweep fallito");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;
            try { _sweepTimer?.Dispose(); } catch { }

            foreach (var key in _sessions.Keys.ToList())
            {
                await ReleaseAsync(key).ConfigureAwait(false);
            }

            try { await Task.WhenAll(_pendingReleases.Values).ConfigureAwait(false); } catch { }

            foreach (var kv in _gates)
            {
                try { kv.Value.Dispose(); } catch { }
            }
            _gates.Clear();
        }
    }
}
