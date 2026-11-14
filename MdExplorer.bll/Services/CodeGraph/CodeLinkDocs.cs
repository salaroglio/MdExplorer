using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Un punto di una regola da descrivere: dove, i suoi archi con i nomi dei capi, le righe intorno.</summary>
    public sealed class CodeLinkPoint
    {
        public string RuleId { get; init; }
        public string Path { get; init; }
        public int Line { get; init; }
        /// <summary>«Astrazione → implementazione» (o «chiamante → metodo»), con la sintesi dei capi se c'è.</summary>
        public IReadOnlyList<string> Links { get; init; }
        /// <summary>Le righe intorno al punto, numerate.</summary>
        public string Excerpt { get; init; }
        public string FactHash { get; init; }
        public string PromptVersion { get; init; }
        /// <summary><c>missing</c> (mai descritto) o <c>changed</c> (riga, archi o domanda cambiati).</summary>
        public string Why { get; init; }
        public string Key => $"{Path}:{Line}";
    }

    /// <summary>
    /// La parte deterministica delle descrizioni dei collegamenti (D43, 30/10/2026, utente: «una domanda specifica per
    /// creare una descrizione associata ai nuovi grafi»): quali punti di una regola con <c>describe</c> vanno descritti,
    /// con quali impronte, che cosa chiedere all'LLM e come leggere la risposta. Gli archi metodo → metodo nascono dallo
    /// stesso punto della classe: un punto è UNA domanda, non una per arco.
    /// </summary>
    public static class CodeLinkDocs
    {
        /// <summary>Quante righe intorno al punto vanno all'LLM (sopra e sotto).</summary>
        public const int ContextLines = 4;

        /// <summary>I punti della regola (file:riga) con i loro archi, nell'ordine dei file.</summary>
        public static List<(string Path, int Line, List<ScriptEdge> Edges)> Points(CodeScript ruleScript)
            => ruleScript.Edges.Where(e => e.Path != null && e.Line != null)
                .GroupBy(e => (e.Path, Line: e.Line.Value))
                .OrderBy(g => g.Key.Path, StringComparer.Ordinal).ThenBy(g => g.Key.Line)
                .Select(g => (g.Key.Path, g.Key.Line, g.ToList())).ToList();

        /// <summary>L'impronta del fatto: la riga del sorgente (spazi compattati) e gli archi del punto.</summary>
        public static string FactHash(string lineText, IEnumerable<ScriptEdge> edges)
        {
            var text = Regex.Replace(lineText ?? string.Empty, @"\s+", " ").Trim() + "\n"
                + string.Join("\n", edges.Select(e => $"{e.SourceId}>{e.Relation}>{e.TargetId}").Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal));
            return CodeBodyHash.Hash(text);
        }

        /// <summary>La versione della domanda: cambia la domanda, la descrizione si rifà.</summary>
        public static string PromptVersion(string describe) => CodeBodyHash.Hash("link-doc-1\n" + describe.Trim());

        /// <summary>Le righe intorno al punto, numerate come nell'editor.</summary>
        public static string Excerpt(IReadOnlyList<string> lines, int line)
        {
            var from = Math.Max(1, line - ContextLines);
            var to = Math.Min(lines.Count, line + ContextLines);
            var sb = new StringBuilder();
            for (var i = from; i <= to; i++) sb.Append(i).Append(i == line ? " > " : "   ").Append(lines[i - 1]).Append('\n');
            return sb.ToString();
        }

        /// <summary>Da fare: senza descrizione, o con la riga, gli archi o la domanda cambiati. Null = a posto.</summary>
        public static string Why(ScriptLinkDoc existing, string factHash, string promptVersion)
            => existing == null ? "missing" : existing.FactHash != factHash || existing.PromptVersion != promptVersion ? "changed" : null;

        /// <summary>
        /// La domanda all'LLM: la regola (che cosa collega e perché), la domanda dell'utente/Mark, i punti con le righe e i
        /// collegamenti, la lingua e il formato. Solo quello che c'è nella richiesta: niente file da leggere.
        /// </summary>
        public static string BuildPrompt(CodeEnrichmentRule rule, IReadOnlyList<CodeLinkPoint> points, string language)
        {
            var languageName = language switch
            {
                "it" => "italiano",
                "en" => "inglese (English)",
                _ => throw new ArgumentException(UiText.Say($"Lingua delle descrizioni '{language}' non prevista: it oppure en.", $"Description language '{language}' not supported: it or en."), nameof(language)),
            };
            var sb = new StringBuilder();
            sb.Append("Descrivi dei collegamenti nel grafo del codice. Una regola del progetto li ha trovati nel sorgente:\n");
            sb.Append("REGOLA ").Append(rule.Id).Append(": ").Append(rule.Description.Trim()).Append("\n\n");
            sb.Append("DOMANDA, da rispondere per OGNI punto: ").Append(rule.Describe.Trim()).Append("\n\n");
            sb.Append("Rispondi solo con quello che si vede nelle righe date; se qualcosa non si vede, dillo invece di supporlo.\n");
            sb.Append("Lingua: ").Append(languageName).Append(". Una o due frasi per punto.\n\n");
            foreach (var p in points)
            {
                sb.Append("=== PUNTO ").Append(p.Key).Append(" ===\n");
                foreach (var l in p.Links) sb.Append("collegamento: ").Append(l).Append('\n');
                sb.Append(p.Excerpt).Append('\n');
            }
            sb.Append("Formato della risposta: SOLO righe JSON, una per punto, senza altro testo:\n");
            sb.Append("{\"point\": \"<file>:<riga> come sopra\", \"summary\": \"la risposta\"}\n");
            return sb.ToString();
        }

        /// <summary>Le righe JSON della risposta; un punto che non era nella domanda, o vuoto, si scarta e si dice.</summary>
        public static List<(string Key, string Summary)> ParseAnswer(string answer, IReadOnlyList<CodeLinkPoint> points, out List<string> rejected)
        {
            var asked = points.Select(p => p.Key).ToHashSet(StringComparer.Ordinal);
            var result = new List<(string, string)>();
            rejected = new List<string>();
            foreach (var raw in (answer ?? string.Empty).Replace("\r\n", "\n").Split('\n'))
            {
                var line = raw.Trim();
                if (!line.StartsWith("{", StringComparison.Ordinal)) continue;
                try
                {
                    using var doc = JsonDocument.Parse(line);
                    var point = doc.RootElement.TryGetProperty("point", out var pp) && pp.ValueKind == JsonValueKind.String ? pp.GetString() : null;
                    var summary = doc.RootElement.TryGetProperty("summary", out var ss) && ss.ValueKind == JsonValueKind.String ? ss.GetString()?.Trim() : null;
                    if (point == null || !asked.Contains(point)) { rejected.Add(UiText.Say($"punto «{point}» non chiesto", $"point «{point}» not asked")); continue; }
                    if (string.IsNullOrEmpty(summary)) { rejected.Add(UiText.Say($"{point}: risposta vuota", $"{point}: empty answer")); continue; }
                    result.Add((point, summary));
                }
                catch (JsonException ex) { rejected.Add(UiText.Say("riga JSON non valida: ", "invalid JSON line: ") + ex.Message); }
            }
            return result;
        }
    }
}
