using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI.CopilotAcp
{
    /// <summary>
    /// A single text fragment emitted during a prompt stream.
    /// <see cref="Kind"/> distinguishes the agent's internal reasoning ("thinking")
    /// from user-visible response text ("message") so the UI can render them in
    /// the appropriate channel.
    /// </summary>
    public readonly struct CopilotAcpChunk
    {
        public string Kind { get; }
        public string Text { get; }
        public CopilotAcpChunk(string kind, string text) { Kind = kind; Text = text; }
        public const string KindMessage = "message";
        public const string KindThinking = "thinking";
    }

    /// <summary>
    /// Wraps a single long-running `copilot --acp` process and exposes a JSON-RPC
    /// client that supports session creation and streaming prompts.
    ///
    /// Wire format observed against Copilot CLI 1.0.48:
    ///   - NDJSON over stdin/stdout (one JSON object per line)
    ///   - JSON-RPC 2.0
    ///   - The agent emits `session/update` notifications BEFORE the request response,
    ///     so the reader must continue draining until the response with the matching id arrives.
    ///   - Streaming text comes through `session/update` with `update.sessionUpdate = "agent_message_chunk"`.
    /// </summary>
    public sealed class CopilotAcpSession : IAsyncDisposable
    {
        private const int PROTOCOL_VERSION = 1;
        private const int INITIALIZE_TIMEOUT_MS = 30000;
        private const int SESSION_NEW_TIMEOUT_MS = 60000;
        // Per-prompt timeouts: the idle deadline resets on every session/update so
        // a long but continuously-streaming agentic turn never trips it. The hard
        // cap is the absolute ceiling regardless of activity.
        private const int PROMPT_IDLE_TIMEOUT_MS = 300000;   // 5 min of silence
        private const int PROMPT_HARD_TIMEOUT_MS = 1800000;  // 30 min absolute

        private readonly ILogger _logger;
        private readonly string _workingDirectory;
        private readonly string _modelId;

        private Process _process;
        private Task _readerTask;
        private Task _stderrTask;
        private CancellationTokenSource _readerCts;
        private string _sessionId;
        private int _nextRequestId;

        // Request/response correlation
        private readonly ConcurrentDictionary<int, TaskCompletionSource<JsonDocument>> _pending =
            new ConcurrentDictionary<int, TaskCompletionSource<JsonDocument>>();

        // Active prompt streaming channel (one prompt at a time per session)
        private Channel<CopilotAcpChunk> _activeStreamChannel;
        // Idle-timeout CTS of the active prompt; reset on every session/update.
        private CancellationTokenSource _activePromptIdleCts;
        private readonly SemaphoreSlim _promptGate = new SemaphoreSlim(1, 1);

        // Stdin write lock (writer is single-threaded, but multiple awaits race)
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);

        private volatile bool _disposed;
        private volatile bool _processExited;

        public string SessionId => _sessionId;
        public string WorkingDirectory => _workingDirectory;
        public string ModelId => _modelId;
        public bool IsAlive => _process != null && !_processExited && !_disposed;
        public DateTime LastUsedUtc { get; private set; } = DateTime.UtcNow;

        public CopilotAcpSession(ILogger logger, string workingDirectory, string modelId)
        {
            _logger = logger;
            _workingDirectory = workingDirectory;
            _modelId = modelId;
        }

        /// <summary>
        /// Spawns the copilot process, performs the JSON-RPC initialize handshake,
        /// and creates a session bound to the configured working directory.
        /// </summary>
        public async Task StartAsync(CancellationToken ct = default)
        {
            if (_process != null) throw new InvalidOperationException("Session already started");

            var psi = CopilotProcessLauncher.BuildStartInfo("--acp --allow-all-tools --no-color --log-level error");
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.StandardOutputEncoding = Encoding.UTF8;
            psi.StandardErrorEncoding = Encoding.UTF8;

            if (!string.IsNullOrEmpty(_workingDirectory) && Directory.Exists(_workingDirectory))
            {
                psi.WorkingDirectory = _workingDirectory;
            }

            _process = new Process { StartInfo = psi };
            _process.EnableRaisingEvents = true;
            _process.Exited += OnProcessExited;

            if (!_process.Start())
            {
                throw new InvalidOperationException("Failed to start copilot --acp process");
            }

            _logger.LogInformation("[CopilotAcpSession] Spawned pid={Pid} cwd={Cwd}", _process.Id, _workingDirectory);

            _readerCts = new CancellationTokenSource();
            _readerTask = Task.Run(() => ReaderLoopAsync(_readerCts.Token));

            // Drain stderr to logs so we never block on a full pipe.
            _stderrTask = Task.Run(async () =>
            {
                try
                {
                    string line;
                    while ((line = await _process.StandardError.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        _logger.LogWarning("[CopilotAcpSession][stderr] {Line}", line);
                    }
                }
                catch { }
            });

            try
            {
                await InitializeAsync(ct).ConfigureAwait(false);
                await NewSessionAsync(ct).ConfigureAwait(false);
            }
            catch
            {
                await DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }

        private async Task InitializeAsync(CancellationToken ct)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(INITIALIZE_TIMEOUT_MS);
            var id = NextId();
            var req = new
            {
                jsonrpc = "2.0",
                id,
                method = "initialize",
                @params = new
                {
                    protocolVersion = PROTOCOL_VERSION,
                    clientCapabilities = new
                    {
                        fs = new { readTextFile = false, writeTextFile = false }
                    }
                }
            };
            using var doc = await SendRequestAsync(id, req, timeoutCts.Token).ConfigureAwait(false);
            var result = doc.RootElement.GetProperty("result");
            if (result.TryGetProperty("protocolVersion", out var pvEl) && pvEl.GetInt32() != PROTOCOL_VERSION)
            {
                throw new InvalidOperationException(
                    $"ACP protocol version mismatch: agent reported {pvEl.GetInt32()}, expected {PROTOCOL_VERSION}");
            }
            _logger.LogInformation("[CopilotAcpSession] initialize OK");
        }

        private async Task NewSessionAsync(CancellationToken ct)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(SESSION_NEW_TIMEOUT_MS);
            var id = NextId();
            var req = new
            {
                jsonrpc = "2.0",
                id,
                method = "session/new",
                @params = new
                {
                    cwd = _workingDirectory ?? Environment.CurrentDirectory,
                    mcpServers = Array.Empty<object>()
                }
            };
            using var doc = await SendRequestAsync(id, req, timeoutCts.Token).ConfigureAwait(false);
            var result = doc.RootElement.GetProperty("result");
            _sessionId = result.GetProperty("sessionId").GetString();
            _logger.LogInformation("[CopilotAcpSession] session/new OK sessionId={SessionId}", _sessionId);

            // Best-effort: select model via the in-session /model slash command.
            // The Copilot CLI exposes "model <id>" as an available command; sending it
            // through the same session/prompt channel does NOT trigger a model response,
            // it just switches the selected model. If the command isn't available we
            // ignore the failure and continue with the agent's default selection.
            if (!string.IsNullOrEmpty(_modelId) && _modelId != "auto")
            {
                try { await SelectModelAsync(_modelId, ct).ConfigureAwait(false); }
                catch (Exception ex) { _logger.LogWarning(ex, "[CopilotAcpSession] model selection failed; using default"); }
            }
        }

        private async Task SelectModelAsync(string modelId, CancellationToken ct)
        {
            // The "/model <id>" slash command is recognized inside session/prompt.
            // We send it as a normal prompt and just consume the response.
            var promptText = $"/model {modelId}";
            await foreach (var _ in PromptAsync(promptText, ct).ConfigureAwait(false))
            {
                // discard
            }
        }

        /// <summary>
        /// Sends a user prompt and yields streaming chunks tagged as either
        /// <see cref="CopilotAcpChunk.KindMessage"/> (visible response) or
        /// <see cref="CopilotAcpChunk.KindThinking"/> (internal reasoning).
        /// Only one prompt may be in flight at a time per session.
        /// </summary>
        public async IAsyncEnumerable<CopilotAcpChunk> PromptAsync(
            string text,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(CopilotAcpSession));
            if (string.IsNullOrEmpty(_sessionId)) throw new InvalidOperationException("Session not initialized");

            await _promptGate.WaitAsync(ct).ConfigureAwait(false);
            LastUsedUtc = DateTime.UtcNow;

            var channel = Channel.CreateUnbounded<CopilotAcpChunk>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true,
                AllowSynchronousContinuations = false
            });
            _activeStreamChannel = channel;

            // Idle timeout resets on every session/update (see DispatchMessage);
            // hard cap is the absolute ceiling regardless of activity.
            var idleCts = new CancellationTokenSource(PROMPT_IDLE_TIMEOUT_MS);
            var hardCts = new CancellationTokenSource(PROMPT_HARD_TIMEOUT_MS);
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, idleCts.Token, hardCts.Token);
            _activePromptIdleCts = idleCts;

            var requestId = NextId();
            var req = new
            {
                jsonrpc = "2.0",
                id = requestId,
                method = "session/prompt",
                @params = new
                {
                    sessionId = _sessionId,
                    prompt = new object[] { new { type = "text", text } }
                }
            };

            // Fire the request and let the reader feed the channel until the request
            // resolves (or fails) on a background task; we yield as text arrives.
            var responseTask = SendRequestAsync(requestId, req, linkedCts.Token);

            // When the response arrives (or errors), close the channel.
            _ = responseTask.ContinueWith(t =>
            {
                try
                {
                    if (t.IsFaulted) channel.Writer.TryComplete(t.Exception?.GetBaseException());
                    else if (t.IsCanceled)
                    {
                        string reason;
                        if (idleCts.IsCancellationRequested)
                            reason = $"Copilot ACP idle timeout: no activity for {PROMPT_IDLE_TIMEOUT_MS / 1000}s";
                        else if (hardCts.IsCancellationRequested)
                            reason = $"Copilot ACP hard timeout: exceeded {PROMPT_HARD_TIMEOUT_MS / 60000} minutes";
                        else
                            reason = "Prompt cancelled";
                        channel.Writer.TryComplete(new OperationCanceledException(reason));
                    }
                    else channel.Writer.TryComplete();
                }
                finally
                {
                    Interlocked.CompareExchange(ref _activeStreamChannel, null, channel);
                }
                // Only access .Result when the task ran to completion; on faulted/canceled
                // .Result rethrows and becomes an unobserved task exception.
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    // [ACP-PROBE] TEMPORARY: dump the raw session/prompt result to discover
                    // whether Copilot CLI ACP exposes any usage / credits / token / stopReason
                    // data we could surface as a consumption indicator. Remove after the probe.
                    try
                    {
                        _logger.LogWarning("[ACP-PROBE] session/prompt result: {Json}",
                            t.Result?.RootElement.GetRawText());
                    }
                    catch { }
                    t.Result?.Dispose();
                }
            }, TaskScheduler.Default);

            try
            {
                await foreach (var chunk in channel.Reader.ReadAllAsync(ct).ConfigureAwait(false))
                {
                    yield return chunk;
                }
            }
            finally
            {
                Interlocked.CompareExchange(ref _activePromptIdleCts, null, idleCts);
                Interlocked.CompareExchange(ref _activeStreamChannel, null, channel);

                var timeoutFired = idleCts.IsCancellationRequested || hardCts.IsCancellationRequested;
                var externalCancel = ct.IsCancellationRequested;
                if (timeoutFired || externalCancel)
                {
                    // Always tell the agent to stop on any cancellation: otherwise
                    // it keeps producing content we silently drop AND its session
                    // memory diverges from what the user actually received. We
                    // briefly wait for responseTask so the next prompt isn't queued
                    // before this one fully unwinds inside the agent.
                    try { await CancelAsync(CancellationToken.None).ConfigureAwait(false); } catch { }
                    try
                    {
                        using var graceCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        await responseTask.WaitAsync(graceCts.Token).ConfigureAwait(false);
                    }
                    catch { /* timed out — release anyway */ }
                }

                try { idleCts.Dispose(); } catch { }
                try { hardCts.Dispose(); } catch { }
                try { linkedCts.Dispose(); } catch { }
                _promptGate.Release();
            }
        }

        /// <summary>
        /// Sends session/cancel; the reader loop should observe the prompt response
        /// shortly after with stopReason=cancelled.
        /// </summary>
        public async Task CancelAsync(CancellationToken ct = default)
        {
            if (_disposed || _processExited || string.IsNullOrEmpty(_sessionId)) return;
            try
            {
                var notification = new
                {
                    jsonrpc = "2.0",
                    method = "session/cancel",
                    @params = new { sessionId = _sessionId }
                };
                await SendRawAsync(notification, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[CopilotAcpSession] cancel failed");
            }
        }

        private int NextId() => Interlocked.Increment(ref _nextRequestId);

        private async Task<JsonDocument> SendRequestAsync(int id, object request, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<JsonDocument>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (!_pending.TryAdd(id, tcs))
            {
                throw new InvalidOperationException($"Duplicate request id {id}");
            }

            try
            {
                await SendRawAsync(request, ct).ConfigureAwait(false);

                using var registration = ct.Register(() =>
                {
                    if (_pending.TryRemove(id, out var pendingTcs))
                    {
                        pendingTcs.TrySetCanceled();
                    }
                });

                return await tcs.Task.ConfigureAwait(false);
            }
            catch
            {
                _pending.TryRemove(id, out _);
                throw;
            }
        }

        private async Task SendRawAsync(object message, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(message);
            await _writeLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_processExited || _process?.StandardInput == null)
                {
                    throw new InvalidOperationException("ACP process is not running");
                }
                await _process.StandardInput.WriteLineAsync(json.AsMemory(), ct).ConfigureAwait(false);
                await _process.StandardInput.FlushAsync().ConfigureAwait(false);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        private async Task ReaderLoopAsync(CancellationToken ct)
        {
            try
            {
                string line;
                while (!ct.IsCancellationRequested &&
                       (line = await _process.StandardOutput.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    JsonDocument doc;
                    try { doc = JsonDocument.Parse(line); }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[CopilotAcpSession] Failed to parse stdout line: {Line}", Truncate(line, 200));
                        continue;
                    }
                    DispatchMessage(doc);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CopilotAcpSession] Reader loop crashed");
            }
            finally
            {
                _logger.LogInformation("[CopilotAcpSession] Reader loop exiting");
                FailAllPending(new InvalidOperationException("ACP process stdout closed"));
                _activeStreamChannel?.Writer.TryComplete(new InvalidOperationException("ACP process stdout closed"));
            }
        }

        private void DispatchMessage(JsonDocument doc)
        {
            // A message is either:
            //  - response to a request → has "id" + ("result" or "error")
            //  - notification           → has "method" + no "id"
            //  - request from server    → has "method" + "id" (we do not currently handle these)
            var root = doc.RootElement;
            if (root.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.Number)
            {
                if (root.TryGetProperty("method", out _))
                {
                    // Server-initiated request. Not handled; we should reply with method-not-found
                    // but ignoring is safer than misbehaving. Log for visibility.
                    _logger.LogDebug("[CopilotAcpSession] Ignoring server-initiated request: {Method}",
                        root.GetProperty("method").GetString());
                    doc.Dispose();
                    return;
                }

                var id = idEl.GetInt32();
                if (_pending.TryRemove(id, out var tcs))
                {
                    if (root.TryGetProperty("error", out var errEl))
                    {
                        var msg = errEl.TryGetProperty("message", out var m) ? m.GetString() : "unknown JSON-RPC error";
                        tcs.TrySetException(new InvalidOperationException($"ACP error: {msg}"));
                        doc.Dispose();
                    }
                    else
                    {
                        // Hand the doc off to the awaiter; awaiter is responsible for Dispose.
                        tcs.TrySetResult(doc);
                    }
                }
                else
                {
                    _logger.LogDebug("[CopilotAcpSession] Response for unknown id {Id}", id);
                    doc.Dispose();
                }
                return;
            }

            // Notification
            if (root.TryGetProperty("method", out var methodEl))
            {
                var method = methodEl.GetString();
                if (method == "session/update")
                {
                    // Agent is alive — push the idle deadline forward.
                    try { _activePromptIdleCts?.CancelAfter(PROMPT_IDLE_TIMEOUT_MS); } catch { }
                    HandleSessionUpdate(root);
                }
                doc.Dispose();
                return;
            }

            _logger.LogDebug("[CopilotAcpSession] Unrecognized message shape");
            doc.Dispose();
        }

        private void HandleSessionUpdate(JsonElement root)
        {
            if (!root.TryGetProperty("params", out var paramsEl)) return;
            if (!paramsEl.TryGetProperty("update", out var updateEl)) return;
            if (!updateEl.TryGetProperty("sessionUpdate", out var kindEl)) return;
            var kind = kindEl.GetString();

            string chunkKind = kind switch
            {
                "agent_message_chunk" => CopilotAcpChunk.KindMessage,
                "agent_thought_chunk" => CopilotAcpChunk.KindThinking,
                _ => null
            };
            if (chunkKind == null)
            {
                // [ACP-PROBE] TEMPORARY: dump non-text update payloads (tool_call, plan, and any
                // hypothetical usage/token/credits update) to see if consumption data arrives
                // out-of-band from Copilot CLI ACP. Remove after the probe.
                try { _logger.LogWarning("[ACP-PROBE] session/update kind={Kind}: {Json}", kind, updateEl.GetRawText()); } catch { }
                return;
            }

            if (!updateEl.TryGetProperty("content", out var contentEl)) return;
            if (!contentEl.TryGetProperty("type", out var typeEl) || typeEl.GetString() != "text") return;
            if (!contentEl.TryGetProperty("text", out var textEl)) return;
            var text = textEl.GetString();
            if (string.IsNullOrEmpty(text)) return;

            var channel = _activeStreamChannel;
            channel?.Writer.TryWrite(new CopilotAcpChunk(chunkKind, text));
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            _processExited = true;
            _logger.LogInformation("[CopilotAcpSession] Process exited (ExitCode={ExitCode})",
                _process?.ExitCode);
            FailAllPending(new InvalidOperationException("ACP process exited"));
            _activeStreamChannel?.Writer.TryComplete(new InvalidOperationException("ACP process exited"));
        }

        private void FailAllPending(Exception ex)
        {
            foreach (var kv in _pending)
            {
                if (_pending.TryRemove(kv.Key, out var tcs))
                {
                    tcs.TrySetException(ex);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            try { _readerCts?.Cancel(); } catch { }

            try
            {
                if (_process != null && !_process.HasExited)
                {
                    try { _process.StandardInput?.Close(); } catch { }
                    try { _process.Kill(entireProcessTree: true); } catch { }
                    try { await _process.WaitForExitAsync().ConfigureAwait(false); } catch { }
                }
            }
            catch { }

            try { if (_readerTask != null) await _readerTask.ConfigureAwait(false); } catch { }
            try { if (_stderrTask != null) await _stderrTask.ConfigureAwait(false); } catch { }

            try { _process?.Dispose(); } catch { }
            try { _readerCts?.Dispose(); } catch { }
            _promptGate.Dispose();
            _writeLock.Dispose();

            FailAllPending(new ObjectDisposedException(nameof(CopilotAcpSession)));
            _activeStreamChannel?.Writer.TryComplete();
        }

        private static string Truncate(string s, int max) => s.Length <= max ? s : s.Substring(0, max) + "…";

    }
}
