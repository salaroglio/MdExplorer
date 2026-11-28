using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MdExplorer.Features.Services.AI.ChatHistory;

namespace MdExplorer.Features.Services.AI.Codex
{
    /// <summary>
    /// Una conversazione con Codex: un processo <c>codex app-server</c> (D2: uno per sessione) e un thread dentro.
    /// <para>
    /// Forma misurata su Codex 0.172.0 (28/11/2026): <c>turn/start</c> → <c>item/started</c> / <c>item/*/delta</c> /
    /// <c>item/completed</c> → <c>thread/tokenUsage/updated</c> → <c>turn/completed</c> con lo <c>status</c>. Il modello
    /// passato a <c>turn/start</c> vale anche per i turni dopo, quindi cambiare modello non chiede un processo nuovo.
    /// </para>
    /// <para>
    /// ⚠️ I subagent di Codex scrivono sullo stesso stream con un altro <c>threadId</c> (misurato: la risposta del
    /// subagent è arrivata col suo thread): ogni notifica si filtra sul thread e sul turno di questa sessione, o il testo
    /// di un subagent finirebbe nella risposta.
    /// </para>
    /// <para>Sprint: docs-internal/Sprints/2026-11-28-Motore-Codex.md, fase F1.</para>
    /// </summary>
    public sealed class CodexSession : IAsyncDisposable
    {
        private static readonly int PROMPT_IDLE_TIMEOUT_MS = (int)MdExplorer.Features.Agents.AgentTurnLimits.Idle.TotalMilliseconds;
        private static readonly int PROMPT_HARD_TIMEOUT_MS = (int)MdExplorer.Features.Agents.AgentTurnLimits.Hard.TotalMilliseconds;

        private readonly ILogger _logger;
        private readonly string _workingDirectory;
        private readonly CodexSessionOptions _options;
        private volatile string _modelId;
        private CodexAppServer _server;

        private Channel<CodexChunk> _activeChannel;
        private TaskCompletionSource<CodexTurnUsage> _activeTurn;
        private volatile string _activeTurnId;
        private CancellationTokenSource _activeIdleCts;
        private readonly SemaphoreSlim _promptGate = new(1, 1);
        private CodexTurnUsage _pendingUsage;
        private bool _messageStarted;
        private long _turnStartedAt;
        // Le modifiche ai file annunciate (item/started) per id: la richiesta di approvazione porta solo l'id.
        private readonly ConcurrentDictionary<string, IReadOnlyList<string>> _announcedChanges = new(StringComparer.Ordinal);
        private volatile bool _disposed;

        /// <summary>L'id del thread di Codex: è l'id della sessione del motore per la cronologia e per la ripresa.</summary>
        public string SessionId { get; private set; }
        public string WorkingDirectory => _workingDirectory;
        public string ModelId => _modelId;
        /// <summary>Il modello che Codex dichiara per il thread (anche quando non ne abbiamo chiesto uno).</summary>
        public string EffectiveModel { get; private set; }
        /// <summary>La versione del CLI, dall'user agent dell'<c>initialize</c>.</summary>
        public string CliVersion { get; private set; }
        public CodexTurnUsage LastTurnUsage { get; private set; }
        public CodexRateLimit LastRateLimit { get; private set; }
        /// <summary>Il testo della risposta dell'ultimo turno, intero (per confrontarlo con quello trasmesso).</summary>
        public string LastTurnText { get; private set; }
        /// <summary>true se nell'ultimo turno è arrivato del ragionamento: con <c>gpt-6-luna</c> sul piano free non arriva.</summary>
        public bool LastTurnHadReasoning { get; private set; }
        public bool IsAlive => _server?.IsAlive == true && !_disposed;
        public DateTime LastUsedUtc { get; private set; } = DateTime.UtcNow;

        public string ProfileKey => _options.ProfileKey;
        public string McpGroups => _options.McpGroups;
        public string ExtraRootsKey => _options.ExtraRootsKey ?? string.Empty;
        public bool WebSearch => _options.WebSearch;

        public CodexSession(ILogger logger, string workingDirectory, string modelId, CodexSessionOptions options = null)
        {
            _logger = logger;
            _workingDirectory = workingDirectory;
            _modelId = string.IsNullOrWhiteSpace(modelId) ? null : modelId.Trim();
            _options = options ?? CodexSessionOptions.Default;
        }

        /// <summary>
        /// Verifica dei diagrammi dei file .md scritti nel turno (C4 della valutazione del 07/11), come per Claude e
        /// opencode: a turno finito MdExplorer li controlla e, se ci sono errori, li rimanda a Codex per UN turno.
        /// </summary>
        public MdExplorer.Features.Diagrams.PlantumlBlockVerifier DiagramVerifier
        {
            get => _diagramVerifier;
            set
            {
                _diagramVerifier = value;
                _diagrams = value == null ? null : new MdExplorer.Features.Diagrams.WrittenDiagramsCheck(value, _workingDirectory, "Codex");
            }
        }
        private MdExplorer.Features.Diagrams.PlantumlBlockVerifier _diagramVerifier;
        private MdExplorer.Features.Diagrams.WrittenDiagramsCheck _diagrams;

        /// <summary>
        /// Avvia il processo e apre il thread (nuovo, o ripreso con <see cref="CodexSessionOptions.ResumeThreadId"/>).
        /// Prima legge la configurazione dell'utente per spegnere i suoi server MCP personali (D5).
        /// </summary>
        public async Task StartAsync(CancellationToken ct = default)
        {
            if (_server != null) throw new InvalidOperationException(UiText.Say("Sessione già avviata", "Session already started"));
            _server = new CodexAppServer(_logger, _workingDirectory, _options.Environment);
            _server.Notification += OnNotification;
            _server.ServerRequest = OnServerRequestAsync;
            _server.Exited += OnServerExited;
            await _server.StartAsync(ct).ConfigureAwait(false);
            CliVersion = VersionOf(_server.UserAgent);

            var personal = await PersonalMcpServersAsync(ct).ConfigureAwait(false);
            var parameters = new JsonObject
            {
                ["cwd"] = _workingDirectory,
                ["sandbox"] = _options.SandboxId,
                ["approvalPolicy"] = _options.ApprovalPolicyId,
                ["config"] = _options.BuildConfig(personal),
                ["serviceName"] = "mdexplorer",
            };
            if (_modelId != null) parameters["model"] = _modelId;

            JsonElement result;
            if (!string.IsNullOrWhiteSpace(_options.ResumeThreadId))
            {
                parameters["threadId"] = _options.ResumeThreadId;
                parameters["excludeTurns"] = true;
                result = await _server.RequestAsync("thread/resume", parameters, ct, TimeSpan.FromSeconds(120)).ConfigureAwait(false);
            }
            else
            {
                result = await _server.RequestAsync("thread/start", parameters, ct, TimeSpan.FromSeconds(120)).ConfigureAwait(false);
            }
            SessionId = result.GetProperty("thread").GetProperty("id").GetString();
            if (result.TryGetProperty("model", out var model) && model.ValueKind == JsonValueKind.String) EffectiveModel = model.GetString();
            _logger.LogInformation("[CodexSession] thread {Thread} {How} cwd={Cwd} modello={Model} sandbox={Sandbox} approvazioni={Approval} cli={Version}",
                SessionId, _options.ResumeThreadId != null ? "ripreso" : "nuovo", _workingDirectory, _modelId ?? EffectiveModel ?? "(predefinito)",
                _options.SandboxId, _options.ApprovalPolicyId, CliVersion);
        }

        private async Task<IReadOnlyList<string>> PersonalMcpServersAsync(CancellationToken ct)
        {
            var read = await _server.RequestAsync("config/read", new { cwd = _workingDirectory }, ct).ConfigureAwait(false);
            if (!read.TryGetProperty("config", out var config) || !config.TryGetProperty("mcp_servers", out var servers)
                || servers.ValueKind != JsonValueKind.Object) return Array.Empty<string>();
            return servers.EnumerateObject().Select(p => p.Name).ToList();
        }

        /// <summary>Il prossimo turno gira su <paramref name="modelId"/>: Codex lo accetta su <c>turn/start</c>, la conversazione resta.</summary>
        public Task SetModelAsync(string modelId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(modelId)) throw new ArgumentException(UiText.Say("Il modello è obbligatorio", "The model is required"), nameof(modelId));
            _modelId = modelId.Trim();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Manda un messaggio e restituisce i frammenti man mano che arrivano. Un turno per volta. Con
        /// <see cref="DiagramVerifier"/> può seguire un turno di correzione dei diagrammi, nello stesso flusso.
        /// </summary>
        public async IAsyncEnumerable<CodexChunk> PromptAsync(string text, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var diagrams = _diagrams;
            diagrams?.Reset();
            await foreach (var chunk in OneTurnAsync(text, ct).ConfigureAwait(false)) yield return chunk;
            if (diagrams == null || ct.IsCancellationRequested) yield break;

            var (notice, forModel) = await diagrams.TakeAsync(lastRound: false, ct).ConfigureAwait(false);
            if (notice != null) yield return new CodexChunk(CodexChunk.KindTool, notice);
            if (forModel == null) yield break;

            await foreach (var chunk in OneTurnAsync(forModel, ct).ConfigureAwait(false)) yield return chunk;
            if (ct.IsCancellationRequested) yield break;
            var (after, _) = await diagrams.TakeAsync(lastRound: true, ct).ConfigureAwait(false);
            if (after != null) yield return new CodexChunk(CodexChunk.KindTool, after);
        }

        private async IAsyncEnumerable<CodexChunk> OneTurnAsync(string text, [EnumeratorCancellation] CancellationToken ct = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(CodexSession));
            if (SessionId == null) throw new InvalidOperationException(UiText.Say("Sessione non avviata", "Session not started"));

            await _promptGate.WaitAsync(ct).ConfigureAwait(false);
            LastUsedUtc = DateTime.UtcNow;
            var channel = Channel.CreateUnbounded<CodexChunk>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
            var turn = new TaskCompletionSource<CodexTurnUsage>(TaskCreationOptions.RunContinuationsAsynchronously);
            var idleCts = new CancellationTokenSource(PROMPT_IDLE_TIMEOUT_MS);
            var hardCts = new CancellationTokenSource(PROMPT_HARD_TIMEOUT_MS);
            var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, idleCts.Token, hardCts.Token);
            _activeChannel = channel;
            _activeTurn = turn;
            _activeIdleCts = idleCts;
            _activeTurnId = null;
            _pendingUsage = null;
            _messageStarted = false;
            _answer.Clear();
            LastTurnHadReasoning = false;
            _turnStartedAt = Environment.TickCount64;

            _ = WaitTurnAsync(turn.Task, linked.Token).ContinueWith(t =>
            {
                if (t.IsFaulted) channel.Writer.TryComplete(t.Exception?.GetBaseException());
                else if (t.IsCanceled)
                {
                    var reason = idleCts.IsCancellationRequested ? MdExplorer.Features.Agents.AgentTurnLimits.Why("Codex", idle: true)
                        : hardCts.IsCancellationRequested ? MdExplorer.Features.Agents.AgentTurnLimits.Why("Codex", idle: false)
                        : UiText.Say("Turno annullato", "Turn cancelled");
                    channel.Writer.TryComplete(new OperationCanceledException(reason));
                }
                else channel.Writer.TryComplete();
            }, TaskScheduler.Default);

            try
            {
                var parameters = new JsonObject
                {
                    ["threadId"] = SessionId,
                    ["input"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = text }),
                };
                if (_modelId != null) parameters["model"] = _modelId;
                var started = await _server.RequestAsync("turn/start", parameters, linked.Token).ConfigureAwait(false);
                _activeTurnId = started.GetProperty("turn").GetProperty("id").GetString();
            }
            catch
            {
                channel.Writer.TryComplete();
                Cleanup();
                throw;
            }

            try
            {
                await foreach (var chunk in channel.Reader.ReadAllAsync(ct).ConfigureAwait(false)) yield return chunk;
            }
            finally
            {
                if (idleCts.IsCancellationRequested || hardCts.IsCancellationRequested || ct.IsCancellationRequested)
                {
                    // Su qualunque annullamento va detto anche a Codex, o continua a lavorare per niente.
                    try { await CancelAsync(CancellationToken.None).ConfigureAwait(false); } catch { }
                    try
                    {
                        using var grace = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        await turn.Task.WaitAsync(grace.Token).ConfigureAwait(false);
                    }
                    catch { }
                }
                Cleanup();
            }

            void Cleanup()
            {
                Interlocked.CompareExchange(ref _activeChannel, null, channel);
                Interlocked.CompareExchange(ref _activeTurn, null, turn);
                Interlocked.CompareExchange(ref _activeIdleCts, null, idleCts);
                _activeTurnId = null;
                try { idleCts.Dispose(); } catch { }
                try { hardCts.Dispose(); } catch { }
                try { linked.Dispose(); } catch { }
                _promptGate.Release();
            }
        }

        private static async Task<CodexTurnUsage> WaitTurnAsync(Task<CodexTurnUsage> turn, CancellationToken ct)
        {
            var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var registration = ct.Register(() => cancelled.TrySetResult(true));
            if (await Task.WhenAny(turn, cancelled.Task).ConfigureAwait(false) == turn) return await turn.ConfigureAwait(false);
            throw new OperationCanceledException(ct);
        }

        /// <summary>Ferma il turno in corso (<c>turn/interrupt</c>): Codex chiude il turno con <c>status: "interrupted"</c>.</summary>
        public async Task CancelAsync(CancellationToken ct = default)
        {
            var turnId = _activeTurnId;
            if (_disposed || turnId == null || _server?.IsAlive != true) return;
            try
            {
                await _server.RequestAsync("turn/interrupt", new { threadId = SessionId, turnId }, ct, TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                _logger.LogInformation("[CodexSession] turno {Turn} interrotto", turnId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[CodexSession] interruzione del turno {Turn} fallita", turnId);
            }
        }

        private readonly System.Text.StringBuilder _answer = new();

        private bool Mine(JsonElement p)
        {
            if (p.ValueKind != JsonValueKind.Object) return false;
            if (!p.TryGetProperty("threadId", out var thread) || thread.GetString() != SessionId) return false;
            // Una notifica del turno precedente (in coda dopo un'interruzione) non entra nel turno nuovo.
            var turnId = _activeTurnId;
            return turnId == null || !p.TryGetProperty("turnId", out var turn) || turn.GetString() == turnId;
        }

        private void OnNotification(string method, JsonElement p)
        {
            try { _activeIdleCts?.CancelAfter(PROMPT_IDLE_TIMEOUT_MS); } catch { }

            if (method == "account/rateLimits/updated")
            {
                LastRateLimit = ReadRateLimit(p);
                return;
            }
            if (!Mine(p)) return;

            switch (method)
            {
                case "item/agentMessage/delta":
                    var delta = p.TryGetProperty("delta", out var d) ? d.GetString() : null;
                    if (string.IsNullOrEmpty(delta)) break;
                    _answer.Append(delta);
                    _diagrams?.AnswerText(delta);
                    Emit(new CodexChunk(CodexChunk.KindMessage, delta));
                    break;
                case "item/reasoning/summaryTextDelta":
                case "item/reasoning/textDelta":
                    var thought = p.TryGetProperty("delta", out var r) ? r.GetString() : null;
                    if (string.IsNullOrEmpty(thought)) break;
                    LastTurnHadReasoning = true;
                    Emit(new CodexChunk(CodexChunk.KindThinking, thought));
                    break;
                case "item/started":
                    if (p.TryGetProperty("item", out var started)) ItemStarted(started);
                    break;
                case "item/completed":
                    if (p.TryGetProperty("item", out var completed)) ItemCompleted(completed);
                    break;
                case "thread/tokenUsage/updated":
                    _pendingUsage = ReadUsage(p);
                    break;
                case "error":
                    _logger.LogWarning("[CodexSession] errore dal server: {Error}", Truncate(p.GetRawText(), 500));
                    break;
                case "turn/completed":
                    TurnCompleted(p);
                    break;
            }
        }

        private void ItemStarted(JsonElement item)
        {
            var type = item.TryGetProperty("type", out var t) ? t.GetString() : null;
            var id = item.TryGetProperty("id", out var i) ? i.GetString() : null;
            switch (type)
            {
                case "agentMessage":
                    // Più messaggi nello stesso turno (commento, poi risposta): una riga vuota fra l'uno e l'altro.
                    if (_messageStarted) { _answer.Append("\n\n"); Emit(new CodexChunk(CodexChunk.KindMessage, "\n\n")); }
                    _messageStarted = true;
                    break;
                case "commandExecution":
                    var command = item.TryGetProperty("command", out var c) ? c.GetString() : "?";
                    Emit(new CodexChunk(CodexChunk.KindTool, "shell · " + Truncate(command, 160), "shell · " + Truncate(command, 160),
                        ChatToolRecord.Call("shell", id, item.Clone())));
                    break;
                case "fileChange":
                    var paths = ChangedPaths(item);
                    if (id != null) _announcedChanges[id] = paths;
                    var what = "edit · " + string.Join(", ", paths.Select(System.IO.Path.GetFileName));
                    Emit(new CodexChunk(CodexChunk.KindTool, what, what, ChatToolRecord.Call("edit", id,
                        item.TryGetProperty("changes", out var changes) ? changes.Clone() : (JsonElement?)null)));
                    break;
                case "mcpToolCall":
                    var tool = $"{(item.TryGetProperty("server", out var s) ? s.GetString() : "?")}/{(item.TryGetProperty("tool", out var n) ? n.GetString() : "?")}";
                    var args = item.TryGetProperty("arguments", out var a) ? a.Clone() : (JsonElement?)null;
                    Emit(new CodexChunk(CodexChunk.KindTool, $"{tool} {Truncate(args?.GetRawText(), 160)}", tool, ChatToolRecord.Call(tool, id, args)));
                    break;
                case "webSearch":
                    var query = item.TryGetProperty("query", out var q) ? q.GetString() : string.Empty;
                    Emit(new CodexChunk(CodexChunk.KindTool, "web · " + query, "web · " + query, ChatToolRecord.Call("web_search", id, item.Clone())));
                    break;
            }
        }

        private void ItemCompleted(JsonElement item)
        {
            var type = item.TryGetProperty("type", out var t) ? t.GetString() : null;
            var id = item.TryGetProperty("id", out var i) ? i.GetString() : null;
            var status = item.TryGetProperty("status", out var s) && s.ValueKind == JsonValueKind.String ? s.GetString() : null;
            var failed = status is "failed" or "declined";
            switch (type)
            {
                case "commandExecution":
                    var output = item.TryGetProperty("aggregatedOutput", out var o) && o.ValueKind == JsonValueKind.String ? o.GetString() : string.Empty;
                    var exitCode = item.TryGetProperty("exitCode", out var e) && e.ValueKind == JsonValueKind.Number ? e.GetInt32() : (int?)null;
                    failed |= exitCode is not null and not 0;
                    Emit(new CodexChunk(CodexChunk.KindTool, Outcome(status, failed), Outcome(status, failed),
                        ChatToolRecord.Result("shell", id, output, item.Clone(), failed)));
                    break;
                case "fileChange":
                    if (id != null) _announcedChanges.TryRemove(id, out _);
                    if (!failed && item.TryGetProperty("changes", out var changes)) _diagrams?.ToolInput(changes);
                    Emit(new CodexChunk(CodexChunk.KindTool, Outcome(status, failed), Outcome(status, failed),
                        ChatToolRecord.Result("edit", id, string.Join("\n", ChangedPaths(item)), item.Clone(), failed)));
                    break;
                case "mcpToolCall":
                    var tool = $"{(item.TryGetProperty("server", out var sv) ? sv.GetString() : "?")}/{(item.TryGetProperty("tool", out var n) ? n.GetString() : "?")}";
                    string content;
                    if (item.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.Object)
                    {
                        failed = true;
                        content = err.TryGetProperty("message", out var m) ? m.GetString() : err.GetRawText();
                    }
                    else content = item.TryGetProperty("result", out var res) ? McpResultText(res) : string.Empty;
                    Emit(new CodexChunk(CodexChunk.KindTool, Outcome(status, failed), Outcome(status, failed),
                        ChatToolRecord.Result(tool, id, content, item.TryGetProperty("result", out var whole) ? whole.Clone() : (JsonElement?)null, failed)));
                    break;
            }
        }

        private static string Outcome(string status, bool failed) => status == "declined"
            ? UiText.Say("↳ rifiutato", "↳ declined")
            : failed ? UiText.Say("↳ errore", "↳ error") : "↳ ok";

        private void TurnCompleted(JsonElement p)
        {
            var completion = _activeTurn;
            if (completion == null || !p.TryGetProperty("turn", out var turn)) return;
            var status = turn.TryGetProperty("status", out var s) ? s.GetString() : null;
            var usage = _pendingUsage ?? new CodexTurnUsage();
            usage = new CodexTurnUsage
            {
                Model = _modelId ?? EffectiveModel,
                InputTokens = usage.InputTokens,
                CachedInputTokens = usage.CachedInputTokens,
                OutputTokens = usage.OutputTokens,
                ReasoningTokens = usage.ReasoningTokens,
                TotalTokens = usage.TotalTokens,
                ThreadTotalTokens = usage.ThreadTotalTokens,
                ContextWindow = usage.ContextWindow,
                DurationMs = turn.TryGetProperty("durationMs", out var dm) && dm.ValueKind == JsonValueKind.Number ? dm.GetInt64() : Environment.TickCount64 - _turnStartedAt,
                Status = status,
            };
            LastTurnUsage = usage;
            LastTurnText = _answer.ToString();
            _logger.LogInformation("[CodexSession] turno concluso thread={Thread} status={Status} token={Tokens} durata={Duration}ms",
                SessionId, status, usage.TotalTokens, usage.DurationMs);

            if (status == "failed")
            {
                var error = turn.TryGetProperty("error", out var e) && e.ValueKind == JsonValueKind.Object
                    ? (e.TryGetProperty("message", out var m) ? m.GetString() : e.GetRawText())
                    : UiText.Say("errore senza motivo", "error with no reason");
                completion.TrySetException(new InvalidOperationException(UiText.Say($"Codex ha chiuso il turno in errore: {error}", $"Codex ended the turn with an error: {error}")));
                return;
            }
            // "interrupted" è l'esito atteso di uno Stop, non un guasto.
            completion.TrySetResult(usage);
        }

        private Task<object> OnServerRequestAsync(string method, JsonElement p)
        {
            object answer = method switch
            {
                "item/commandExecution/requestApproval" => new { decision = Decide(new CodexApprovalRequest
                {
                    Kind = CodexApprovalRequest.KindCommand,
                    Command = p.TryGetProperty("command", out var c) ? c.GetString() : null,
                    Cwd = p.TryGetProperty("cwd", out var w) ? w.GetString() : null,
                }, method) ? "accept" : "decline" },
                "item/fileChange/requestApproval" => new { decision = Decide(new CodexApprovalRequest
                {
                    Kind = CodexApprovalRequest.KindFileChange,
                    Paths = p.TryGetProperty("itemId", out var item) && _announcedChanges.TryGetValue(item.GetString() ?? string.Empty, out var paths)
                        ? paths : Array.Empty<string>(),
                }, method) ? "accept" : "decline" },
                // Le versioni vecchie del protocollo (v1): stessa decisione, altri nomi.
                "execCommandApproval" => new { decision = Decide(new CodexApprovalRequest
                {
                    Kind = CodexApprovalRequest.KindCommand,
                    Command = p.TryGetProperty("command", out var lc) ? string.Join(" ", lc.EnumerateArray().Select(x => x.GetString())) : null,
                    Cwd = p.TryGetProperty("cwd", out var lw) ? lw.GetString() : null,
                }, method) ? "approved" : "denied" },
                "applyPatchApproval" => new { decision = Decide(new CodexApprovalRequest { Kind = CodexApprovalRequest.KindFileChange }, method) ? "approved" : "denied" },
                // Permessi in più (cartelle, rete) fuori dal sandbox: mai, il sandbox lo decide MdExplorer.
                "item/permissions/requestApproval" => new { permissions = new { } },
                // Nessuno davanti a uno schermo per un modulo MCP o una domanda: si declina, il turno prosegue.
                "mcpServer/elicitation/request" => new { action = "decline", content = (object)null },
                "item/tool/requestUserInput" => new { answers = new { } },
                _ => throw new InvalidOperationException(UiText.Say($"Richiesta di Codex non gestita: {method}", $"Unhandled Codex request: {method}")),
            };
            return Task.FromResult(answer);
        }

        private bool Decide(CodexApprovalRequest request, string method)
        {
            var decision = _options.Approve?.Invoke(request) ?? CodexApprovalDecision.No(UiText.Say(
                "questa sessione non accetta comandi né modifiche", "this session accepts no commands or changes"));
            _logger.LogInformation("[CodexSession] {Method} {What} → {Decision} {Reason}", method,
                Truncate(request.Command ?? string.Join(", ", request.Paths), 200), decision.Accept ? "accept" : "decline", decision.Reason);
            if (!decision.Accept && !string.IsNullOrWhiteSpace(decision.Reason))
                Emit(new CodexChunk(CodexChunk.KindTool, UiText.Say("MdExplorer ha rifiutato: ", "MdExplorer declined: ") + decision.Reason));
            return decision.Accept;
        }

        private void OnServerExited(string reason)
        {
            var ex = new InvalidOperationException(reason);
            _activeTurn?.TrySetException(ex);
            _activeChannel?.Writer.TryComplete(ex);
        }

        private void Emit(CodexChunk chunk) => _activeChannel?.Writer.TryWrite(chunk);

        internal static IReadOnlyList<string> ChangedPaths(JsonElement item)
        {
            if (!item.TryGetProperty("changes", out var changes) || changes.ValueKind != JsonValueKind.Array) return Array.Empty<string>();
            return changes.EnumerateArray()
                .Select(c => c.TryGetProperty("path", out var path) ? path.GetString() : null)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .ToList();
        }

        private static string McpResultText(JsonElement result)
        {
            if (result.ValueKind != JsonValueKind.Object || !result.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
                return result.ValueKind == JsonValueKind.Undefined ? string.Empty : result.GetRawText();
            return string.Join("\n", content.EnumerateArray().Select(part =>
                part.TryGetProperty("type", out var type) && type.GetString() == "text" && part.TryGetProperty("text", out var text)
                    ? text.GetString() : part.GetRawText()));
        }

        public static CodexTurnUsage ReadUsage(JsonElement p)
        {
            if (!p.TryGetProperty("tokenUsage", out var usage)) return null;
            var last = usage.TryGetProperty("last", out var l) ? l : default;
            var total = usage.TryGetProperty("total", out var t) ? t : default;
            return new CodexTurnUsage
            {
                InputTokens = Long(last, "inputTokens"),
                CachedInputTokens = Long(last, "cachedInputTokens"),
                OutputTokens = Long(last, "outputTokens"),
                ReasoningTokens = Long(last, "reasoningOutputTokens"),
                TotalTokens = Long(last, "totalTokens"),
                ThreadTotalTokens = Long(total, "totalTokens"),
                ContextWindow = Long(usage, "modelContextWindow"),
            };
        }

        public static CodexRateLimit ReadRateLimit(JsonElement p)
        {
            if (!p.TryGetProperty("rateLimits", out var limits)) return null;
            var primary = limits.TryGetProperty("primary", out var pr) && pr.ValueKind == JsonValueKind.Object ? pr : default;
            var secondary = limits.TryGetProperty("secondary", out var se) && se.ValueKind == JsonValueKind.Object ? se : default;
            return new CodexRateLimit
            {
                PlanType = limits.TryGetProperty("planType", out var plan) && plan.ValueKind == JsonValueKind.String ? plan.GetString() : null,
                PrimaryUsedPercent = Double(primary, "usedPercent"),
                PrimaryWindowMinutes = Long(primary, "windowDurationMins"),
                PrimaryResetsAt = Long(primary, "resetsAt") is long pReset ? DateTimeOffset.FromUnixTimeSeconds(pReset) : null,
                SecondaryUsedPercent = Double(secondary, "usedPercent"),
                SecondaryWindowMinutes = Long(secondary, "windowDurationMins"),
                SecondaryResetsAt = Long(secondary, "resetsAt") is long sReset ? DateTimeOffset.FromUnixTimeSeconds(sReset) : null,
                ReachedType = limits.TryGetProperty("rateLimitReachedType", out var reached) && reached.ValueKind == JsonValueKind.String ? reached.GetString() : null,
            };
        }

        private static long? Long(JsonElement e, string name) =>
            e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt64() : null;
        private static double? Double(JsonElement e, string name) =>
            e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : null;

        /// <summary>La versione dall'user agent: <c>mdexplorer/0.172.0 (Ubuntu …)</c> → <c>0.172.0</c>.</summary>
        public static string VersionOf(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent)) return null;
            var slash = userAgent.IndexOf('/');
            if (slash < 0) return null;
            var rest = userAgent.Substring(slash + 1);
            var space = rest.IndexOf(' ');
            return space < 0 ? rest : rest.Substring(0, space);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;
            var ex = new ObjectDisposedException(nameof(CodexSession));
            _activeTurn?.TrySetException(ex);
            _activeChannel?.Writer.TryComplete();
            if (_server != null) await _server.DisposeAsync().ConfigureAwait(false);
            _promptGate.Dispose();
        }

        private static string Truncate(string s, int max) =>
            string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "…";
    }
}
