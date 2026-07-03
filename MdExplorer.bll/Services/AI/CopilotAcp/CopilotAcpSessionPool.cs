using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI.CopilotAcp
{
    /// <summary>
    /// Maintains one long-lived <see cref="CopilotAcpSession"/> per connection key
    /// (typically the SignalR connection id) so that subsequent prompts on the same
    /// channel reuse the already-warm Copilot CLI process.
    ///
    /// Registered as a DI singleton. Disposes all sessions on host shutdown.
    /// </summary>
    public sealed class CopilotAcpSessionPool : IAsyncDisposable
    {
        private static readonly TimeSpan DefaultIdleTimeout = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(2);
        private const int DefaultMaxSessions = 16;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<CopilotAcpSessionPool> _logger;
        private readonly ConcurrentDictionary<string, Entry> _sessions =
            new ConcurrentDictionary<string, Entry>(StringComparer.Ordinal);
        // Per-connection serialization gate. Prevents two concurrent GetOrCreateAsync
        // calls for the same connectionId from disposing each other's freshly-started
        // sessions.
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _gates =
            new ConcurrentDictionary<string, SemaphoreSlim>(StringComparer.Ordinal);
        // In-flight Release tasks tracked so DisposeAsync awaits them on shutdown.
        private readonly ConcurrentDictionary<Guid, Task> _pendingReleases =
            new ConcurrentDictionary<Guid, Task>();
        // Cancellation source for the currently streaming prompt of each connection, so a
        // user "Stop" action can abort the in-flight prompt. One prompt per session.
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _activePrompts =
            new ConcurrentDictionary<string, CancellationTokenSource>(StringComparer.Ordinal);
        private readonly Timer _sweepTimer;
        private readonly TimeSpan _idleTimeout;
        private readonly int _maxSessions;

        private volatile bool _disposed;

        public CopilotAcpSessionPool(ILoggerFactory loggerFactory, ILogger<CopilotAcpSessionPool> logger)
        {
            _loggerFactory = loggerFactory;
            _logger = logger;
            _idleTimeout = DefaultIdleTimeout;
            _maxSessions = DefaultMaxSessions;
            _sweepTimer = new Timer(_ => SweepIdleSessions(), null, SweepInterval, SweepInterval);
        }

        /// <summary>
        /// Returns the existing session for <paramref name="connectionId"/> if it
        /// matches the requested <paramref name="workingDirectory"/> and
        /// <paramref name="modelId"/>, otherwise tears it down and starts a fresh one.
        /// </summary>
        public async Task<CopilotAcpSession> GetOrCreateAsync(
            string connectionId,
            string workingDirectory,
            string modelId,
            CancellationToken ct = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(CopilotAcpSessionPool));
            if (string.IsNullOrEmpty(connectionId)) throw new ArgumentException("connectionId required", nameof(connectionId));

            // Lock-free fast path: matching session already in cache.
            if (_sessions.TryGetValue(connectionId, out var existingFast) && existingFast.Session.IsAlive &&
                string.Equals(existingFast.Session.WorkingDirectory, workingDirectory, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(existingFast.Session.ModelId, modelId, StringComparison.Ordinal))
            {
                return existingFast.Session;
            }

            // Serialize same-connectionId concurrent calls so two threads cannot dispose
            // each other's freshly started sessions.
            var gate = _gates.GetOrAdd(connectionId, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                // Re-check under the gate.
                if (_sessions.TryGetValue(connectionId, out var existing))
                {
                    if (existing.Session.IsAlive &&
                        string.Equals(existing.Session.WorkingDirectory, workingDirectory, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(existing.Session.ModelId, modelId, StringComparison.Ordinal))
                    {
                        return existing.Session;
                    }
                    _logger.LogInformation("[CopilotAcpSessionPool] Replacing stale session for {ConnectionId}", connectionId);
                    await ReleaseAsync(connectionId).ConfigureAwait(false);
                }

                if (_sessions.Count >= _maxSessions)
                {
                    EvictOldest();
                }

                var sessionLogger = _loggerFactory.CreateLogger<CopilotAcpSession>();
                var session = new CopilotAcpSession(sessionLogger, workingDirectory, modelId);
                try
                {
                    await session.StartAsync(ct).ConfigureAwait(false);
                }
                catch
                {
                    await session.DisposeAsync().ConfigureAwait(false);
                    throw;
                }

                var entry = new Entry(session);
                if (!_sessions.TryAdd(connectionId, entry))
                {
                    // Should not happen under the gate; defensive.
                    await session.DisposeAsync().ConfigureAwait(false);
                    if (_sessions.TryGetValue(connectionId, out var winner)) return winner.Session;
                    throw new InvalidOperationException("Failed to register session and no winner found");
                }

                _logger.LogInformation("[CopilotAcpSessionPool] Created session for {ConnectionId} (total={Total})",
                    connectionId, _sessions.Count);
                return session;
            }
            finally
            {
                gate.Release();
            }
        }

        /// <summary>
        /// Registers the cancellation source of the prompt now streaming for
        /// <paramref name="connectionId"/> so a later Stop can abort it. Any stale source
        /// (should not happen — one prompt per session) is cancelled and replaced.
        /// </summary>
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
        /// Removes the active-prompt registration for <paramref name="connectionId"/>, but only
        /// if it is still <paramref name="cts"/> (so we never clear a newer prompt's source).
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
        /// Cancels the in-flight prompt for <paramref name="connectionId"/> (user pressed Stop).
        /// The streaming PromptAsync unwinds and tells the agent to stop (session/cancel).
        /// Returns true when a prompt was actually in flight.
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
            if (_sessions.TryRemove(connectionId, out var entry))
            {
                _logger.LogInformation("[CopilotAcpSessionPool] Releasing session for {ConnectionId} (remaining={Remaining})",
                    connectionId, _sessions.Count);
                await entry.Session.DisposeAsync().ConfigureAwait(false);
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
                var oldest = _sessions
                    .OrderBy(kv => kv.Value.Session.LastUsedUtc)
                    .FirstOrDefault();
                if (oldest.Key != null)
                {
                    _logger.LogInformation("[CopilotAcpSessionPool] Evicting oldest session {ConnectionId} (cap={Cap})",
                        oldest.Key, _maxSessions);
                    TrackRelease(oldest.Key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[CopilotAcpSessionPool] Evict failed");
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
                    var s = kv.Value.Session;
                    if (!s.IsAlive || s.LastUsedUtc < cutoff)
                    {
                        toRelease ??= new List<string>();
                        toRelease.Add(kv.Key);
                    }
                }
                if (toRelease != null)
                {
                    foreach (var id in toRelease)
                    {
                        _logger.LogInformation("[CopilotAcpSessionPool] Sweeping idle/dead session {ConnectionId}", id);
                        TrackRelease(id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[CopilotAcpSessionPool] Sweep failed");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;
            try { _sweepTimer?.Dispose(); } catch { }

            var keys = _sessions.Keys.ToList();
            foreach (var key in keys)
            {
                await ReleaseAsync(key).ConfigureAwait(false);
            }

            // Drain any sweep/evict releases that were started right before shutdown.
            try { await Task.WhenAll(_pendingReleases.Values).ConfigureAwait(false); } catch { }

            foreach (var kv in _gates)
            {
                try { kv.Value.Dispose(); } catch { }
            }
            _gates.Clear();
        }

        private sealed class Entry
        {
            public CopilotAcpSession Session { get; }
            public Entry(CopilotAcpSession session) { Session = session; }
        }
    }
}
