using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace MdExplorer.Features.Services.AI.ChatHistory
{
    /// <summary>
    /// Il blocco con cui il modello, in fondo a ogni risposta del tab, aggiorna titolo e dettaglio della chat (sprint
    /// 2026-11-24-Chat-Memorizzate, F5, §3.5). Il modello ragiona su che cosa scrivere; qui si fa il resto: togliere il
    /// blocco dal flusso, leggerlo, controllare la regola del dettaglio.
    /// <code>
    /// &lt;&lt;&lt;mde-thread
    /// {"title": "…", "detail": "- …\n- …"}
    /// mde-thread&gt;&gt;&gt;
    /// </code>
    /// </summary>
    public static class ChatThreadBlock
    {
        public const string Start = "<<<mde-thread";
        public const string End = "mde-thread>>>";
        /// <summary>La regola di oggi per il dettaglio: un elenco puntato di al massimo tanti punti (U7).</summary>
        public const int MaxDetailPoints = 5;
        public const int MaxTitleLength = 120;

        /// <summary>Che cosa c'era nel blocco, e che cosa non va.</summary>
        public sealed class Parsed
        {
            /// <summary>null = il titolo non cambia (o non era valido: vedi <see cref="Problems"/>).</summary>
            public string Title { get; init; }
            /// <summary>null = il dettaglio non cambia (o non era valido: vedi <see cref="Problems"/>).</summary>
            public string Detail { get; init; }
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

                string title = null, detail = null;
                if (doc.RootElement.TryGetProperty("title", out var t))
                {
                    var value = t.ValueKind == JsonValueKind.String ? t.GetString()?.Trim() : null;
                    if (string.IsNullOrEmpty(value)) problems.Add(UiText.Say("titolo vuoto o non testo", "title empty or not text"));
                    else if (value.Contains('\n')) problems.Add(UiText.Say("il titolo va su una riga sola", "the title must be on one line"));
                    else if (value.Length > MaxTitleLength) problems.Add(UiText.Say($"titolo di {value.Length} caratteri invece di {MaxTitleLength} al massimo", $"title of {value.Length} characters instead of at most {MaxTitleLength}"));
                    else title = value;
                }
                if (doc.RootElement.TryGetProperty("detail", out var d))
                {
                    var value = d.ValueKind == JsonValueKind.String ? d.GetString() : null;
                    var why = DetailProblem(value);
                    if (why != null) problems.Add(why);
                    else detail = value.Trim();
                }
                return new Parsed { Title = title, Detail = detail, Problems = problems };
            }
        }

        /// <summary>
        /// Null se il dettaglio rispetta la regola di oggi (un elenco puntato di al massimo <see cref="MaxDetailPoints"/>
        /// punti, un punto per riga), altrimenti il motivo da dire all'utente.
        /// </summary>
        public static string DetailProblem(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown)) return UiText.Say("dettaglio vuoto", "empty detail");
            var lines = markdown.Replace("\r\n", "\n").Split('\n').Where(l => l.Trim().Length > 0).ToList();
            var notBullets = lines.Where(l => !(l.StartsWith("- ") || l.StartsWith("* "))).ToList();
            if (notBullets.Count > 0)
                return UiText.Say($"il dettaglio deve essere un elenco puntato, un punto per riga (riga fuori regola: «{Short(notBullets[0])}»)",
                    $"the detail must be a bulleted list, one point per line (line off the rule: «{Short(notBullets[0])}»)");
            if (lines.Count > MaxDetailPoints)
                return UiText.Say($"{lines.Count} punti invece di {MaxDetailPoints} al massimo", $"{lines.Count} points instead of at most {MaxDetailPoints}");
            return null;
        }

        private static string Short(string s) => s.Length <= 60 ? s : s.Substring(0, 60) + "…";

        /// <summary>
        /// Le istruzioni da mettere in fondo alla domanda: il testo della skill con titolo e dettaglio attuali al posto di
        /// <c>{{title}}</c> e <c>{{detail}}</c>.
        /// </summary>
        public static string Instruction(string template, string currentTitle, string currentDetail)
        {
            if (string.IsNullOrWhiteSpace(template)) throw new ArgumentException("Manca il testo delle istruzioni del blocco.", nameof(template));
            return template
                .Replace("{{title}}", string.IsNullOrWhiteSpace(currentTitle) ? UiText.Say("(nessuno: è il primo turno)", "(none: this is the first turn)") : currentTitle)
                .Replace("{{detail}}", string.IsNullOrWhiteSpace(currentDetail) ? UiText.Say("(nessuno)", "(none)") : currentDetail);
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
