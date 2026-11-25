using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace MdExplorer.Features.Services.AI.ChatHistory
{
    /// <summary>
    /// La vista compatta di un'attività da dare al modello per «Impara da questa attività» (sprint
    /// 2026-11-24-Skill-Che-Migliorano, F1): una riga per evento, col progressivo fra parentesi quadre. È una VISTA, non il
    /// contenuto: domande intere, risposte e argomenti accorciati, i risultati solo se sono errori; il resto il modello lo
    /// legge per intero con <c>chat_read</c>. Ogni accorciamento lo dice con «…» e il numero dei caratteri tolti.
    /// Il prototipo è stato provato su tre chat vere in F0 (15-40 KB a chat).
    /// </summary>
    public static class ChatActivityView
    {
        public const int AnswerMax = 600;
        public const int ToolArgsMax = 160;
        public const int ErrorMax = 400;

        public static string Compact(IEnumerable<ChatRecordEvent> events)
        {
            var sb = new StringBuilder();
            foreach (var e in events ?? Enumerable.Empty<ChatRecordEvent>())
            {
                var line = Line(e);
                if (line != null) sb.Append('[').Append(e.Seq).Append("] ").Append(line).Append('\n');
            }
            return sb.ToString().TrimEnd('\n');
        }

        private static string Line(ChatRecordEvent e)
        {
            var side = string.IsNullOrEmpty(e.Channel) || e.Channel == ChatRecordEvent.DefaultChannel ? "" : $"({e.Channel}) ";
            switch (e.Kind)
            {
                case ChatRecordEvent.KindUserPrompt:
                    return side + "utente: " + (e.Content ?? string.Empty).Trim();
                case ChatRecordEvent.KindText:
                    return side + "risposta: " + Short(e.Content, AnswerMax);
                case ChatRecordEvent.KindToolCall:
                    return side + $"strumento {e.Tool ?? "?"}: " + Short(e.Json.HasValue ? e.Json.Value.GetRawText() : e.Content, ToolArgsMax);
                case ChatRecordEvent.KindToolResult:
                    return e.IsError ? side + $"↳ errore di {e.Tool ?? "?"}: " + Short(e.Content, ErrorMax) : null;
                case ChatRecordEvent.KindError:
                    return side + "errore: " + Short(e.Content, ErrorMax);
                case ChatRecordEvent.KindMeta:
                    // Solo quello che l'utente ha visto come messaggio, e le modifiche della domanda (una correzione).
                    var ev = EventName(e);
                    if (ev == "ReceiveMessage") return side + "avviso: " + Short(e.Content, ErrorMax);
                    if (ev == "EditAndRegenerateFromMessage") return side + "l'utente ha modificato una domanda precedente";
                    return null;
                default:
                    // Ragionamento, attività corte, consumi: non servono alla vista; chat_read li dà interi.
                    return null;
            }
        }

        private static string EventName(ChatRecordEvent e)
        {
            if (!e.Json.HasValue || e.Json.Value.ValueKind != JsonValueKind.Object) return e.Content;
            return e.Json.Value.TryGetProperty("event", out var name) && name.ValueKind == JsonValueKind.String ? name.GetString() : e.Content;
        }

        private static string Short(string text, int max)
        {
            var t = (text ?? string.Empty).Trim();
            return t.Length <= max ? t : t.Substring(0, max) + $"… (+{t.Length - max} caratteri)";
        }
    }
}
