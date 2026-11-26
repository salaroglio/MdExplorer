using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace MdExplorer.Features.Services.AI.ChatHistory
{
    /// <summary>
    /// Il blocco con cui il modello, in fondo a ogni risposta del tab, aggiorna titolo e punti della chat (sprint
    /// 2026-11-24-Chat-Memorizzate, F5, §3.5). Il modello ragiona su che cosa scrivere; qui si fa il resto: togliere il
    /// blocco dal flusso, leggerlo, aggiungere il punto al registro.
    /// <code>
    /// &lt;&lt;&lt;mde-thread
    /// {"title": "…", "point": "…"}
    /// mde-thread&gt;&gt;&gt;
    /// </code>
    /// Dal 26/11/2026 il modello manda solo il punto NUOVO del turno, e MDE lo aggiunge in fondo al registro (il dettaglio
    /// della chat, un punto per riga dal più vecchio): prima riscriveva tutto l'elenco a ogni turno, al massimo 5 punti
    /// riuniti a modo suo, e la storia si perdeva senza che si potesse ricostruire.
    /// </summary>
    public static class ChatThreadBlock
    {
        public const string Start = "<<<mde-thread";
        public const string End = "mde-thread>>>";
        /// <summary>Quanti punti, i più recenti, il modello vede nell'istruzione: per non ripetere un argomento già scritto.</summary>
        public const int PromptPoints = 5;
        public const int MaxTitleLength = 120;
        public const int MaxPointLength = 200;

        /// <summary>Che cosa c'era nel blocco, e che cosa non va.</summary>
        public sealed class Parsed
        {
            /// <summary>null = il titolo non cambia (o non era valido: vedi <see cref="Problems"/>).</summary>
            public string Title { get; init; }
            /// <summary>Il punto nuovo del turno; null = nessun argomento nuovo (o non era valido: vedi <see cref="Problems"/>).</summary>
            public string Point { get; init; }
            public IReadOnlyList<string> Problems { get; init; } = Array.Empty<string>();
        }

        /// <summary>Legge l'interno del blocco (il JSON fra i due marcatori).</summary>
        public static Parsed Parse(string inner)
        {
            var problems = new List<string>();
            JsonDocument doc;
            try { doc = JsonDocument.Parse(inner ?? string.Empty); }
            catch (JsonException ex) { return new Parsed { Problems = new[] { UiText.Say($"il blocco non è JSON valido ({ex.Message})", $"the block is not valid JSON ({ex.Message})") } }; }

            using (doc)
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    return new Parsed { Problems = new[] { UiText.Say("il blocco non è un oggetto JSON", "the block is not a JSON object") } };

                string title = null, point = null;
                if (doc.RootElement.TryGetProperty("title", out var t))
                {
                    var value = t.ValueKind == JsonValueKind.String ? t.GetString()?.Trim() : null;
                    if (string.IsNullOrEmpty(value)) problems.Add(UiText.Say("titolo vuoto o non testo", "title empty or not text"));
                    else if (value.Contains('\n')) problems.Add(UiText.Say("il titolo va su una riga sola", "the title must be on one line"));
                    else if (value.Length > MaxTitleLength) problems.Add(UiText.Say($"titolo di {value.Length} caratteri invece di {MaxTitleLength} al massimo", $"title of {value.Length} characters instead of at most {MaxTitleLength}"));
                    else title = value;
                }
                if (doc.RootElement.TryGetProperty("point", out var pt))
                {
                    var value = pt.ValueKind == JsonValueKind.String ? pt.GetString()?.Trim() : null;
                    // «- …» è il modo in cui il registro lo scrive: se il modello lo mette, è lo stesso punto.
                    if (value != null && (value.StartsWith("- ") || value.StartsWith("* "))) value = value.Substring(2).Trim();
                    if (string.IsNullOrEmpty(value)) problems.Add(UiText.Say("punto vuoto o non testo", "point empty or not text"));
                    else if (value.Contains('\n')) problems.Add(UiText.Say("il punto va su una riga sola", "the point must be on one line"));
                    else if (value.Length > MaxPointLength) problems.Add(UiText.Say($"punto di {value.Length} caratteri invece di {MaxPointLength} al massimo", $"point of {value.Length} characters instead of at most {MaxPointLength}"));
                    else point = value;
                }
                if (doc.RootElement.TryGetProperty("detail", out _))
                    problems.Add(UiText.Say("l'elenco intero non si manda più: solo il punto nuovo («point»)", "the whole list is no longer sent: only the new point («point»)"));
                return new Parsed { Title = title, Point = point, Problems = problems };
            }
        }

        /// <summary>I punti del registro, dal più vecchio: le righe <c>- …</c> (o <c>* …</c>) del dettaglio.</summary>
        public static IReadOnlyList<string> Points(string log)
            => (log ?? string.Empty).Replace("\r\n", "\n").Split('\n').Select(l => l.Trim())
                .Where(l => l.StartsWith("- ") || l.StartsWith("* ")).Select(l => l.Substring(2).Trim()).Where(l => l.Length > 0).ToList();

        /// <summary>
        /// Il registro con il punto nuovo in fondo. Lo stesso punto dell'ultimo non si ripete; null se non cambia niente.
        /// </summary>
        public static string AppendPoint(string log, string point)
        {
            if (string.IsNullOrWhiteSpace(point)) return null;
            var points = Points(log);
            if (points.Count > 0 && string.Equals(points[^1], point.Trim(), StringComparison.OrdinalIgnoreCase)) return null;
            return string.Join("\n", points.Append(point.Trim()).Select(p => "- " + p));
        }

        private static string Short(string s) => s.Length <= 60 ? s : s.Substring(0, 60) + "…";

        /// <summary>
        /// Le istruzioni da mettere in fondo alla domanda: il testo della skill con il titolo attuale al posto di
        /// <c>{{title}}</c> e gli ultimi <see cref="PromptPoints"/> punti del registro, dal più vecchio, al posto di <c>{{detail}}</c>.
        /// </summary>
        public static string Instruction(string template, string currentTitle, string log)
        {
            if (string.IsNullOrWhiteSpace(template)) throw new ArgumentException("Manca il testo delle istruzioni del blocco.", nameof(template));
            var recent = Points(log).TakeLast(PromptPoints).ToList();
            return template
                .Replace("{{title}}", string.IsNullOrWhiteSpace(currentTitle) ? UiText.Say("(nessuno: è il primo turno)", "(none: this is the first turn)") : currentTitle)
                .Replace("{{detail}}", recent.Count == 0 ? UiText.Say("(nessuno)", "(none)") : string.Join("\n", recent.Select(p => "- " + p)));
        }

        /// <summary>
        /// Toglie il blocco dal testo che va a video mentre arriva a pezzi. Trattiene solo quello che potrebbe essere
        /// l'inizio del marcatore, così lo streaming non si ferma; il blocco, una volta chiuso, sta in <see cref="Block"/>.
        /// </summary>
        public sealed class StreamFilter
        {
            private readonly StringBuilder _held = new();
            private readonly StringBuilder _inside = new();
            private bool _inBlock;

            /// <summary>L'interno del blocco chiuso (fra i marcatori), o null finché non c'è.</summary>
            public string Block { get; private set; }

            /// <summary>Il testo da mandare subito a video (può essere vuoto).</summary>
            public string Feed(string piece)
            {
                if (string.IsNullOrEmpty(piece)) return string.Empty;
                var output = new StringBuilder();
                foreach (var c in piece) Step(c, output);
                return output.ToString();
            }

            /// <summary>
            /// Fine del turno: restituisce quello che era trattenuto. Un blocco aperto e mai chiuso torna a video così
            /// com'è (è quello che il modello ha scritto), e <see cref="Unclosed"/> lo dice.
            /// </summary>
            public string Flush()
            {
                var output = _held.ToString();
                _held.Clear();
                if (_inBlock && Block == null)
                {
                    Unclosed = true;
                    output += Start + _inside;
                    _inside.Clear();
                    _inBlock = false;
                }
                return output;
            }

            /// <summary>Vero se il blocco era cominciato ma non è stato chiuso.</summary>
            public bool Unclosed { get; private set; }

            private void Step(char c, StringBuilder output)
            {
                if (Block != null)
                {
                    // Dopo il blocco non dovrebbe arrivare niente; se arriva, va a video.
                    output.Append(c);
                    return;
                }
                if (_inBlock)
                {
                    _inside.Append(c);
                    var text = _inside.ToString();
                    if (text.EndsWith(End, StringComparison.Ordinal))
                    {
                        Block = text.Substring(0, text.Length - End.Length).Trim();
                        _inside.Clear();
                        _inBlock = false;
                    }
                    return;
                }
                _held.Append(c);
                var held = _held.ToString();
                if (held.EndsWith(Start, StringComparison.Ordinal))
                {
                    // Il testo prima del marcatore va a video, senza l'a capo che lo separava dal blocco.
                    output.Append(held.Substring(0, held.Length - Start.Length).TrimEnd('\n', '\r', ' '));
                    _held.Clear();
                    _inBlock = true;
                    return;
                }
                // Si trattiene solo il pezzo finale che potrebbe ancora diventare il marcatore, con gli a capo e gli spazi
                // che lo precedono: se poi comincia il blocco, quelli non devono restare in fondo alla risposta.
                var keep = LongestMarkerPrefixSuffix(held);
                while (keep < held.Length && IsBlank(held[held.Length - keep - 1])) keep++;
                if (keep < held.Length)
                {
                    output.Append(held, 0, held.Length - keep);
                    _held.Remove(0, held.Length - keep);
                }
            }

            private static bool IsBlank(char c) => c == '\n' || c == '\r' || c == ' ' || c == '\t';

            private static int LongestMarkerPrefixSuffix(string s)
            {
                for (var len = Math.Min(s.Length, Start.Length - 1); len > 0; len--)
                    if (string.CompareOrdinal(s, s.Length - len, Start, 0, len) == 0) return len;
                return 0;
            }
        }
    }
}
