using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace MdExplorer.Features.Services.AI.ChatHistory
{
    /// <summary>
    /// Trasforma quello che un turno di MarkAgent manda al client (gli eventi SignalR di <c>AiChatHub</c>, uguali per
    /// Claude Code, Copilot e opencode) nelle righe del file della chat (sprint 2026-11-24-Chat-Memorizzate, F2).
    /// I pezzi di testo e di ragionamento si accumulano in un blocco (D3): una riga si chiude quando cambia il tipo di
    /// evento o quando il turno finisce. Non scrive niente: restituisce le righe pronte, chi lo usa le salva.
    /// </summary>
    public sealed class ChatTurnAccumulator
    {
        /// <summary>Quello che il client ha visto come attività di uno strumento: una riga corta. Le chiamate intere arrivano in F3.</summary>
        public const string KindActivity = "activity";

        private readonly int _turn;
        private readonly string _channel;
        private readonly string _engine;
        private readonly Func<DateTime> _clock;

        private string _openKind;
        private readonly StringBuilder _open = new();
        private DateTime _openAt;
        private bool _completed;

        /// <param name="nextSeq">Il progressivo della prima riga di questo turno (le righe della chat proseguono da lì).</param>
        public ChatTurnAccumulator(long nextSeq, int turn, string channel, string engine, Func<DateTime> clock = null)
        {
            if (nextSeq < 1) throw new ArgumentOutOfRangeException(nameof(nextSeq), "Il progressivo parte da 1.");
            NextSeq = nextSeq;
            _turn = turn;
            _channel = string.IsNullOrEmpty(channel) ? ChatRecordEvent.DefaultChannel : channel;
            _engine = engine;
            _clock = clock ?? (() => DateTime.UtcNow);
        }

        /// <summary>Il progressivo che avrà la prossima riga: il turno dopo riparte da qui.</summary>
        public long NextSeq { get; private set; }

        /// <summary>La domanda dell'utente com'è arrivata al motore (con l'eventuale blocco di contesto in testa).</summary>
        public IReadOnlyList<ChatRecordEvent> UserPrompt(string text)
        {
            var done = new List<ChatRecordEvent>();
            CloseOpen(done);
            done.Add(Make(ChatRecordEvent.KindUserPrompt, text ?? string.Empty, null));
            return done;
        }

        /// <summary>Un evento a parte deciso dal servizio (per esempio una modifica della domanda), salvato come <c>meta</c>.</summary>
        public IReadOnlyList<ChatRecordEvent> Meta(string what, object data)
        {
            var done = new List<ChatRecordEvent>();
            CloseOpen(done);
            done.Add(Make(ChatRecordEvent.KindMeta, what, ToJson(new { @event = what, data })));
            return done;
        }

        /// <summary>
        /// Un evento mandato al client. Restituisce le righe che si sono chiuse (spesso nessuna: un pezzo di testo resta
        /// aperto finché ne arrivano altri).
        /// </summary>
        public IReadOnlyList<ChatRecordEvent> OnClientEvent(string method, object[] args)
        {
            var done = new List<ChatRecordEvent>();
            if (_completed || string.IsNullOrEmpty(method)) return done;
            args ??= Array.Empty<object>();
            var first = args.Length > 0 ? args[0] : null;

            switch (method)
            {
                case "ReceiveStreamChunk":
                    Grow(ChatRecordEvent.KindText, first as string, done);
                    break;
                case "ReceiveThinking":
                    Grow(ChatRecordEvent.KindThinking, first as string, done);
                    break;
                case "ReceiveToolActivity":
                    CloseOpen(done);
                    done.Add(Make(KindActivity, first as string ?? string.Empty, null));
                    break;
                case "ReceiveError":
                    CloseOpen(done);
                    var error = Make(ChatRecordEvent.KindError, first as string ?? string.Empty, null);
                    error.IsError = true;
                    done.Add(error);
                    break;
                case "ReceiveMessage":
                    // ReceiveMessage(ruolo, testo): un messaggio di sistema mostrato nella chat.
                    CloseOpen(done);
                    done.Add(Make(ChatRecordEvent.KindMeta, args.Length > 1 ? args[1] as string : null,
                        ToJson(new { @event = method, role = first as string })));
                    break;
                case "StreamComplete":
                    CloseOpen(done);
                    break;
                default:
                    if (method.EndsWith("Usage", StringComparison.Ordinal))
                    {
                        CloseOpen(done);
                        done.Add(Make(ChatRecordEvent.KindUsage, null, ToJson(new { @event = method, data = first })));
                    }
                    else if (method.StartsWith("Receive", StringComparison.Ordinal) || method == "TruncateMessagesAfter")
                    {
                        // ReceiveStreamMeta, ReceiveAnsweredModel, TruncateMessagesAfter… : tutto quello che il client
                        // riceve entra nella chat, anche ciò che oggi non ha un tipo suo.
                        CloseOpen(done);
                        done.Add(Make(ChatRecordEvent.KindMeta, null, ToJson(new { @event = method, data = first })));
                    }
                    // Gli altri (ModelLoading, ModelLoaded…) riguardano il modello locale, non la conversazione.
                    break;
            }
            return done;
        }

        /// <summary>Una chiamata di strumento o il suo risultato, interi, dal parser del motore (F3).</summary>
        public IReadOnlyList<ChatRecordEvent> Tool(ChatToolRecord record)
        {
            var done = new List<ChatRecordEvent>();
            if (_completed || record == null) return done;
            if (record.Kind != ChatRecordEvent.KindToolCall && record.Kind != ChatRecordEvent.KindToolResult)
                throw new ArgumentException($"Tipo di strumento sconosciuto: «{record.Kind}».", nameof(record));
            CloseOpen(done);
            var e = Make(record.Kind, record.Content, record.Json);
            e.Tool = record.Tool;
            e.CallId = record.CallId;
            e.IsError = record.IsError;
            done.Add(e);
            return done;
        }

        /// <summary>Il turno è finito: chiude il blocco aperto. Dopo, gli eventi si ignorano.</summary>
        public IReadOnlyList<ChatRecordEvent> Complete()
        {
            var done = new List<ChatRecordEvent>();
            if (_completed) return done;
            CloseOpen(done);
            _completed = true;
            return done;
        }

        private void Grow(string kind, string piece, List<ChatRecordEvent> done)
        {
            if (string.IsNullOrEmpty(piece)) return;
            if (_openKind != kind)
            {
                CloseOpen(done);
                _openKind = kind;
                _openAt = _clock();
            }
            _open.Append(piece);
        }

        private void CloseOpen(List<ChatRecordEvent> done)
        {
            if (_openKind == null) return;
            var e = Make(_openKind, _open.ToString(), null);
            e.At = _openAt;
            done.Add(e);
            _openKind = null;
            _open.Clear();
        }

        private ChatRecordEvent Make(string kind, string content, JsonElement? json) => new()
        {
            Seq = NextSeq++,
            Turn = _turn,
            At = _clock(),
            Channel = _channel,
            Kind = kind,
            Engine = _engine,
            Content = content,
            Json = json,
        };

        private static JsonElement? ToJson(object value)
        {
            try
            {
                return JsonSerializer.SerializeToElement(value);
            }
            catch (Exception ex) when (ex is NotSupportedException || ex is InvalidOperationException || ex is JsonException)
            {
                // Un dato che non si serializza non sparisce: si salva il motivo, e il resto della riga resta.
                return JsonSerializer.SerializeToElement(new { unserializable = value?.GetType().FullName, reason = ex.Message });
            }
        }
    }
}
