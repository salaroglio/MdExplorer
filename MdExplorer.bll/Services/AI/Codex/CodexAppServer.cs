using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI.Codex
{
    /// <summary>
    /// Un processo <c>codex app-server</c> e il suo protocollo: JSON-RPC 2.0 <b>senza</b> il campo <c>jsonrpc</c>, una
    /// riga JSON per messaggio su stdin/stdout (misurato su Codex 0.172.0, 28/11/2026).
    /// <list type="bullet">
    /// <item><description>Richieste nostre: <c>{"id","method","params"}</c> → risposta <c>{"id","result"}</c> o <c>{"id","error"}</c>.</description></item>
    /// <item><description>Notifiche del server: <c>{"method","params"}</c>, senza <c>id</c> → <see cref="Notification"/>.</description></item>
    /// <item><description>Richieste del server (approvazioni): <c>{"id","method","params"}</c> → <see cref="ServerRequest"/>, che
    ///   DEVE rispondere: un turno che aspetta un'approvazione senza risposta resta appeso per sempre.</description></item>
    /// </list>
    /// <para>
    /// Prima di tutto <see cref="StartAsync"/> fa l'handshake: <c>initialize</c> (con <c>clientInfo</c> e
    /// <c>experimentalApi</c>) e poi la notifica <c>initialized</c>; prima, il server risponde «Not initialized».
    /// </para>
    /// <para>Sprint: docs-internal/Sprints/2026-11-28-Motore-Codex.md, fase F1.</para>
    /// </summary>
    public sealed class CodexAppServer : IAsyncDisposable
    {
        private const int STDERR_RING_SIZE = 20;
        private static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(60);

        private readonly ILogger _logger;
        private readonly string _workingDirectory;
        private readonly IReadOnlyDictionary<string, string> _environment;
        private Process _process;
        private Task _readerTask;
        private Task _stderrTask;
        private long _nextId;
        private readonly ConcurrentDictionary<long, TaskCompletionSource<JsonElement>> _pending = new();
        private readonly SemaphoreSlim _writeLock = new(1, 1);
        private readonly ConcurrentQueue<string> _stderrRing = new();
        private volatile bool _exited;
        private volatile bool _disposed;

        /// <summary>Una notifica del server: metodo e <c>params</c> (già clonati, si possono tenere).</summary>
        public event Action<string, JsonElement> Notification;

        /// <summary>
        /// Una richiesta del server (approvazione di un comando o di una modifica, input dell'utente, elicitation MCP):
        /// metodo e <c>params</c>; restituisce il <c>result</c> da rimandare. Senza gestore, o se il gestore fallisce, la
        /// richiesta riceve un errore JSON-RPC: mai il silenzio.
        /// </summary>
        public Func<string, JsonElement, Task<object>> ServerRequest { get; set; }

        /// <summary>Il processo è terminato (con il motivo, stderr compreso).</summary>
        public event Action<string> Exited;

        /// <summary>La cartella di Codex dichiarata dall'<c>initialize</c> (<c>codexHome</c>).</summary>
        public string CodexHome { get; private set; }

        /// <summary>L'user agent dell'<c>initialize</c>: contiene la versione del CLI (<c>mdexplorer/0.172.0 …</c>).</summary>
        public string UserAgent { get; private set; }

        public bool IsAlive => _process != null && !_exited && !_disposed;

        public CodexAppServer(ILogger logger, string workingDirectory, IReadOnlyDictionary<string, string> environment = null)
        {
            _logger = logger;
            _workingDirectory = workingDirectory;
            _environment = environment;
        }

        /// <summary>Avvia <c>codex app-server</c> e fa l'handshake.</summary>
        public async Task StartAsync(CancellationToken ct = default)
        {
            if (_process != null) throw new InvalidOperationException(UiText.Say("Server Codex già avviato", "Codex server already started"));

            var psi = CodexProcessLauncher.BuildStartInfo("app-server");
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.StandardOutputEncoding = Encoding.UTF8;
            psi.StandardErrorEncoding = Encoding.UTF8;
            // JSON su stdin: niente BOM, accenti intatti anche su Windows.
            psi.StandardInputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            if (_environment != null)
                foreach (var (key, value) in _environment) psi.Environment[key] = value;

            if (!string.IsNullOrEmpty(_workingDirectory))
            {
                if (!Directory.Exists(_workingDirectory))
                    throw new InvalidOperationException(UiText.Say(
                        $"Cartella di lavoro inesistente: '{_workingDirectory}'. Codex va lanciato dentro il progetto.",
                        $"Working directory does not exist: '{_workingDirectory}'. Codex must run inside the project."));
                psi.WorkingDirectory = _workingDirectory;
            }

            _process = new Process { StartInfo = psi, EnableRaisingEvents = true };
            _process.Exited += OnProcessExited;
            if (!_process.Start())
                throw new InvalidOperationException(UiText.Say("Avvio di `codex app-server` fallito", "Starting `codex app-server` failed"));
            _logger.LogInformation("[CodexAppServer] Avviato pid={Pid} cwd={Cwd}", _process.Id, _workingDirectory);

            _readerTask = Task.Run(ReaderLoopAsync);
            _stderrTask = Task.Run(async () =>
            {
                try
                {
                    string line;
                    while ((line = await _process.StandardError.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        _stderrRing.Enqueue(line);
                        while (_stderrRing.Count > STDERR_RING_SIZE) _stderrRing.TryDequeue(out _);
                        _logger.LogDebug("[CodexAppServer][stderr] {Line}", line);
                    }
                }
                catch { }
            });

            var init = await RequestAsync("initialize", new
            {
                clientInfo = new { name = "mdexplorer", title = "MdExplorer", version = typeof(CodexAppServer).Assembly.GetName().Version?.ToString() ?? "0" },
                capabilities = new { experimentalApi = true },
            }, ct).ConfigureAwait(false);
            CodexHome = init.TryGetProperty("codexHome", out var home) ? home.GetString() : null;
            UserAgent = init.TryGetProperty("userAgent", out var ua) ? ua.GetString() : null;
            await NotifyAsync("initialized", new { }, ct).ConfigureAwait(false);
            _logger.LogInformation("[CodexAppServer] initialize ok: {UserAgent} codexHome={CodexHome}", UserAgent, CodexHome);
        }

        /// <summary>
        /// Una richiesta e la sua risposta. Un <c>error</c> JSON-RPC diventa <see cref="CodexRpcException"/> con il
        /// messaggio del server; il processo morto, un'eccezione con le ultime righe di stderr.
        /// </summary>
        public async Task<JsonElement> RequestAsync(string method, object parameters, CancellationToken ct = default, TimeSpan? timeout = null)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(CodexAppServer));
            var id = Interlocked.Increment(ref _nextId);
            var reply = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pending[id] = reply;
            try
            {
                await WriteAsync(new JsonObject
                {
                    ["id"] = id,
                    ["method"] = method,
                    ["params"] = ToNode(parameters),
                }, ct).ConfigureAwait(false);

                using var limit = CancellationTokenSource.CreateLinkedTokenSource(ct);
                limit.CancelAfter(timeout ?? DefaultRequestTimeout);
                try
                {
                    return await reply.Task.WaitAsync(limit.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                {
                    throw new TimeoutException(UiText.Say(
                        $"Codex non ha risposto a `{method}` entro {(timeout ?? DefaultRequestTimeout).TotalSeconds:0} secondi.",
                        $"Codex did not answer `{method}` within {(timeout ?? DefaultRequestTimeout).TotalSeconds:0} seconds.") + DescribeStderr());
                }
            }
            finally
            {
                _pending.TryRemove(id, out _);
            }
        }

        /// <summary>Una notifica nostra (senza risposta), per esempio <c>initialized</c>.</summary>
        public Task NotifyAsync(string method, object parameters, CancellationToken ct = default)
            => WriteAsync(new JsonObject { ["method"] = method, ["params"] = ToNode(parameters) }, ct);

        private static JsonNode ToNode(object value) => value switch
        {
            null => new JsonObject(),
            JsonNode node => node,
            JsonElement element => JsonNode.Parse(element.GetRawText()),
            _ => JsonSerializer.SerializeToNode(value),
        };

        private async Task WriteAsync(JsonObject message, CancellationToken ct)
        {
            var json = message.ToJsonString();
            await _writeLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_exited || _process?.StandardInput == null)
                    throw new InvalidOperationException(UiText.Say("Il processo Codex non è in esecuzione.", "The Codex process is not running.") + DescribeStderr());
                await _process.StandardInput.WriteLineAsync(json.AsMemory(), ct).ConfigureAwait(false);
                await _process.StandardInput.FlushAsync().ConfigureAwait(false);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        private async Task ReaderLoopAsync()
        {
            try
            {
                string line;
                while ((line = await _process.StandardOutput.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    JsonDocument doc;
                    try { doc = JsonDocument.Parse(line); }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[CodexAppServer] Riga di stdout non JSON: {Line}", Truncate(line, 200));
                        continue;
                    }
                    using (doc)
                    {
                        try { Dispatch(doc.RootElement); }
                        catch (Exception ex) { _logger.LogError(ex, "[CodexAppServer] Errore nel dispatch di {Line}", Truncate(line, 300)); }
                    }
                }
            }
            catch (Exception ex) when (!_disposed)
            {
                _logger.LogError(ex, "[CodexAppServer] Lettura di stdout interrotta");
            }
            finally
            {
                FailPending(new InvalidOperationException(UiText.Say("Codex ha chiuso stdout.", "Codex closed stdout.") + DescribeStderr()));
            }
        }

        private void Dispatch(JsonElement root)
        {
            var hasMethod = root.TryGetProperty("method", out var methodEl) && methodEl.ValueKind == JsonValueKind.String;
            var hasId = root.TryGetProperty("id", out var idEl) && idEl.ValueKind is JsonValueKind.Number or JsonValueKind.String;

            if (!hasMethod && hasId)
            {
                // Risposta a una nostra richiesta: gli id nostri sono sempre numeri.
                if (idEl.ValueKind != JsonValueKind.Number || !_pending.TryRemove(idEl.GetInt64(), out var reply)) return;
                if (root.TryGetProperty("error", out var error))
                    reply.TrySetException(new CodexRpcException(
                        error.TryGetProperty("code", out var code) && code.ValueKind == JsonValueKind.Number ? code.GetInt32() : 0,
                        error.TryGetProperty("message", out var msg) ? msg.GetString() : error.GetRawText()));
                else
                    reply.TrySetResult(root.TryGetProperty("result", out var result) ? result.Clone() : default);
                return;
            }
            if (!hasMethod) return;

            var method = methodEl.GetString();
            var parameters = root.TryGetProperty("params", out var p) ? p.Clone() : default;
            if (hasId)
            {
                _ = AnswerServerRequestAsync(idEl.Clone(), method, parameters);
                return;
            }
            try { Notification?.Invoke(method, parameters); }
            catch (Exception ex) { _logger.LogError(ex, "[CodexAppServer] Gestore della notifica {Method} fallito", method); }
        }

        private async Task AnswerServerRequestAsync(JsonElement id, string method, JsonElement parameters)
        {
            var handler = ServerRequest;
            JsonObject answer;
            try
            {
                if (handler == null) throw new InvalidOperationException(UiText.Say("MdExplorer non gestisce questa richiesta di Codex.", "MdExplorer does not handle this Codex request."));
                var result = await handler(method, parameters).ConfigureAwait(false);
                answer = new JsonObject { ["id"] = JsonNode.Parse(id.GetRawText()), ["result"] = ToNode(result) };
                _logger.LogInformation("[CodexAppServer] Richiesta {Method} → {Answer}", method, Truncate(answer["result"]?.ToJsonString(), 200));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[CodexAppServer] Richiesta {Method} non gestita: rispondo con un errore", method);
                answer = new JsonObject
                {
                    ["id"] = JsonNode.Parse(id.GetRawText()),
                    ["error"] = new JsonObject { ["code"] = -32603, ["message"] = ex.Message },
                };
            }
            try { await WriteAsync(answer, CancellationToken.None).ConfigureAwait(false); }
            catch (Exception ex) { _logger.LogWarning(ex, "[CodexAppServer] Risposta a {Method} non inviata", method); }
        }

        private void FailPending(Exception ex)
        {
            foreach (var key in _pending.Keys)
                if (_pending.TryRemove(key, out var reply)) reply.TrySetException(ex);
        }

        /// <summary>Le ultime righe di stderr, da allegare a un errore («Not logged in», versione, …).</summary>
        public string DescribeStderr()
        {
            var lines = _stderrRing.ToArray();
            return lines.Length == 0 ? string.Empty : UiText.Say(" Ultime righe di stderr: ", " Last stderr lines: ") + string.Join(" | ", lines);
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            _exited = true;
            int? exitCode = null;
            try { exitCode = _process?.ExitCode; } catch { }
            var reason = UiText.Say($"Il processo Codex è terminato (exit code {exitCode?.ToString() ?? "?"}).",
                $"The Codex process ended (exit code {exitCode?.ToString() ?? "?"}).") + DescribeStderr();
            if (!_disposed) _logger.LogInformation("[CodexAppServer] {Reason}", reason);
            FailPending(new InvalidOperationException(reason));
            try { Exited?.Invoke(reason); } catch { }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;
            try
            {
                if (_process != null && !_process.HasExited)
                {
                    // Chiudere stdin è il congedo pulito: il server esce da solo.
                    try { _process.StandardInput?.Close(); } catch { }
                    using var grace = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                    try { await _process.WaitForExitAsync(grace.Token).ConfigureAwait(false); }
                    catch { try { _process.Kill(entireProcessTree: true); } catch { } }
                }
            }
            catch { }
            try { if (_readerTask != null) await _readerTask.ConfigureAwait(false); } catch { }
            try { if (_stderrTask != null) await _stderrTask.ConfigureAwait(false); } catch { }
            try { _process?.Dispose(); } catch { }
            FailPending(new ObjectDisposedException(nameof(CodexAppServer)));
            _writeLock.Dispose();
        }

        private static string Truncate(string s, int max) =>
            string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "…";
    }

    /// <summary>Un <c>error</c> JSON-RPC di Codex, con il codice e il messaggio del server.</summary>
    public sealed class CodexRpcException : Exception
    {
        public int Code { get; }
        public CodexRpcException(int code, string message) : base(UiText.Say($"Codex ha risposto con un errore ({code}): {message}", $"Codex answered with an error ({code}): {message}"))
            => Code = code;
    }
}
