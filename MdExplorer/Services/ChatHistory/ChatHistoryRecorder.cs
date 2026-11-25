using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.AI.ChatHistory;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.ChatHistory
{
    /// <summary>
    /// Registra le chat di MarkAgent mentre succedono (sprint 2026-11-24-Chat-Memorizzate, F2): la riga della cronologia
    /// nel DB utente (<see cref="ChatThread"/>) e il contenuto nel file JSONL (<see cref="IChatFileStore"/>).
    /// Una chat per connessione del tab: i canali laterali (commit, diagrammi…) ci entrano col loro canale (D2);
    /// «nuova chat» e la disconnessione la chiudono. La scrittura avviene nel turno stesso, che gira già in background:
    /// l'ordine è quello degli eventi e un errore si vede subito.
    /// </summary>
    public sealed class ChatHistoryRecorder
    {
        private readonly IChatFileStore _files;
        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<ChatHistoryRecorder> _logger;
        private readonly ConcurrentDictionary<string, ThreadState> _threads = new(StringComparer.Ordinal);
        // Le connessioni a cui si è già detto che la chat non si salva perché manca il progetto: una volta sola, non a ogni turno.
        private readonly ConcurrentDictionary<string, byte> _toldNoProject = new(StringComparer.Ordinal);

        public ChatHistoryRecorder(IChatFileStore files, IServiceScopeFactory scopes, ILogger<ChatHistoryRecorder> logger)
        {
            _files = files ?? throw new ArgumentNullException(nameof(files));
            _scopes = scopes ?? throw new ArgumentNullException(nameof(scopes));
            _logger = logger;
        }

        internal sealed class ThreadState
        {
            public Guid Id;
            public string RelativePath;
            public string ProjectPath;
            public long NextSeq = 1;
            public int Turn;
            /// <summary>Vero finché il client non ha saputo l'id della chat (lo riceve al primo turno).</summary>
            public bool Announce;
            /// <summary>L'ultima sessione del motore scritta nella riga: si riscrive solo se cambia.</summary>
            public string EngineSessionId;
            /// <summary>Titolo e dettaglio attuali (F5): vanno nelle istruzioni del turno dopo.</summary>
            public string Title;
            public string Detail;
        }

        private const string ThreadPromptResource = "MdExplorer.Service.skills.mde_mark_thread.prompt.md";
        private static readonly Lazy<string> _threadPrompt = new(LoadThreadPrompt);

        /// <summary>
        /// Il testo delle istruzioni del blocco titolo/dettaglio (skill mde-mark-thread), dalla risorsa incorporata e mai
        /// dal progetto: il formato lo legge il codice. Senza frontmatter né commenti.
        /// </summary>
        internal static string ThreadPrompt => _threadPrompt.Value;

        private static string LoadThreadPrompt()
        {
            using var stream = typeof(ChatHistoryRecorder).Assembly.GetManifestResourceStream(ThreadPromptResource)
                ?? throw new InvalidOperationException($"Manca la risorsa {ThreadPromptResource}: il pacchetto non è completo.");
            using var reader = new System.IO.StreamReader(stream);
            var text = reader.ReadToEnd();
            var fm = System.Text.RegularExpressions.Regex.Match(text, @"^---[ \t]*\r?\n.*?\r?\n---[ \t]*\r?\n", System.Text.RegularExpressions.RegexOptions.Singleline);
            if (fm.Success && fm.Index == 0) text = text.Substring(fm.Length);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"<!--.*?-->", string.Empty, System.Text.RegularExpressions.RegexOptions.Singleline);
            return text.Trim();
        }

        /// <summary>Come è andato un <see cref="Resume"/>.</summary>
        public sealed class ResumeOutcome
        {
            public bool Resumed { get; init; }
            /// <summary>Perché non si è ripresa, detto all'utente; null se si è ripresa.</summary>
            public string Reason { get; init; }
            /// <summary>Righe del file che non si leggono (si riprende lo stesso, dal progressivo più alto letto).</summary>
            public IReadOnlyList<string> Problems { get; init; } = Array.Empty<string>();
        }

        /// <summary>
        /// Apre la registrazione di un turno. Se la chat della connessione non c'è ancora (o il progetto è cambiato), la
        /// crea. Con nessun progetto non si registra: la cronologia è per progetto, e lo si dice nel turno.
        /// </summary>
        public ChatTurnRecording BeginTurn(string connectionId, string projectPath, string engine, string modelId, string channel)
        {
            if (string.IsNullOrEmpty(connectionId)) throw new ArgumentException("Manca la connessione della chat.", nameof(connectionId));
            if (string.IsNullOrWhiteSpace(projectPath))
            {
                _logger.LogWarning("[ChatHistory] turno di {ConnectionId} non registrato: nessun progetto", connectionId);
                return ChatTurnRecording.NotRecorded(UiText.Say(
                    "Questa chat non viene salvata nella cronologia: non è legata a un progetto aperto.",
                    "This chat is not saved in the history: it is not tied to an open project."),
                    tell: _toldNoProject.TryAdd(connectionId, 0));
            }

            try
            {
                var state = _threads.AddOrUpdate(connectionId,
                    _ => OpenThread(projectPath, engine, modelId),
                    (_, existing) => string.Equals(existing.ProjectPath, projectPath, StringComparison.Ordinal)
                        ? existing
                        : OpenThread(projectPath, engine, modelId));
                int turn;
                long seq;
                lock (state)
                {
                    turn = ++state.Turn;
                    seq = state.NextSeq;
                }
                var accumulator = new ChatTurnAccumulator(seq, turn, channel, engine);
                return new ChatTurnRecording(this, state, accumulator, engine, modelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ChatHistory] apertura della chat fallita per {ConnectionId}", connectionId);
                return ChatTurnRecording.NotRecorded(Unsaved(ex), tell: true);
            }
        }

        /// <summary>«Nuova chat» o disconnessione: il turno dopo apre una chat nuova. Il file e la riga restano.</summary>
        public void Forget(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId)) return;
            _threads.TryRemove(connectionId, out _);
            _toldNoProject.TryRemove(connectionId, out _);
        }

        /// <summary>
        /// Riaggancia alla connessione una chat già nella cronologia (una riconnessione di SignalR, o «riprendi»): i turni
        /// dopo proseguono nello stesso file, col progressivo e il numero di turno letti da lì. Rifiuta, dicendo perché, una
        /// chat che non c'è, di un altro progetto o senza file.
        /// </summary>
        public ResumeOutcome Resume(string connectionId, Guid threadId, string projectPath)
        {
            if (string.IsNullOrEmpty(connectionId)) throw new ArgumentException("Manca la connessione della chat.", nameof(connectionId));
            if (string.IsNullOrWhiteSpace(projectPath))
                return new ResumeOutcome { Reason = UiText.Say("Nessun progetto aperto per questa chat.", "No project open for this chat.") };

            ChatThread row;
            using (var scope = _scopes.CreateScope())
            {
                var factory = scope.ServiceProvider.GetRequiredService<IDALFactory<IUserSettingsDB>>();
                using var session = factory.OpenSession();
                row = session.GetDal<ChatThread>().GetList().SingleOrDefault(t => t.Id == threadId);
            }
            if (row == null)
                return new ResumeOutcome { Reason = UiText.Say($"La chat {threadId} non è nella cronologia.", $"The chat {threadId} is not in the history.") };
            if (!string.Equals(row.ProjectPath, projectPath, StringComparison.Ordinal))
                return new ResumeOutcome { Reason = UiText.Say(
                    $"La chat {threadId} è di un altro progetto ({row.ProjectPath}).", $"The chat {threadId} belongs to another project ({row.ProjectPath}).") };

            var content = _files.Read(row.FilePath);
            if (!content.Exists)
                return new ResumeOutcome { Reason = UiText.Say(
                    $"Il file della chat non c'è più: {row.FilePath}.", $"The chat file is gone: {row.FilePath}.") };

            var state = new ThreadState
            {
                Id = row.Id,
                RelativePath = row.FilePath,
                ProjectPath = row.ProjectPath,
                NextSeq = content.Events.Count == 0 ? 1 : content.Events.Max(e => e.Seq) + 1,
                Turn = content.Events.Count == 0 ? 0 : content.Events.Max(e => e.Turn),
                EngineSessionId = row.EngineSessionId,
                Title = row.Title,
                Detail = row.Detail,
            };
            _threads[connectionId] = state;
            _toldNoProject.TryRemove(connectionId, out _);
            foreach (var p in content.Problems) _logger.LogWarning("[ChatHistory] chat {ThreadId} ripresa con una riga illeggibile: {Problem}", threadId, p);
            _logger.LogInformation("[ChatHistory] chat {ThreadId} ripresa su {ConnectionId} dal progressivo {Seq}", threadId, connectionId, state.NextSeq);
            return new ResumeOutcome { Resumed = true, Problems = content.Problems };
        }

        /// <summary>Una chat eliminata: nessuna connessione deve più scriverci.</summary>
        public void ForgetThread(Guid threadId)
        {
            foreach (var pair in _threads.Where(p => p.Value.Id == threadId).ToList())
                _threads.TryRemove(pair.Key, out _);
        }

        /// <summary>La chat registrata per la connessione, se c'è (per i test e per F4).</summary>
        public Guid? CurrentThreadId(string connectionId)
            => connectionId != null && _threads.TryGetValue(connectionId, out var s) ? s.Id : null;

        internal static string Unsaved(Exception ex) => UiText.Say(
            "La chat non viene salvata nella cronologia: ", "The chat is not being saved in the history: ") + ex.Message;

        private ThreadState OpenThread(string projectPath, string engine, string modelId)
        {
            using var scope = _scopes.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDALFactory<IUserSettingsDB>>();
            using var session = factory.OpenSession();
            using var tx = session.BeginTransaction();
            var now = DateTime.UtcNow;
            var row = new ChatThread
            {
                ProjectPath = projectPath,
                // Il percorso dipende dall'Id, che nasce al Save (GuidComb): segnaposto, poi il vero nella stessa transazione.
                FilePath = "-",
                Engine = engine,
                ModelId = modelId,
                StartedAt = now,
                LastActivityAt = now,
            };
            session.GetDal<ChatThread>().Save(row);
            row.FilePath = _files.RelativePathFor(projectPath, row.Id);
            session.GetDal<ChatThread>().Save(row);
            tx.Commit();
            _logger.LogInformation("[ChatHistory] chat {ThreadId} aperta ({Engine}) in {File}", row.Id, engine, row.FilePath);
            return new ThreadState { Id = row.Id, RelativePath = row.FilePath, ProjectPath = projectPath, Announce = true };
        }

        internal void Write(ThreadState state, IReadOnlyList<ChatRecordEvent> events, long nextSeq)
        {
            if (events.Count == 0) return;
            lock (state)
            {
                _files.Append(state.RelativePath, events);
                state.NextSeq = Math.Max(state.NextSeq, nextSeq);
            }
        }

        internal void Touch(ThreadState state, string engine, string modelId, string engineSessionId, string title, string detail)
        {
            using var scope = _scopes.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDALFactory<IUserSettingsDB>>();
            using var session = factory.OpenSession();
            using var tx = session.BeginTransaction();
            var row = session.GetDal<ChatThread>().GetList().SingleOrDefault(t => t.Id == state.Id)
                ?? throw new InvalidOperationException(UiText.Say(
                    $"la riga della chat {state.Id} non è più nella cronologia.", $"the chat row {state.Id} is no longer in the history."));
            row.LastActivityAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(engine)) row.Engine = engine;
            if (!string.IsNullOrEmpty(modelId)) row.ModelId = modelId;
            if (!string.IsNullOrEmpty(engineSessionId))
            {
                row.EngineSessionId = engineSessionId;
                state.EngineSessionId = engineSessionId;
            }
            if (title != null) { row.Title = title; lock (state) state.Title = title; }
            if (detail != null) { row.Detail = detail; lock (state) state.Detail = detail; }
            session.GetDal<ChatThread>().Save(row);
            tx.Commit();
        }

        internal ILogger Logger => _logger;
    }

    /// <summary>
    /// La registrazione di un turno: riceve quello che il turno manda al client e lo scrive. Se qualcosa non si può
    /// scrivere, il turno va avanti e il motivo si dice una volta (<see cref="TakeFailureToTell"/>).
    /// </summary>
    public sealed class ChatTurnRecording
    {
        private readonly ChatHistoryRecorder _recorder;
        private readonly ChatHistoryRecorder.ThreadState _state;
        private readonly ChatTurnAccumulator _accumulator;
        private readonly string _engine;
        private readonly string _modelId;
        private readonly object _gate = new();
        private string _failure;
        private int _told;

        internal ChatTurnRecording(ChatHistoryRecorder recorder, ChatHistoryRecorder.ThreadState state,
            ChatTurnAccumulator accumulator, string engine, string modelId)
        {
            _recorder = recorder;
            _state = state;
            _accumulator = accumulator;
            _engine = engine;
            _modelId = modelId;
        }

        private ChatTurnRecording(string failure, bool tell)
        {
            _failure = failure;
            _told = tell ? 0 : 1;
        }

        internal static ChatTurnRecording NotRecorded(string why, bool tell) => new(why, tell);

        public bool IsRecording => _accumulator != null && _failure == null;
        public Guid? ThreadId => _state?.Id;
        private string _engineSessionId;

        /// <summary>
        /// L'id della chat, la prima volta che lo si chiede per una chat appena aperta: il client lo tiene per riprenderla
        /// dopo una riconnessione. Poi null.
        /// </summary>
        public Guid? TakeThreadToAnnounce()
        {
            if (_state == null) return null;
            lock (_state)
            {
                if (!_state.Announce) return null;
                _state.Announce = false;
                return _state.Id;
            }
        }

        private string _newTitle;
        private string _newDetail;

        /// <summary>Vero se in questo turno al modello è stato chiesto il blocco titolo/dettaglio (solo il tab, F5).</summary>
        public bool ExpectsThreadBlock { get; private set; }

        /// <summary>
        /// Le istruzioni del blocco titolo/dettaglio da mettere in fondo alla domanda, con titolo e dettaglio attuali; da
        /// qui in poi il turno aspetta il blocco. Null se il turno non si registra.
        /// </summary>
        public string ThreadInstruction()
        {
            if (!IsRecording) return null;
            string title, detail;
            lock (_state) { title = _state.Title; detail = _state.Detail; }
            ExpectsThreadBlock = true;
            return ChatThreadBlock.Instruction(ChatHistoryRecorder.ThreadPrompt, title, detail);
        }

        /// <summary>
        /// Il blocco arrivato a fine risposta (null se non è arrivato). Registra il blocco nella chat e prepara titolo e
        /// dettaglio per la riga. Restituisce l'avviso per l'utente, o null se è tutto a posto.
        /// </summary>
        public string ApplyThreadBlock(string inner, bool unclosed)
        {
            if (!ExpectsThreadBlock || !IsRecording) return null;
            if (inner == null)
            {
                var why = unclosed
                    ? UiText.Say("il blocco è cominciato ma non è stato chiuso", "the block was started but not closed")
                    : UiText.Say("il modello non ha mandato il blocco", "the model did not send the block");
                Meta("thread-block-missing", new { reason = why });
                return UiText.Say($"Titolo e dettaglio della chat non aggiornati a questo turno: {why}.",
                    $"Chat title and detail not updated this turn: {why}.");
            }
            var parsed = ChatThreadBlock.Parse(inner);
            Meta("thread-block", new { raw = inner, title = parsed.Title, detail = parsed.Detail, problems = parsed.Problems });
            _newTitle = parsed.Title;
            _newDetail = parsed.Detail;
            return parsed.Problems.Count == 0 ? null : UiText.Say(
                "Titolo o dettaglio della chat non aggiornati: " + string.Join("; ", parsed.Problems) + ".",
                "Chat title or detail not updated: " + string.Join("; ", parsed.Problems) + ".");
        }

        /// <summary>La sessione del motore che ha risposto: va nella riga, per far ripartire il motore da lì.</summary>
        public void EngineSession(string engineSessionId)
        {
            if (!string.IsNullOrWhiteSpace(engineSessionId)) _engineSessionId = engineSessionId;
        }

        public void UserPrompt(string text) => Run(a => a.UserPrompt(text));
        public void Meta(string what, object data) => Run(a => a.Meta(what, data));
        public void OnClientEvent(string method, object[] args) => Run(a => a.OnClientEvent(method, args));
        /// <summary>Una chiamata di strumento o il suo risultato, interi, dal parser del motore.</summary>
        public void Tool(ChatToolRecord record) => Run(a => a.Tool(record));

        /// <summary>Fine del turno: chiude il blocco aperto e aggiorna la riga della cronologia.</summary>
        public void Complete()
        {
            Run(a => a.Complete());
            if (!IsRecording) return;
            try { _recorder.Touch(_state, _engine, _modelId, _engineSessionId, _newTitle, _newDetail); }
            catch (Exception ex) { Fail(ex); }
        }

        /// <summary>Il motivo per cui la chat non si salva, la prima volta che lo si chiede; poi null (si dice una volta).</summary>
        public string TakeFailureToTell()
        {
            if (_failure == null) return null;
            return Interlocked.Exchange(ref _told, 1) == 0 ? _failure : null;
        }

        private void Run(Func<ChatTurnAccumulator, IReadOnlyList<ChatRecordEvent>> step)
        {
            if (!IsRecording) return;
            lock (_gate)
            {
                if (!IsRecording) return;
                try
                {
                    var events = step(_accumulator);
                    _recorder.Write(_state, events, _accumulator.NextSeq);
                }
                catch (Exception ex) { Fail(ex); }
            }
        }

        private void Fail(Exception ex)
        {
            _recorder.Logger.LogError(ex, "[ChatHistory] scrittura della chat {ThreadId} fallita", _state?.Id);
            _failure ??= ChatHistoryRecorder.Unsaved(ex);
        }
    }

    /// <summary>
    /// Il client del turno, che inoltra tutto e intanto lo registra. Il client vede esattamente quello che vedeva prima;
    /// se la registrazione fallisce, glielo si dice una volta con un messaggio di sistema, e la chat continua.
    /// </summary>
    public sealed class RecordingClientProxy : IClientProxy
    {
        private readonly IClientProxy _inner;
        private readonly ChatTurnRecording _recording;
        private readonly string _channel;
        // Il blocco titolo/dettaglio non va a video (F5): lo si toglie dal testo mentre arriva.
        private readonly ChatThreadBlock.StreamFilter _filter = new();
        private bool _blockApplied;

        public RecordingClientProxy(IClientProxy inner, ChatTurnRecording recording, string channel)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _recording = recording ?? throw new ArgumentNullException(nameof(recording));
            _channel = channel;
        }

        public async Task SendCoreAsync(string method, object[] args, CancellationToken cancellationToken = default)
        {
            if (_recording.ExpectsThreadBlock && method == "ReceiveStreamChunk" && args?.Length > 0 && args[0] is string piece)
            {
                var shown = _filter.Feed(piece);
                if (shown.Length == 0) return;
                args = (object[])args.Clone();
                args[0] = shown;
            }
            else if (_recording.ExpectsThreadBlock && method == "StreamComplete" && !_blockApplied)
            {
                // Fine della risposta: il testo trattenuto va a video, il blocco nella chat, e l'eventuale avviso prima della fine.
                _blockApplied = true;
                var rest = _filter.Flush();
                if (rest.Length > 0) await SendCoreAsync("ReceiveStreamChunk", new object[] { rest, _channel }, cancellationToken);
                var notice = _recording.ApplyThreadBlock(_filter.Block, _filter.Unclosed);
                if (notice != null) await SendCoreAsync("ReceiveMessage", new object[] { "system", notice }, cancellationToken);
            }

            await _inner.SendCoreAsync(method, args, cancellationToken);
            _recording.OnClientEvent(method, args);
            var failure = _recording.TakeFailureToTell();
            if (failure != null)
                await _inner.SendCoreAsync("ReceiveError", new object[] { failure, _channel }, cancellationToken);
        }
    }
}
