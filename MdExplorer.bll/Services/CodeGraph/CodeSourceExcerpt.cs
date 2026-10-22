using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Il taglia e cuci del sorgente per le domande all'LLM (22/10/2026): solo le righe dei simboli da descrivere, dalla
    /// riga di partenza a quella di fine che l'analisi AST scrive negli script (<c>code:lineStart</c>/<c>code:lineEnd</c>).
    /// Prima ogni domanda mandava il file intero, e dal basso lo stesso file torna a ogni livello (utente: «che sia uno
    /// spreco mi sembra ovvio»). Una funzione porta con sé il commento di documentazione subito sopra; un tipo (classe,
    /// interfaccia, enum…) le sue righe SENZA i corpi dei membri, di cui resta la prima riga: i membri l'LLM li conosce
    /// dalle sintesi già scritte (<c>members</c> nel prompt).
    /// </summary>
    public static class CodeSourceExcerpt
    {
        private static readonly HashSet<string> TypeKinds = new(StringComparer.Ordinal)
            { "class", "interface", "record", "struct", "enum", "type", "component", "service", "directive", "pipe", "ngmodule" };

        /// <summary>Quante righe di commento sopra un simbolo si portano al massimo.</summary>
        public const int MaxDocLines = 40;

        public static string[] Lines(string sourceText) => (sourceText ?? string.Empty).Replace("\r\n", "\n").Split('\n');

        /// <summary>
        /// Le righe dei simboli <paramref name="wanted"/> (in ordine di riga, sovrapposizioni fuse), ognuna sotto
        /// <c>// ---- righe a-b ----</c>; dentro un tipo i corpi dei membri di <paramref name="ast"/> sono sostituiti da
        /// <c>// … corpo di X (righe a-b) …</c>.
        /// </summary>
        public static string For(string sourceText, IEnumerable<ScriptSymbol> wanted, CodeScript ast)
        {
            var lines = Lines(sourceText);
            var children = ast.Symbols.Where(s => s.ParentId != null && s.LineStart != null && !s.IsExternal)
                .GroupBy(s => s.ParentId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

            // (da, a, righe da togliere dentro) per simbolo; poi si fondono i pezzi che si toccano
            var pieces = new List<(int From, int To, List<(int From, int To, string Name)> Elided)>();
            foreach (var s in wanted.Where(s => s.LineStart != null).OrderBy(s => s.LineStart))
            {
                var from = Math.Clamp(s.LineStart.Value, 1, lines.Length);
                var to = Math.Clamp(s.LineEnd ?? from, from, lines.Length);
                from = WithDocAbove(lines, from);
                var elided = new List<(int, int, string)>();
                if (TypeKinds.Contains(s.Kind) && children.TryGetValue(s.Id, out var members))
                    foreach (var m in members.Where(m => m.Kind != "field" && m.LineEnd != null && m.LineEnd > m.LineStart).OrderBy(m => m.LineStart))
                        elided.Add((m.LineStart.Value + 1, Math.Min(m.LineEnd.Value, to), m.Name));   // resta la prima riga, la firma
                pieces.Add((from, to, elided));
            }
            var merged = new List<(int From, int To, List<(int From, int To, string Name)> Elided)>();
            foreach (var p in pieces.OrderBy(p => p.From))
            {
                if (merged.Count > 0 && p.From <= merged[^1].To + 1)
                {
                    var last = merged[^1];
                    last.Elided.AddRange(p.Elided);
                    merged[^1] = (last.From, Math.Max(last.To, p.To), last.Elided);
                }
                else merged.Add(p);
            }

            var sb = new StringBuilder();
            foreach (var (from, to, elided) in merged)
            {
                sb.Append("// ---- righe ").Append(from).Append('-').Append(to).Append(" ----\n");
                for (var l = from; l <= to; l++)
                {
                    var cut = elided.FirstOrDefault(e => l >= e.From && l <= e.To);
                    if (cut.Name != null)
                    {
                        sb.Append("    // … corpo di ").Append(cut.Name).Append(" (righe ").Append(cut.From - 1).Append('-').Append(cut.To).Append("): vedi members …\n");
                        l = cut.To;
                        continue;
                    }
                    sb.Append(lines[l - 1]).Append('\n');
                }
                sb.Append('\n');
            }
            return sb.ToString();
        }

        /// <summary>
        /// L'intestazione del file (package, import, using, require…): le righe prima del primo simbolo dichiarato, al più
        /// <paramref name="maxLines"/>. Per la descrizione del file, insieme alle sintesi dei suoi simboli.
        /// </summary>
        public static string Header(string sourceText, CodeScript ast, int maxLines = 60)
        {
            var lines = Lines(sourceText);
            var first = ast.Symbols.Where(s => !s.IsExternal && s.Kind != "module" && s.LineStart != null).Select(s => s.LineStart.Value).DefaultIfEmpty(lines.Length + 1).Min();
            var to = Math.Min(Math.Min(first - 1, lines.Length), maxLines);
            if (to < 1) return string.Empty;
            var sb = new StringBuilder();
            sb.Append("// ---- righe 1-").Append(to).Append(" (intestazione) ----\n");
            for (var l = 1; l <= to; l++) sb.Append(lines[l - 1]).Append('\n');
            return sb.Append('\n').ToString();
        }

        /// <summary>Il commento di documentazione subito sopra la riga <paramref name="from"/> (Javadoc, ///, #, docstring escluse).</summary>
        private static int WithDocAbove(string[] lines, int from)
        {
            var l = from - 1;
            var taken = 0;
            while (l >= 1 && taken < MaxDocLines)
            {
                var t = lines[l - 1].TrimStart();
                if (!(t.StartsWith("*") || t.StartsWith("/*") || t.StartsWith("//") || t.StartsWith("#") || t.StartsWith("@") || t.StartsWith("<#") || t.StartsWith("#>"))) break;
                l--; taken++;
            }
            return l + 1;
        }
    }
}
