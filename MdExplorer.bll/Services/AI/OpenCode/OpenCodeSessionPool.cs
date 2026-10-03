using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI.OpenCode
{
    /// <summary>
    /// Tiene viva una <see cref="OpenCodeSession"/> per connessione SignalR, così i turni
    /// successivi restano nella stessa conversazione.
    /// <para>
    /// Gemello di <c>ClaudeCodeSessionPool</c> con una differenza che viene dal protocollo: qui
    /// <b>cambiare modello non tocca la sessione</b> — il modello è un campo del messaggio, non
    /// una proprietà del processo. Quindi il pool sostituisce la sessione solo quando cambia il
    /// <b>progetto</b>.
    /// </para>
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Motore-OpenCode.md, fase F4.</para>
    /// </summary>
    public sealed class OpenCodeSessionPool : IAsyncDisposable
    {
        private static readonly TimeSpan DefaultIdleTimeout = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(2);
        private const int DefaultMaxSessions = 16;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<OpenCodeSessionPool> _logger;
        private readonly OpenCodeServer _server;

        private readonly ConcurrentDictionary<string, OpenCodeSession> _sessions = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _gates = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<Guid, Task> _pendingReleases = new();
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _activePrompts = new(StringComparer.Ordinal);

        private readonly Timer _sweepTimer;
        private volatile bool _disposed;

        public OpenCodeSessionPool(ILoggerFactory loggerFactory, ILogger<OpenCodeSessionPool> logger, OpenCodeServer server)
        {
            _loggerFactory = loggerFactory;
            _logger = logger;
            _server = server;
            _sweepTimer = new Timer(_ => SweepIdleSessions(), null, SweepInterval, SweepInterval);
        }

        /// <summary>
        /// La sessione di questa connessione, creandola se serve. Stesso progetto = stessa
        /// sessione, anche se il modello cambia: il nuovo modello vale dal messaggio successivo
        /// e la conversazione resta (misurato).
        /// </summary>
        public async Task<OpenCodeSession> GetOrCreateAsync(
            string connectionId,
            string workingDirectory,
            string model,
            CancellationToken ct = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(OpenCodeSessionPool));
            if (string.IsNullOrEmpty(connectionId))
                throw new ArgumentException("connectionId obbligatoria", nameof(connectionId));

            if (_sessions.TryGetValue(connectionId, out var fast) && SameProject(fast, workingDirectory))
            {
                if (fast.Model != model) fast.SetModel(model);
                return fast;
            }

            var gate = _gates.GetOrAdd(connectionId, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_sessions.TryGetValue(connectionId, out var existing))
                {
                    if (SameProject(existing, workingDirectory))
                    {
                        if (existing.Model != model)
                        {
                            existing.SetModel(model);
                            _logger.LogInformation(
                                "[OpenCodeSessionPool] Modello cambiato sulla sessione viva di {ConnectionId}: conversazione conservata",
                                connectionId);
                        }
                        return existing;
                    }
                    _logger.LogInformation("[OpenCodeSessionPool] Cambio progetto: sostituisco la sessione di {ConnectionId}", connectionId);
                    await ReleaseAsync(connectionId).ConfigureAwait(false);
                }

                if (_sessions.Count >= DefaultMaxSessions) EvictOldest();

                var session = new OpenCodeSession(
                    _loggerFactory.CreateLogger<OpenCodeSession>(), _server, workingDirectory, model);

                if (!_sessions.TryAdd(connectionId, session))
                {
                    await session.DisposeAsync().ConfigureAwait(false);
                    if (_sessions.TryGetValue(connectionId, out var winner)) return winner;
                    throw new InvalidOperationException("Registrazione della sessione opencode fallita e nessun vincitore trovato");
                }

                _logger.LogInformation("[OpenCodeSessionPool] Creata sessione per {ConnectionId} (totale={Total})",
                    connectionId, _sessions.Count);
                return session;
            }
            finally
            {
                gate.Release();
            }
        }

        private static bool SameProject(OpenCodeSession session, string workingDirectory) =>
            session.IsAlive &&
            string.Equals(session.WorkingDirectory, System.IO.Path.GetFullPath(workingDirectory),
                StringComparison.OrdinalIgnoreCase);

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

        /// <summary>Toglie la registrazione, ma solo se è ancora quella passata.</summary>
        public void UnregisterActivePrompt(string connectionId, CancellationTokenSource cts)
        {
            if (string.IsNullOrEmpty(connectionId) || cts == null) return;
            if (_activePrompts.TryGetValue(connectionId, out var cur) && ReferenceEquals(cur, cts))
            {
                _activePrompts.TryRemove(connectionId, out _);
            }
        }

        /// <summary>
        /// Stop dell'utente. <c>true</c> solo se c'era davvero un turno in volo: dire «fatto» a
        /// vuoto spegnerebbe l'indicatore nella UI mentre il server continua a generare.
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
                _logger.LogInformation("[OpenCodeSessionPool] Rilascio la sessione di {ConnectionId} (restanti={Remaining})",
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
                    _logger.LogInformation("[OpenCodeSessionPool] Sfratto la sessione più vecchia {ConnectionId} (tetto={Cap})",
                        oldest.Key, DefaultMaxSessions);
                    TrackRelease(oldest.Key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[OpenCodeSessionPool] Sfratto fallito");
            }
        }

        private void SweepIdleSessions()
        {
            if (_disposed) return;
            try
            {
                var cutoff = DateTime.UtcNow - DefaultIdleTimeout;
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
                        _logger.LogInformation("[OpenCodeSessionPool] Spazzo la sessione inattiva {ConnectionId}", id);
                        TrackRelease(id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[OpenCodeSessionPool] Sweep fallito");
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
