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

namespace MdExplorer.Features.Services.AI.ClaudeCode
{
    /// <summary>
    /// Avvolge un singolo processo <c>claude -p</c> di lunga durata e ne parla il protocollo
    /// <b>nativo</b>: NDJSON tipizzato su stdin/stdout, un oggetto JSON per riga, discriminato
    /// dal campo <c>type</c>.
    ///
    /// <para><b>Perché non ACP.</b> Claude Code non parla ACP: non esiste alcun flag
    /// <c>--acp</c> (verificato su 2.1.250). Farglielo parlare richiederebbe l'adapter di terze
    /// parti di Zed, cioè un processo Node in più che <b>appiattisce</b> su ACP proprio le cose
    /// che qui contano — consumi, tool, ripresa della sessione. Questa classe è quindi la
    /// gemella di <c>CopilotAcpSession</c>, non una sua estensione: <b>stessa forma</b>
    /// (processo persistente, reader loop, <see cref="Channel{T}"/>, un prompt per volta),
    /// <b>protocollo diverso</b>. Copilot ACP non viene toccato.</para>
    ///
    /// <para><b>Forma del filo, osservata su claude 2.1.250:</b>
    /// <list type="bullet">
    /// <item><description>Verso il CLI: <c>{"type":"user","message":{"role":"user","content":[{"type":"text","text":…}]}}</c></description></item>
    /// <item><description>Dal CLI, in ordine: <c>system/init</c> → <c>system/status</c> →
    ///   <c>stream_event</c> (delta) → <c>assistant</c> → <c>result</c>. Il turno finisce
    ///   <b>sul <c>result</c></b>: non ci sono id di richiesta da correlare come in JSON-RPC.</description></item>
    /// <item><description>⚠️ <b>Il CLI resta muto finché non riceve il primo messaggio</b>
    ///   (verificato: 8 s di silenzio dopo lo spawn, zero byte). Non esiste quindi un handshake:
    ///   <see cref="StartAsync"/> lancia il processo e basta, e la prima prova che il CLI
    ///   funziona davvero arriva col primo turno.</description></item>
    /// </list></para>
    ///
    /// <para><b>Permessi.</b> In <c>-p</c> non arriva nessun <c>can_use_tool</c>: MDE non può
    /// fare da guardia in tempo reale. La superficie si dichiara al lancio
    /// (<see cref="ClaudeCodeSessionOptions.ToolPolicy"/>) e si <b>verifica</b> sull'<c>init</c>:
    /// se il CLI riporta un <c>permissionMode</c> diverso da quello chiesto, il turno fallisce
    /// forte. Non è pedanteria: <c>--permission-mode manual</c> viene ignorato <b>in silenzio</b>
    /// (l'init risponde <c>default</c> e i comandi partono lo stesso).</para>
    /// </summary>
    public sealed class ClaudeCodeSession : IAsyncDisposable
    {
        /// <summary>Modalità permessi richiesta. <c>dontAsk</c> è onorata (verificato).</summary>
        private const string REQUESTED_PERMISSION_MODE = "dontAsk";

        // Timeout per turno: la scadenza di inattività si sposta in avanti a ogni messaggio
        // ricevuto, così un turno lungo ma vivo non viene mai ucciso; il tetto duro è il
        // limite assoluto. Stessi valori del lato Copilot, per non avere due comportamenti
        // diversi a parità di attesa dell'utente.
        private const int PROMPT_IDLE_TIMEOUT_MS = 300000;   // 5 minuti di silenzio
        private const int PROMPT_HARD_TIMEOUT_MS = 1800000;  // 30 minuti in assoluto
        private const int STDERR_RING_SIZE = 20;

        private readonly ILogger _logger;
        private readonly string _workingDirectory;
        private readonly string _modelId;
        private readonly ClaudeCodeSessionOptions _options;

        private Process _process;
        private Task _readerTask;
        private Task _stderrTask;
        private CancellationTokenSource _readerCts;
        private int _nextControlId;

        // Turno attivo (uno per volta, come sul lato Copilot).
        private Channel<ClaudeCodeChunk> _activeStreamChannel;
        private TaskCompletionSource<ClaudeCodeTurnUsage> _activeTurnCompletion;
        private CancellationTokenSource _activePromptIdleCts;
        private volatile bool _interruptRequested;
        private readonly SemaphoreSlim _promptGate = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);

        // Ultime righe di stderr: servono a dare un errore leggibile quando il processo muore
        // subito (CLI non autenticato, versione incompatibile, ...) invece di un generico
        // "stdout chiuso".
        private readonly ConcurrentQueue<string> _stderrRing = new ConcurrentQueue<string>();

        private volatile bool _disposed;
        private volatile bool _processExited;

        /// <summary>Id di sessione riportato dal CLI, disponibile dopo il primo turno.</summary>
        public string SessionId { get; private set; }
        public string WorkingDirectory => _workingDirectory;
        public string ModelId => _modelId;
        /// <summary>Modello realmente in uso, come riportato dall'ultimo <c>init</c>.</summary>
        public string EffectiveModel { get; private set; }
        /// <summary>Versione del CLI, dall'ultimo <c>init</c>.</summary>
        public string CliVersion { get; private set; }
        /// <summary>Consuntivo dell'ultimo turno concluso (costo, token, durata).</summary>
        public ClaudeCodeTurnUsage LastTurnUsage { get; private set; }
        /// <summary>Ultimo stato noto delle finestre di consumo dell'abbonamento.</summary>
        public ClaudeCodeRateLimit LastRateLimit { get; private set; }
        public bool IsAlive => _process != null && !_processExited && !_disposed;
        public DateTime LastUsedUtc { get; private set; } = DateTime.UtcNow;

        public ClaudeCodeSession(
            ILogger logger,
            string workingDirectory,
            string modelId,
            ClaudeCodeSessionOptions options = null)
        {
            _logger = logger;
            _workingDirectory = workingDirectory;
            _modelId = modelId;
            _options = options ?? ClaudeCodeSessionOptions.Default;
        }

        /// <summary>
        /// Costruisce la riga di comando. Pubblico e statico perché è la parte che si vuole
        /// poter leggere in un test o in un log senza lanciare niente.
        /// </summary>
        public static string BuildArguments(string modelId, ClaudeCodeSessionOptions options)
        {
            options ??= ClaudeCodeSessionOptions.Default;
            var sb = new StringBuilder();

            // Sessione bidirezionale in NDJSON. --verbose è obbligatorio: senza, in print
            // mode lo stream è ridotto e i delta non arrivano.
            sb.Append("-p --input-format stream-json --output-format stream-json --verbose");
            // Delta token-per-token: senza questo si vedono solo i messaggi interi a fine blocco.
            sb.Append(" --include-partial-messages");
            // Eco del nostro messaggio: ACK a costo zero, dice che stdin è arrivato a destinazione.
            sb.Append(" --replay-user-messages");
            sb.Append(" --permission-mode ").Append(REQUESTED_PERMISSION_MODE);

            if (!string.IsNullOrWhiteSpace(modelId) && modelId != "auto")
            {
                sb.Append(" --model ").Append(Quote(modelId));
            }

            if (options.ToolPolicy == ClaudeCodeToolPolicy.NoExecution)
            {
                // Divieto mirato, MAI `--tools ""`: con zero tool il modello non rifiuta,
                // si inventa la chiamata e finge l'output (verificato).
                sb.Append(" --disallowedTools Bash");
            }

            if (options.Bare)
            {
                sb.Append(" --bare");
            }

            if (!string.IsNullOrWhiteSpace(options.McpConfigPath))
            {
                // --strict-mcp-config è indissociabile: senza, il CLI somma i server MCP
                // personali dell'utente ai nostri.
                sb.Append(" --mcp-config ").Append(Quote(options.McpConfigPath));
                sb.Append(" --strict-mcp-config");
            }

            if (!string.IsNullOrWhiteSpace(options.ResumeSessionId))
            {
                sb.Append(" --resume ").Append(Quote(options.ResumeSessionId));
            }

            return sb.ToString();
        }

        private static string Quote(string value) =>
            value.IndexOf(' ') >= 0 ? "\"" + value + "\"" : value;

        /// <summary>
        /// Lancia il processo e avvia i loop di lettura. <b>Non</b> fa handshake: il CLI non
        /// emette nulla finché non riceve un messaggio. Un fallimento di autenticazione o di
        /// configurazione emerge quindi al primo <see cref="PromptAsync"/>, con lo stderr del
        /// processo allegato all'errore.
        /// </summary>
        public Task StartAsync(CancellationToken ct = default)
        {
            if (_process != null) throw new InvalidOperationException("Sessione già avviata");

            // `--bare` spegne anche OAuth e portachiavi: senza API key nell'ambiente il CLI
            // risponderebbe «Not logged in» al primo turno, cioè con un errore che non dice
            // nulla sulla vera causa. Meglio rifiutare qui, dove la causa è ancora visibile.
            if (_options.Bare &&
                string.IsNullOrEmpty(Environment.GetEnvironmentVariable(ClaudeCodeSessionOptions.ApiKeyEnvVariable)))
            {
                throw new InvalidOperationException(
                    $"Opzione Bare attiva ma la variabile {ClaudeCodeSessionOptions.ApiKeyEnvVariable} non è impostata. " +
                    "Con --bare Claude Code non legge né OAuth né portachiavi: l'accesso con l'abbonamento " +
                    "non funziona. Imposta una API key oppure lascia Bare = false.");
            }

            var arguments = BuildArguments(_modelId, _options);
            var psi = ClaudeCodeProcessLauncher.BuildStartInfo(arguments);
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
            else if (!string.IsNullOrEmpty(_workingDirectory))
            {
                throw new InvalidOperationException(
                    $"Working directory inesistente: '{_workingDirectory}'. " +
                    "Claude Code va lanciato dentro il progetto, non altrove.");
            }

            _process = new Process { StartInfo = psi };
            _process.EnableRaisingEvents = true;
            _process.Exited += OnProcessExited;

            if (!_process.Start())
            {
                throw new InvalidOperationException($"Avvio di `claude {arguments}` fallito");
            }

            _logger.LogInformation("[ClaudeCodeSession] Avviato pid={Pid} cwd={Cwd} args={Args}",
                _process.Id, _workingDirectory, arguments);

            _readerCts = new CancellationTokenSource();
            _readerTask = Task.Run(() => ReaderLoopAsync(_readerCts.Token));

            // stderr va drenato di continuo, altrimenti una pipe piena blocca il processo.
            _stderrTask = Task.Run(async () =>
            {
                try
                {
                    string line;
                    while ((line = await _process.StandardError.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        _stderrRing.Enqueue(line);
                        while (_stderrRing.Count > STDERR_RING_SIZE) _stderrRing.TryDequeue(out _);
                        _logger.LogWarning("[ClaudeCodeSession][stderr] {Line}", line);
                    }
                }
                catch { }
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Manda un messaggio utente e restituisce i frammenti man mano che arrivano.
        /// Un solo turno per volta per sessione. Il turno si chiude sul <c>result</c>.
        /// </summary>
        public async IAsyncEnumerable<ClaudeCodeChunk> PromptAsync(
            string text,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ClaudeCodeSession));
            if (_process == null) throw new InvalidOperationException("Sessione non avviata");

            await _promptGate.WaitAsync(ct).ConfigureAwait(false);
            LastUsedUtc = DateTime.UtcNow;

            var channel = Channel.CreateUnbounded<ClaudeCodeChunk>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true,
                AllowSynchronousContinuations = false
            });
            var turnCompletion = new TaskCompletionSource<ClaudeCodeTurnUsage>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            _activeStreamChannel = channel;
            _activeTurnCompletion = turnCompletion;
            _interruptRequested = false;

            var idleCts = new CancellationTokenSource(PROMPT_IDLE_TIMEOUT_MS);
            var hardCts = new CancellationTokenSource(PROMPT_HARD_TIMEOUT_MS);
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, idleCts.Token, hardCts.Token);
            _activePromptIdleCts = idleCts;

            // Il turno finisce quando arriva il result: chiudiamo il canale allora, oppure
            // quando scade un timeout / il processo muore.
            var turnTask = WaitTurnAsync(turnCompletion.Task, linkedCts.Token);
            _ = turnTask.ContinueWith(t =>
            {
                try
                {
                    if (t.IsFaulted) channel.Writer.TryComplete(t.Exception?.GetBaseException());
                    else if (t.IsCanceled)
                    {
                        string reason;
                        if (idleCts.IsCancellationRequested)
                            reason = $"Claude Code fermo da {PROMPT_IDLE_TIMEOUT_MS / 1000}s: turno interrotto";
                        else if (hardCts.IsCancellationRequested)
                            reason = $"Claude Code oltre il tetto di {PROMPT_HARD_TIMEOUT_MS / 60000} minuti: turno interrotto";
                        else
                            reason = "Turno annullato";
                        channel.Writer.TryComplete(new OperationCanceledException(reason));
                    }
                    else channel.Writer.TryComplete();
                }
                finally
                {
                    Interlocked.CompareExchange(ref _activeStreamChannel, null, channel);
                }
            }, TaskScheduler.Default);

            try
            {
                await SendUserMessageAsync(text, linkedCts.Token).ConfigureAwait(false);
            }
            catch
            {
                channel.Writer.TryComplete();
                Interlocked.CompareExchange(ref _activeStreamChannel, null, channel);
                Interlocked.CompareExchange(ref _activeTurnCompletion, null, turnCompletion);
                Interlocked.CompareExchange(ref _activePromptIdleCts, null, idleCts);
                idleCts.Dispose(); hardCts.Dispose(); linkedCts.Dispose();
                _promptGate.Release();
                throw;
            }

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
                Interlocked.CompareExchange(ref _activeTurnCompletion, null, turnCompletion);

                var timeoutFired = idleCts.IsCancellationRequested || hardCts.IsCancellationRequested;
                if (timeoutFired || ct.IsCancellationRequested)
                {
                    // Su qualsiasi annullamento va detto anche all'agente: altrimenti continua a
                    // produrre roba che noi buttiamo, e la SUA memoria di sessione diverge da
                    // quello che l'utente ha davvero visto.
                    try { await CancelAsync(CancellationToken.None).ConfigureAwait(false); } catch { }
                    try
                    {
                        using var graceCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        await turnCompletion.Task.WaitAsync(graceCts.Token).ConfigureAwait(false);
                    }
                    catch { /* scaduto: si rilascia comunque */ }
                }

                try { idleCts.Dispose(); } catch { }
                try { hardCts.Dispose(); } catch { }
                try { linkedCts.Dispose(); } catch { }
                _promptGate.Release();
            }
        }

        private static async Task<ClaudeCodeTurnUsage> WaitTurnAsync(
            Task<ClaudeCodeTurnUsage> turn, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var registration = ct.Register(() => tcs.TrySetResult(true));
            var completed = await Task.WhenAny(turn, tcs.Task).ConfigureAwait(false);
            if (completed == turn) return await turn.ConfigureAwait(false);
            throw new OperationCanceledException(ct);
        }

        /// <summary>
        /// Chiede al CLI di fermare il turno in corso, con il control protocol nativo.
        /// Il CLI risponde con un <c>control_response</c> e chiude il turno; ⚠️ lo chiude
        /// riportando <c>error_during_execution</c>, che <b>non</b> è un errore vero — è
        /// l'interruzione che abbiamo chiesto noi, e come tale viene marcata.
        /// </summary>
        public async Task CancelAsync(CancellationToken ct = default)
        {
            if (_disposed || _processExited) return;
            _interruptRequested = true;
            try
            {
                var requestId = "mde-interrupt-" + Interlocked.Increment(ref _nextControlId);
                await SendRawAsync(new
                {
                    type = "control_request",
                    request_id = requestId,
                    request = new { subtype = "interrupt" }
                }, ct).ConfigureAwait(false);
                _logger.LogInformation("[ClaudeCodeSession] interrupt inviato (request_id={RequestId})", requestId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ClaudeCodeSession] invio dell'interrupt fallito");
            }
        }

        private Task SendUserMessageAsync(string text, CancellationToken ct) =>
            SendRawAsync(new
            {
                type = "user",
                message = new
                {
                    role = "user",
                    content = new object[] { new { type = "text", text } }
                }
            }, ct);

        private async Task SendRawAsync(object message, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(message);
            await _writeLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_processExited || _process?.StandardInput == null)
                {
                    throw new InvalidOperationException(
                        "Il processo Claude Code non è in esecuzione." + DescribeStderr());
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
                    using JsonDocument doc = SafeParse(line);
                    if (doc == null) continue;
                    // Qualunque messaggio è un segno di vita: sposta in avanti la scadenza.
                    try { _activePromptIdleCts?.CancelAfter(PROMPT_IDLE_TIMEOUT_MS); } catch { }
                    try { Dispatch(doc.RootElement); }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[ClaudeCodeSession] Errore nel dispatch di una riga NDJSON");
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ClaudeCodeSession] Reader loop terminato con errore");
            }
            finally
            {
                _logger.LogInformation("[ClaudeCodeSession] Reader loop chiuso");
                var ex = new InvalidOperationException(
                    "Claude Code ha chiuso stdout senza concludere il turno." + DescribeStderr());
                _activeTurnCompletion?.TrySetException(ex);
                _activeStreamChannel?.Writer.TryComplete(ex);
            }
        }

        private JsonDocument SafeParse(string line)
        {
            try { return JsonDocument.Parse(line); }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ClaudeCodeSession] Riga di stdout non JSON: {Line}", Truncate(line, 200));
                return null;
            }
        }

        private void Dispatch(JsonElement root)
        {
            if (!root.TryGetProperty("type", out var typeEl) || typeEl.ValueKind != JsonValueKind.String) return;

            switch (typeEl.GetString())
            {
                case "system":
                    HandleSystem(root);
                    break;
                case "stream_event":
                    HandleStreamEvent(root);
                    break;
                case "assistant":
                    HandleAssistant(root);
                    break;
                case "user":
                    HandleUser(root);
                    break;
                case "rate_limit_event":
                    HandleRateLimit(root);
                    break;
                case "result":
                    HandleResult(root);
                    break;
                case "control_response":
                    _logger.LogDebug("[ClaudeCodeSession] control_response: {Raw}", Truncate(root.GetRawText(), 300));
                    break;
                case "control_request":
                    // Il CLI in `-p` non chiede approvazioni (verificato: nessun can_use_tool).
                    // Se un giorno lo facesse e noi restassimo zitti, il turno resterebbe
                    // appeso per sempre: meglio saperlo dal log che scoprirlo dall'utente.
                    _logger.LogWarning("[ClaudeCodeSession] control_request dal CLI, NON gestita: {Raw}",
                        Truncate(root.GetRawText(), 1000));
                    break;
            }
        }

        private void HandleSystem(JsonElement root)
        {
            var subtype = root.TryGetProperty("subtype", out var s) ? s.GetString() : null;
            if (subtype != "init") return;

            if (root.TryGetProperty("session_id", out var sid)) SessionId = sid.GetString();
            if (root.TryGetProperty("model", out var m)) EffectiveModel = m.GetString();
            if (root.TryGetProperty("claude_code_version", out var v)) CliVersion = v.GetString();

            var effectiveMode = root.TryGetProperty("permissionMode", out var pm) ? pm.GetString() : null;
            _logger.LogInformation(
                "[ClaudeCodeSession] init session={SessionId} model={Model} cli={Version} permissionMode={Mode}",
                SessionId, EffectiveModel, CliVersion, effectiveMode);

            if (!string.Equals(effectiveMode, REQUESTED_PERMISSION_MODE, StringComparison.Ordinal))
            {
                // Fallire forte, non adattarsi. `--permission-mode manual` viene ignorato in
                // silenzio dal CLI: se accettassimo un modo diverso da quello chiesto,
                // gireremmo con permessi che non abbiamo scelto — e non ce ne accorgeremmo.
                var ex = new InvalidOperationException(
                    $"Claude Code gira con permissionMode='{effectiveMode}' invece di " +
                    $"'{REQUESTED_PERMISSION_MODE}'. Il CLI ha ignorato la modalità richiesta: " +
                    "la sessione viene rifiutata perché la superficie di esecuzione non è quella decisa.");
                _activeTurnCompletion?.TrySetException(ex);
                _activeStreamChannel?.Writer.TryComplete(ex);
            }
        }

        private void HandleStreamEvent(JsonElement root)
        {
            if (!root.TryGetProperty("event", out var ev)) return;
            if (!ev.TryGetProperty("type", out var evType)) return;
            if (evType.GetString() != "content_block_delta") return;
            if (!ev.TryGetProperty("delta", out var delta)) return;
            if (!delta.TryGetProperty("type", out var deltaType)) return;

            switch (deltaType.GetString())
            {
                case "text_delta":
                    if (delta.TryGetProperty("text", out var t))
                        Emit(ClaudeCodeChunk.KindMessage, t.GetString());
                    break;
                case "thinking_delta":
                    if (delta.TryGetProperty("thinking", out var th))
                        Emit(ClaudeCodeChunk.KindThinking, th.GetString());
                    break;
            }
        }

        /// <summary>
        /// Dai messaggi <c>assistant</c> prendiamo <b>solo</b> i <c>tool_use</c>: il testo
        /// arriva già dai delta, e rileggerlo qui lo raddoppierebbe in chat.
        /// </summary>
        private void HandleAssistant(JsonElement root)
        {
            if (!root.TryGetProperty("message", out var msg)) return;
            if (!msg.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array) return;

            foreach (var block in content.EnumerateArray())
            {
                if (!block.TryGetProperty("type", out var bt) || bt.GetString() != "tool_use") continue;
                var name = block.TryGetProperty("name", out var n) ? n.GetString() : "?";
                var input = block.TryGetProperty("input", out var i) ? Truncate(i.GetRawText(), 160) : string.Empty;
                Emit(ClaudeCodeChunk.KindTool, $"{name} {input}");
            }
        }

        /// <summary>
        /// I <c>tool_result</c> tornano dentro un messaggio di tipo <c>user</c> (è il CLI che
        /// rimanda all'agente l'esito). Quelli con <c>isReplay</c> sono invece l'eco del nostro
        /// stesso messaggio, e vanno ignorati.
        /// </summary>
        private void HandleUser(JsonElement root)
        {
            if (root.TryGetProperty("isReplay", out var replay) &&
                replay.ValueKind == JsonValueKind.True)
            {
                _logger.LogDebug("[ClaudeCodeSession] messaggio utente accettato dal CLI (replay)");
                return;
            }

            if (!root.TryGetProperty("message", out var msg)) return;
            if (!msg.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array) return;

            foreach (var block in content.EnumerateArray())
            {
                if (!block.TryGetProperty("type", out var bt) || bt.GetString() != "tool_result") continue;
                var isError = block.TryGetProperty("is_error", out var e) && e.ValueKind == JsonValueKind.True;
                Emit(ClaudeCodeChunk.KindTool, isError ? "↳ errore" : "↳ ok");
            }
        }

        private void HandleRateLimit(JsonElement root)
        {
            if (!root.TryGetProperty("rate_limit_info", out var info)) return;

            double? fiveHour = null, sevenDay = null;
            DateTimeOffset? fiveHourReset = null, sevenDayReset = null;

            if (info.TryGetProperty("unifiedWindows", out var windows))
            {
                (fiveHour, fiveHourReset) = ReadWindow(windows, "five_hour");
                (sevenDay, sevenDayReset) = ReadWindow(windows, "seven_day");
            }

            LastRateLimit = new ClaudeCodeRateLimit
            {
                Status = info.TryGetProperty("status", out var st) ? st.GetString() : null,
                FiveHourUtilization = fiveHour,
                FiveHourResetsAt = fiveHourReset,
                SevenDayUtilization = sevenDay,
                SevenDayResetsAt = sevenDayReset
            };

            _logger.LogInformation(
                "[ClaudeCodeSession] finestre consumo: 5h={FiveHour:P1} 7g={SevenDay:P1} status={Status}",
                fiveHour ?? 0, sevenDay ?? 0, LastRateLimit.Status);
        }

        private static (double?, DateTimeOffset?) ReadWindow(JsonElement windows, string name)
        {
            if (!windows.TryGetProperty(name, out var w)) return (null, null);
            double? utilization = w.TryGetProperty("utilization", out var u) && u.ValueKind == JsonValueKind.Number
                ? u.GetDouble() : null;
            DateTimeOffset? resets = w.TryGetProperty("resetsAt", out var r) && r.ValueKind == JsonValueKind.Number
                ? DateTimeOffset.FromUnixTimeSeconds(r.GetInt64()) : null;
            return (utilization, resets);
        }

        /// <summary>
        /// Il <c>result</c> chiude il turno. ⚠️ Quando siamo stati noi a mandare l'interrupt il
        /// CLI riporta <c>error_during_execution</c>: è l'esito atteso di uno Stop, non un
        /// guasto, e va restituito come turno concluso — altrimenti l'utente vede un errore
        /// rosso per aver premuto un pulsante che ha funzionato.
        /// </summary>
        private void HandleResult(JsonElement root)
        {
            var subtype = root.TryGetProperty("subtype", out var s) ? s.GetString() : null;
            var isError = root.TryGetProperty("is_error", out var e) && e.ValueKind == JsonValueKind.True;

            // `total_cost_usd` è il cumulato della SESSIONE, non il costo del turno: il costo
            // del turno è la differenza dal consuntivo precedente. Chiamarlo "costo del turno"
            // e mostrarlo così farebbe crescere all'infinito il numero sotto la chat.
            var sessionCost = ReadDouble(root, "total_cost_usd");
            double? turnCost = null;
            if (sessionCost.HasValue)
            {
                var previous = LastTurnUsage?.SessionCostUsd;
                turnCost = previous.HasValue
                    ? Math.Max(0, sessionCost.Value - previous.Value)
                    : sessionCost.Value;
            }

            var usage = new ClaudeCodeTurnUsage
            {
                Subtype = subtype,
                Model = EffectiveModel,
                SessionCostUsd = sessionCost,
                TurnCostUsd = turnCost,
                DurationMs = ReadLong(root, "duration_ms"),
                PermissionDenials = root.TryGetProperty("permission_denials", out var pd) &&
                                    pd.ValueKind == JsonValueKind.Array
                    ? pd.GetArrayLength() : 0,
                InputTokens = ReadUsageInt(root, "input_tokens"),
                OutputTokens = ReadUsageInt(root, "output_tokens"),
                CacheReadInputTokens = ReadUsageInt(root, "cache_read_input_tokens"),
                CacheCreationInputTokens = ReadUsageInt(root, "cache_creation_input_tokens"),
                ThinkingTokens = ReadThinkingTokens(root)
            };
            LastTurnUsage = usage;

            _logger.LogInformation(
                "[ClaudeCodeSession] turno concluso subtype={Subtype} costoTurno={TurnCost}$ costoSessione={SessionCost}$ durata={Duration}ms interrotto={Interrupted}",
                subtype, usage.TurnCostUsd, usage.SessionCostUsd, usage.DurationMs, _interruptRequested);

            var completion = _activeTurnCompletion;
            if (completion == null) return;

            if (isError && !_interruptRequested)
            {
                var message = root.TryGetProperty("result", out var r) && r.ValueKind == JsonValueKind.String
                    ? r.GetString()
                    : subtype;
                completion.TrySetException(new InvalidOperationException(
                    $"Claude Code ha chiuso il turno in errore ({subtype}): {message}"));
                return;
            }

            completion.TrySetResult(usage);
        }

        private static double? ReadDouble(JsonElement root, string name) =>
            root.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.Number ? el.GetDouble() : null;

        private static long? ReadLong(JsonElement root, string name) =>
            root.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.Number ? el.GetInt64() : null;

        private static int? ReadUsageInt(JsonElement root, string name)
        {
            if (!root.TryGetProperty("usage", out var usage)) return null;
            return usage.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.Number
                ? el.GetInt32() : null;
        }

        private static int? ReadThinkingTokens(JsonElement root)
        {
            if (!root.TryGetProperty("usage", out var usage)) return null;
            if (!usage.TryGetProperty("output_tokens_details", out var details)) return null;
            return details.TryGetProperty("thinking_tokens", out var el) && el.ValueKind == JsonValueKind.Number
                ? el.GetInt32() : null;
        }

        private void Emit(string kind, string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            _activeStreamChannel?.Writer.TryWrite(new ClaudeCodeChunk(kind, text));
        }

        private string DescribeStderr()
        {
            var lines = _stderrRing.ToArray();
            if (lines.Length == 0) return string.Empty;
            return " Ultime righe di stderr: " + string.Join(" | ", lines);
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            _processExited = true;
            int? exitCode = null;
            try { exitCode = _process?.ExitCode; } catch { }
            _logger.LogInformation("[ClaudeCodeSession] Processo terminato (ExitCode={ExitCode})", exitCode);

            var ex = new InvalidOperationException(
                $"Il processo Claude Code è terminato (exit code {exitCode?.ToString() ?? "?"})." + DescribeStderr());
            _activeTurnCompletion?.TrySetException(ex);
            _activeStreamChannel?.Writer.TryComplete(ex);
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
                    // Chiudere stdin è il congedo pulito: il CLI conclude e esce da solo.
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

            var disposedEx = new ObjectDisposedException(nameof(ClaudeCodeSession));
            _activeTurnCompletion?.TrySetException(disposedEx);
            _activeStreamChannel?.Writer.TryComplete();
        }

        private static string Truncate(string s, int max) =>
            string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "…";
    }
}
