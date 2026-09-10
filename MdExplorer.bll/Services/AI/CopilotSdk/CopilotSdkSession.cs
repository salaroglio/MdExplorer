using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using GitHub.Copilot;
using GitHub.Copilot.Rpc;
using MdExplorer.Features.Services.AI.CopilotChat;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI.CopilotSdk
{
    /// <summary>
    /// A live Copilot conversation held through the official .NET SDK (<c>GitHub.Copilot.SDK</c>),
    /// the replacement for the hand-written ACP adapter.
    ///
    /// <para>
    /// The hub sees it only as <see cref="ICopilotChatSession"/>, the same contract the ACP
    /// transport meets through <see cref="CopilotAcpChatSession"/>, so the transport can change
    /// underneath the hub without the hub learning anything new. Behaviour mirrors ACP too: one
    /// prompt at a time through a gate, an idle timeout that resets on every event, and an
    /// absolute ceiling.
    /// </para>
    ///
    /// <para>
    /// What the SDK gives that ACP could not: <c>SetModelAsync</c> is a real operation on the live
    /// session (ACP had no way to say it, so it sent <c>/model &lt;id&gt;</c> as the TEXT of a
    /// prompt and Copilot discarded it), and the event stream is typed and complete — 133 event
    /// types where the ACP reader understood two.
    /// </para>
    ///
    /// <para>
    /// The CLI is the USER's, not the one bundled in the NuGet package: the user's carries their
    /// authentication, and the bundled one would ask for a second login (and weighs 333 MB in the
    /// build output unless <c>CopilotSkipCliDownload</c> is set).
    /// </para>
    /// </summary>
    public sealed class CopilotSdkSession : ICopilotChatSession
    {
        private const int PROMPT_IDLE_TIMEOUT_MS = 300000;   // 5 min of silence
        private const int PROMPT_HARD_TIMEOUT_MS = 1800000;  // 30 min absolute
        private const int START_TIMEOUT_MS = 120000;         // the first session pays Copilot's bootstrap

        private readonly ILogger _logger;
        private readonly string _workingDirectory;
        private string _modelId;

        private CopilotClient _client;
        private CopilotSession _session;
        private bool _disposed;

        /// <summary>One turn at a time: a session holds a single conversation.</summary>
        private readonly SemaphoreSlim _promptGate = new SemaphoreSlim(1, 1);

        /// <summary>
        /// The turn in flight. Null between turns: events arriving outside a turn (session
        /// housekeeping) have nobody to be shown to and are dropped.
        /// </summary>
        private TurnState _activeTurn;

        /// <summary>
        /// One answer to one prompt, and whether the agent has told us it STARTED it.
        ///
        /// <para>
        /// An answer is NOT one agent turn. As soon as Copilot uses a tool the answer is several
        /// turns in a row — <c>TurnStart 0</c>, tool calls, <c>TurnEnd 0</c> (with no text at
        /// all), <c>TurnStart 1</c>, the text, <c>TurnEnd 1</c> — and only then
        /// <c>AssistantIdle</c>. Measured 10/09/2026. The first version closed the answer at the
        /// first <c>TurnEnd</c>: any question that made Copilot read a file came back EMPTY. So the
        /// end of an answer is <c>AssistantIdle</c>, and <c>TurnEnd</c> is only a boundary between
        /// the rounds of the same answer.
        /// </para>
        ///
        /// <para>
        /// The Started flag guards the beginning: an ending may only close the answer that has seen
        /// its own start. It was introduced for a one-millisecond race (the idle of the previous
        /// answer landing on the channel of the next one); with idle as the only terminator that
        /// leftover no longer exists, and the flag stays as the guarantee that it cannot come back.
        /// </para>
        /// </summary>
        private sealed class TurnState
        {
            public TurnState(Channel<CopilotChatChunk> channel) { Channel = channel; }
            public Channel<CopilotChatChunk> Channel { get; }
            public bool Started;
        }

        /// <summary>Reset by every event of the turn in flight, so "idle" means really idle.</summary>
        private CancellationTokenSource _activePromptIdleCts;

        public CopilotChatTransport Transport => CopilotChatTransport.Sdk;
        public string SessionId => _session?.SessionId;
        public string WorkingDirectory => _workingDirectory;
        public string ModelId => _modelId;
        public bool IsAlive => _session != null && !_disposed;
        public DateTime LastUsedUtc { get; private set; } = DateTime.UtcNow;

        /// <summary>
        /// Taken from <c>AssistantMessage.Data.Model</c>. Not from <c>AssistantTurnStart</c>, where
        /// the SDK has a field with the same name that stays EMPTY — measured on 1.0.82.
        /// </summary>
        public string AnsweredModel { get; private set; }

        public bool CanSwitchModelLive => true;

        public CopilotSdkSession(ILogger logger, string workingDirectory, string modelId)
        {
            _logger = logger;
            _workingDirectory = workingDirectory;
            _modelId = modelId;
        }

        /// <summary>
        /// Starts the CLI in server mode and opens the conversation.
        /// </summary>
        public async Task StartAsync(CancellationToken ct = default)
        {
            if (_session != null) throw new InvalidOperationException("Session already started");

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(START_TIMEOUT_MS);

            var (cliPath, prefixArgs) = CopilotAcp.CopilotProcessLauncher.ResolveStdioTarget();

            _client = new CopilotClient(new CopilotClientOptions
            {
                Connection = RuntimeConnection.ForStdio(cliPath, prefixArgs),
                UseLoggedInUser = true,
            });

            await _client.StartAsync(timeout.Token).ConfigureAwait(false);

            var config = new SessionConfig
            {
                WorkingDirectory = _workingDirectory,
                Streaming = true,
                OnEvent = HandleEvent,
                // Without this the SDK refuses every tool call, and Copilot cannot even read a
                // file of the project it is working in. See CopilotSdkPermissionPolicy.
                OnPermissionRequest = DecidePermissionAsync,
            };

            // No model = the CLI picks its own. Deliberate: which models exist is a property of
            // the INSTALLATION, so writing an id here would be wrong on some machines — the same
            // reason CopilotCliProvider has no default model constant.
            if (!string.IsNullOrWhiteSpace(_modelId))
            {
                config.Model = _modelId;
            }

            _session = await _client.CreateSessionAsync(config, timeout.Token).ConfigureAwait(false);

            _logger.LogInformation("[CopilotSdkSession] session={SessionId} cwd={Cwd} model={Model}",
                _session.SessionId, _workingDirectory, string.IsNullOrWhiteSpace(_modelId) ? "(CLI default)" : _modelId);
        }

        /// <summary>
        /// Changes the model of the LIVE conversation, keeping its memory. This is the operation
        /// ACP did not have.
        /// </summary>
        public async Task SetModelAsync(string modelId, CancellationToken ct = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(CopilotSdkSession));
            if (_session == null) throw new InvalidOperationException("Session not initialized");
            if (string.IsNullOrWhiteSpace(modelId)) throw new ArgumentException("Model id required", nameof(modelId));

            await _session.SetModelAsync(modelId, ct).ConfigureAwait(false);
            _modelId = modelId;
            _logger.LogInformation("[CopilotSdkSession] session={SessionId} passa al modello {Model}", _session.SessionId, modelId);
        }

        /// <summary>
        /// Sends the prompt and yields the answer as it arrives.
        /// </summary>
        public async IAsyncEnumerable<CopilotChatChunk> PromptAsync(
            string text,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(CopilotSdkSession));
            if (_session == null) throw new InvalidOperationException("Session not initialized");

            await _promptGate.WaitAsync(ct).ConfigureAwait(false);
            LastUsedUtc = DateTime.UtcNow;

            var channel = Channel.CreateUnbounded<CopilotChatChunk>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false, // the SDK may raise events from more than one thread
                AllowSynchronousContinuations = false
            });
            var turn = new TurnState(channel);
            _activeTurn = turn;

            // Idle resets on every event of this turn (see HandleEvent); the hard cap is the
            // ceiling regardless of activity.
            var idleCts = new CancellationTokenSource(PROMPT_IDLE_TIMEOUT_MS);
            var hardCts = new CancellationTokenSource(PROMPT_HARD_TIMEOUT_MS);
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, idleCts.Token, hardCts.Token);
            _activePromptIdleCts = idleCts;

            // Fire the turn; the events feed the channel while we yield.
            var sendTask = _session.SendAsync(text, linkedCts.Token);

            // The turn ending closes the channel — but so does a failed send, or the channel
            // would stay open on an error and the caller would wait for a turn that never began.
            _ = sendTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    channel.Writer.TryComplete(t.Exception?.GetBaseException());
                    Interlocked.CompareExchange(ref _activeTurn, null, turn);
                }
                else if (t.IsCanceled)
                {
                    channel.Writer.TryComplete(new OperationCanceledException(DescribeCancellation(idleCts, hardCts)));
                    Interlocked.CompareExchange(ref _activeTurn, null, turn);
                }
                // Success is NOT the end of the turn: SendAsync returns as soon as the message
                // is accepted, the answer keeps arriving as events. The channel is closed by
                // AssistantTurnEnd / AssistantIdle in HandleEvent.
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
                Interlocked.CompareExchange(ref _activeTurn, null, turn);

                var timeoutFired = idleCts.IsCancellationRequested || hardCts.IsCancellationRequested;
                if (timeoutFired || ct.IsCancellationRequested)
                {
                    // Always tell the agent to stop: otherwise it keeps producing content we
                    // silently drop AND its memory of the conversation diverges from what the
                    // user actually read.
                    try { await CancelAsync(CancellationToken.None).ConfigureAwait(false); } catch { }
                }

                try { idleCts.Dispose(); } catch { }
                try { hardCts.Dispose(); } catch { }
                try { linkedCts.Dispose(); } catch { }
                _promptGate.Release();
            }
        }

        /// <summary>
        /// Aborts the turn in flight. A no-op when nothing is running.
        /// </summary>
        public async Task CancelAsync(CancellationToken ct = default)
        {
            var session = _session;
            if (session == null || _disposed) return;

            try
            {
                await session.AbortAsync(ct).ConfigureAwait(false);
                _logger.LogInformation("[CopilotSdkSession] turno annullato su session={SessionId}", session.SessionId);
            }
            catch (Exception ex)
            {
                // Cancelling is best effort: the caller is already unwinding and must not be
                // handed a second failure on the way out.
                _logger.LogWarning(ex, "[CopilotSdkSession] annullamento fallito su session={SessionId}", session.SessionId);
            }
        }

        /// <summary>
        /// The single place events land. Everything is measured against the turn in flight: with
        /// no turn there is nobody to show them to.
        /// </summary>
        private void HandleEvent(SessionEvent evt)
        {
            var turn = _activeTurn;
            if (turn == null) return;

            // Any event proves the agent is alive: idle means silence, not "slow".
            try { _activePromptIdleCts?.CancelAfter(PROMPT_IDLE_TIMEOUT_MS); } catch { /* already disposed */ }

            switch (evt)
            {
                case AssistantTurnStartEvent:
                    // From here on the endings belong to us. TurnId is NOT usable to tell turns
                    // apart: measured on 1.0.82 it is the string "0" for every turn.
                    turn.Started = true;
                    break;

                case AssistantMessageDeltaEvent message:
                    Write(turn.Channel, CopilotChatChunk.KindMessage, message.Data?.DeltaContent);
                    break;

                case AssistantReasoningDeltaEvent reasoning:
                    Write(turn.Channel, CopilotChatChunk.KindThinking, reasoning.Data?.DeltaContent);
                    break;

                // What the agent is doing, as status lines apart from the answer. Before F4 these
                // were all dropped, and the chat stood still while Copilot read files or ran a
                // sub-agent — nothing told "working" from "stuck".
                case AssistantIntentEvent intent:
                    Write(turn.Channel, CopilotChatChunk.KindTool, intent.Data?.Intent);
                    break;

                case ToolExecutionStartEvent tool:
                    Write(turn.Channel, CopilotChatChunk.KindTool, CopilotSdkActivityDescriber.DescribeTool(
                        tool.Data?.ToolName, tool.Data?.Arguments, tool.Data?.McpServerName, tool.Data?.McpToolName,
                        _workingDirectory));
                    break;

                case ToolExecutionProgressEvent progress:
                    Write(turn.Channel, CopilotChatChunk.KindTool, progress.Data?.ProgressMessage);
                    break;

                case SubagentStartedEvent subagent:
                    Write(turn.Channel, CopilotChatChunk.KindTool, CopilotSdkActivityDescriber.DescribeSubagentStarted(
                        subagent.Data?.AgentDisplayName ?? subagent.Data?.AgentName, subagent.Data?.AgentDescription));
                    break;

                case SubagentFailedEvent failed:
                    Write(turn.Channel, CopilotChatChunk.KindTool, CopilotSdkActivityDescriber.DescribeSubagentFailed(
                        failed.Data?.AgentDisplayName ?? failed.Data?.AgentName, failed.Data?.Error));
                    break;

                case AssistantMessageEvent completed:
                    // The complete message, after its deltas: the one place the model that really
                    // answered is written down.
                    if (!string.IsNullOrWhiteSpace(completed.Data?.Model))
                    {
                        AnsweredModel = completed.Data.Model;
                    }
                    break;

                // AssistantTurnEnd is NOT the end of the answer: it separates the rounds of one
                // answer, and the first round of a question that uses a tool carries no text at
                // all. Closing here gave empty answers (see TurnState).

                case AssistantIdleEvent idle:
                    // The agent has nothing left to do: this is the end of the answer.
                    if (idle.Data?.Aborted == true && turn.Started)
                    {
                        _logger.LogInformation("[CopilotSdkSession] turno interrotto su session={SessionId}", _session?.SessionId);
                    }
                    EndTurn(turn);
                    break;

                case SessionErrorEvent error:
                    // Fatal for the answer, and Idle is not guaranteed to follow: without this the
                    // caller would sit on the idle timeout for five minutes before learning why.
                    FailTurn(turn, "Copilot ha segnalato un errore: " + (error.Data?.Message ?? error.Data?.ErrorType ?? "(senza messaggio)"));
                    break;

                case SessionShutdownEvent shutdown:
                    FailTurn(turn, "La sessione Copilot si è chiusa durante la risposta"
                        + (string.IsNullOrWhiteSpace(shutdown.Data?.ErrorReason) ? "" : ": " + shutdown.Data.ErrorReason));
                    break;

                case ModelCallFailureEvent failure:
                    // Not fatal by itself: the agent may retry the call (AssistantTurnRetry). Worth
                    // a line in the log, because it explains a slow answer.
                    _logger.LogWarning("[CopilotSdkSession] chiamata al modello fallita su session={SessionId}: {Error}",
                        _session?.SessionId, failure.Data?.ErrorMessage);
                    break;

                    // Still ignored: usage, session housekeeping, and plan mode (ExitPlanMode*), which
                    // is not a status to show but an interactive flow asking the user to approve.
            }
        }

        /// <summary>
        /// Closes the turn — but only if it had begun. An ending arriving before our own
        /// beginning belongs to the previous turn (see <see cref="TurnState"/>).
        /// </summary>
        private void EndTurn(TurnState turn)
        {
            if (!turn.Started) return;
            turn.Channel.Writer.TryComplete();
            Interlocked.CompareExchange(ref _activeTurn, null, turn);
        }

        /// <summary>
        /// Closes the answer with an error. Unlike <see cref="EndTurn"/> this does not wait for the
        /// start: an error before the beginning is still an error the caller must see.
        /// </summary>
        private void FailTurn(TurnState turn, string message)
        {
            _logger.LogWarning("[CopilotSdkSession] session={SessionId}: {Message}", _session?.SessionId, message);
            turn.Channel.Writer.TryComplete(new InvalidOperationException(message));
            Interlocked.CompareExchange(ref _activeTurn, null, turn);
        }

        // GHCP001: the SDK marks PermissionDecision "for evaluation purposes only". It is also the
        // only way to approve a tool — OnPermissionRequest must return it — so the diagnostic is
        // suppressed here and nowhere else. If a future SDK changes it, this is the method that
        // breaks at compile time, and CopilotChatTransport=acp is the way around it meanwhile.
#pragma warning disable GHCP001
        private Task<PermissionDecision> DecidePermissionAsync(PermissionRequest request, PermissionInvocation invocation)
        {
            var verdict = CopilotSdkPermissionPolicy.Decide(request, _workingDirectory);
            if (verdict.Approved)
            {
                _logger.LogInformation("[CopilotSdkSession] permesso concesso: {What}", verdict.What);
                return Task.FromResult(PermissionDecision.ApproveOnce());
            }

            _logger.LogWarning("[CopilotSdkSession] permesso negato: {What} — {Reason}", verdict.What, verdict.Reason);
            return Task.FromResult(PermissionDecision.Reject(verdict.Reason));
        }
#pragma warning restore GHCP001

        private static void Write(Channel<CopilotChatChunk> channel, string kind, string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            channel.Writer.TryWrite(new CopilotChatChunk(kind, text));
        }

        private static string DescribeCancellation(CancellationTokenSource idleCts, CancellationTokenSource hardCts)
        {
            if (idleCts.IsCancellationRequested)
                return $"Copilot non dà segni di vita da {PROMPT_IDLE_TIMEOUT_MS / 1000} secondi";
            if (hardCts.IsCancellationRequested)
                return $"Copilot ha superato il tetto di {PROMPT_HARD_TIMEOUT_MS / 60000} minuti per una risposta";
            return "Richiesta annullata";
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            // Whoever is reading must not be left hanging on a channel nobody will close.
            _activeTurn?.Channel.Writer.TryComplete(
                new OperationCanceledException("Sessione Copilot chiusa mentre la risposta era in corso"));
            _activeTurn = null;

            var session = _session;
            _session = null;
            if (session != null)
            {
                try { await session.DisposeAsync().ConfigureAwait(false); }
                catch (Exception ex) { _logger.LogWarning(ex, "[CopilotSdkSession] chiusura della sessione fallita"); }
            }

            var client = _client;
            _client = null;
            if (client != null)
            {
                // Closing the client kills the CLI process: a session left behind is an orphan
                // nobody collects.
                try { await client.DisposeAsync().ConfigureAwait(false); }
                catch (Exception ex) { _logger.LogWarning(ex, "[CopilotSdkSession] chiusura del client fallita"); }
            }

            _promptGate.Dispose();
        }
    }
}
