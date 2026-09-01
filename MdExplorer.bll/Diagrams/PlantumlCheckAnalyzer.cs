using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MdExplorer.Features.Commands;

namespace MdExplorer.Features.Diagrams
{
    /// <summary>
    /// Turns a raw <see cref="PlantumlCheckOutcome"/> plus the diagram source into the answer a
    /// small model can act on.
    /// <para>
    /// Two sources of truth, on purpose. The jar knows the grammar but its line number is a
    /// GUESS — a class left unclosed on line 2 is reported on line 1, i.e. on <c>@startuml</c> —
    /// so quoting "the offending line" from it would mislead. The structural checks here are
    /// fewer but exact, and they point at the right line. The jar stays as the backstop for
    /// everything they do not cover.
    /// </para>
    /// <para>
    /// The rest are checks only MdExplorer can make: for the jar those diagrams are flawless, but
    /// inside MdExplorer they do not show, or they show wrong.
    /// </para>
    /// <para>Contract: docs-internal/Sprints/2026-09-01-Plantuml-Check-Api-Mcp.md.</para>
    /// </summary>
    public static class PlantumlCheckAnalyzer
    {
        /// <summary>
        /// Past this many, a reader stops correcting and starts over. Most severe first.
        /// </summary>
        public const int MaxProblems = 10;

        private const string Error = "error";
        private const string Warning = "warning";
        private const string Hint = "hint";

        // "ERROR\n<riga>\n<messaggio>" — formato misurato sul jar 1.2026.1 in -checkonly.
        private static readonly Regex AssumedTypeRegex =
            new(@"Assumed diagram type:\s*(?<type>[a-z]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // #RRGGBB / #RGB usati come colore in PlantUML (riempimenti, skinparam, colori inline).
        private static readonly Regex HexColorRegex =
            new(@"#(?<hex>[0-9A-Fa-f]{6}|[0-9A-Fa-f]{3})\b", RegexOptions.Compiled);

        public static PlantumlCheckReport Analyze(string source, PlantumlCheckOutcome outcome)
        {
            var report = new PlantumlCheckReport();
            source ??= string.Empty;

            // Lo strumento non ha girato: il diagramma NON e' stato giudicato. Dirlo e fermarsi,
            // perche' un "non valido" qui manderebbe chi legge a correggere un sorgente sano.
            if (outcome?.ToolUnavailable != null)
            {
                report.Ok = false;
                report.ToolUnavailable = outcome.ToolUnavailable;
                return report;
            }

            var lines = source.Replace("\r\n", "\n").Split('\n');
            var problems = new List<PlantumlProblem>();

            AddBacktickProblem(lines, problems);
            AddUnbalancedBraceProblem(lines, problems);
            AddUnclosedIfProblem(lines, problems);
            AddColourProblems(lines, problems);
            AddThemeHint(source, problems);

            if (outcome != null && !outcome.Ok)
            {
                report.DiagramType = ExtractAssumedType(outcome.Stderr);
                AddJarProblem(lines, outcome, report.DiagramType, problems);
            }

            // D1: gli hint solo quando non ci sono errori. Prima farlo funzionare, poi farlo bello:
            // un muro di consigli di stile sopra un errore sposta l'attenzione sulla cosa sbagliata.
            var hasError = problems.Any(p => p.Severity == Error);
            if (hasError)
            {
                problems = problems.Where(p => p.Severity != Hint).ToList();
            }

            report.Problems = problems
                .OrderBy(p => p.Severity == Error ? 0 : p.Severity == Warning ? 1 : 2)
                .ThenBy(p => p.Line)
                .Take(MaxProblems)                                  // D2
                .ToList();
            report.Ok = !hasError;
            return report;
        }

        /// <summary>
        /// Il caso peggiore in assoluto: per il jar il diagramma e' perfetto, ma MdExplorer chiude
        /// il blocco al primo backtick e non mostra nulla, senza nessun messaggio. Chi scrive non
        /// ha modo di accorgersene.
        /// </summary>
        private static void AddBacktickProblem(string[] lines, List<PlantumlProblem> problems)
        {
            for (var i = 0; i < lines.Length; i++)
            {
                if (!lines[i].Contains('`')) continue;

                problems.Add(new PlantumlProblem
                {
                    Severity = Error,
                    Line = i + 1,
                    Source = lines[i].Trim(),
                    Message = "Backtick dentro il blocco.",
                    Meaning = "MdExplorer riconosce il blocco con un'espressione che si ferma al primo backtick: "
                              + "il diagramma verrebbe troncato qui e non comparirebbe affatto, senza nessun errore.",
                    Fix = "Togli i backtick: per evidenziare un identificatore usa il corsivo di PlantUML (//testo//) o le virgolette."
                });
                return;   // uno basta: la causa e' la stessa per tutti
            }
        }

        /// <summary>
        /// Graffa aperta e mai chiusa (corpo di class/entity). Il jar la segnala su @startuml,
        /// che non aiuta; qui si sa la riga esatta dell'apertura rimasta scoperta.
        /// </summary>
        private static void AddUnbalancedBraceProblem(string[] lines, List<PlantumlProblem> problems)
        {
            var openStack = new Stack<int>();
            for (var i = 0; i < lines.Length; i++)
            {
                foreach (var ch in lines[i])
                {
                    if (ch == '{') openStack.Push(i);
                    else if (ch == '}' && openStack.Count > 0) openStack.Pop();
                }
            }
            if (openStack.Count == 0) return;

            var lineIndex = openStack.Last();   // la piu' esterna rimasta aperta
            problems.Add(new PlantumlProblem
            {
                Severity = Error,
                Line = lineIndex + 1,
                Source = lines[lineIndex].Trim(),
                Message = "Graffa aperta e mai chiusa.",
                Meaning = "Il corpo aperto qui non viene mai chiuso, quindi PlantUML non riesce a leggere il diagramma. "
                          + "Il messaggio del jar in questi casi indica la prima riga, non questa.",
                Fix = "Chiudi il blocco con } prima di @enduml."
            });
        }

        /// <summary>Activity diagram: <c>if</c> senza il corrispondente <c>endif</c>.</summary>
        private static void AddUnclosedIfProblem(string[] lines, List<PlantumlProblem> problems)
        {
            var openIf = new Stack<int>();
            for (var i = 0; i < lines.Length; i++)
            {
                var t = lines[i].TrimStart();
                if (t.StartsWith("if ", StringComparison.OrdinalIgnoreCase) || t.StartsWith("if("))
                    openIf.Push(i);
                else if (t.StartsWith("endif", StringComparison.OrdinalIgnoreCase) && openIf.Count > 0)
                    openIf.Pop();
            }
            if (openIf.Count == 0) return;

            var lineIndex = openIf.Last();
            problems.Add(new PlantumlProblem
            {
                Severity = Error,
                Line = lineIndex + 1,
                Source = lines[lineIndex].Trim(),
                Message = "Blocco if senza endif.",
                Meaning = "La condizione aperta qui non viene mai chiusa: PlantUML non riconosce piu' il tipo di diagramma "
                          + "e lo rifiuta.",
                Fix = "Aggiungi endif dopo l'ultimo ramo."
            });
        }

        /// <summary>
        /// I due modi di sbagliare i colori che si vedono solo dentro MdExplorer, dove in tema
        /// scuro l'SVG passa per <c>invert(0.88) hue-rotate(180deg)</c>.
        /// </summary>
        private static void AddColourProblems(string[] lines, List<PlantumlProblem> problems)
        {
            var greys = new List<(int Line, string Hex)>();

            for (var i = 0; i < lines.Length; i++)
            {
                foreach (Match m in HexColorRegex.Matches(lines[i]))
                {
                    var hex = Expand(m.Groups["hex"].Value).ToUpperInvariant();

                    if (hex == "FFFFFF")
                    {
                        problems.Add(new PlantumlProblem
                        {
                            Severity = Warning,
                            Line = i + 1,
                            Source = lines[i].Trim(),
                            Message = "Riempimento bianco puro.",
                            Meaning = "In tema scuro MdExplorer inverte il diagramma e #FFFFFF diventa #1F1F1F, "
                                      + "cioe' esattamente lo sfondo: l'elemento sparisce.",
                            Fix = "Usa un grigio pallido come #F1F3F4 al posto del bianco."
                        });
                        continue;
                    }

                    // Acromatico = R, G e B uguali: e' un colore che porta SOLO luminosita'.
                    if (hex[0] == hex[2] && hex[2] == hex[4] && hex[1] == hex[3] && hex[3] == hex[5])
                    {
                        greys.Add((i + 1, hex));
                    }
                }
            }

            // Due o piu' grigi DIVERSI: il significato e' affidato al chiaro/scuro, che si ribalta.
            var distinctGreys = greys.Select(g => g.Hex).Distinct().ToList();
            if (distinctGreys.Count >= 2)
            {
                var first = greys[0];
                problems.Add(new PlantumlProblem
                {
                    Severity = Warning,
                    Line = first.Line,
                    Source = lines[first.Line - 1].Trim(),
                    Message = $"Il diagramma distingue elementi usando {distinctGreys.Count} grigi diversi.",
                    Meaning = "In tema scuro la luminosita' si ribalta mentre la tinta sopravvive: due grigi che oggi "
                              + "dicono 'questo si' e 'questo no' domani si leggono al contrario.",
                    Fix = "Distingui con la tinta invece che col chiaro/scuro, per esempio #E6F4EA contro #FCE8E6."
                });
            }
        }

        private static void AddThemeHint(string source, List<PlantumlProblem> problems)
        {
            if (source.Contains("!theme", StringComparison.OrdinalIgnoreCase)) return;

            problems.Add(new PlantumlProblem
            {
                Severity = Hint,
                Line = 0,
                Message = "Nessun tema dichiarato.",
                Meaning = "Senza una riga !theme decidono i colori di default di PlantUML, che cambiano fra versioni del jar.",
                Fix = "Aggiungi !theme plain subito dopo @startuml."
            });
        }

        /// <summary>
        /// Il verdetto del jar. La riga che indica e' un'approssimazione, quindi <c>Source</c> si
        /// riporta solo quando indica qualcosa di utile: su <c>@startuml</c> confonderebbe.
        /// </summary>
        private static void AddJarProblem(string[] lines, PlantumlCheckOutcome outcome, string assumedType,
            List<PlantumlProblem> problems)
        {
            var parts = (outcome.Stderr ?? string.Empty)
                .Replace("\r\n", "\n")
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .ToList();

            var line = 0;
            if (parts.Count >= 2) int.TryParse(parts[1], out line);
            var message = parts.Count >= 3 ? string.Join(" ", parts.Skip(2)) : "PlantUML ha rifiutato il diagramma.";

            string sourceLine = null;
            if (line >= 1 && line <= lines.Length)
            {
                var candidate = lines[line - 1].Trim();
                var isFrame = candidate.StartsWith("@start", StringComparison.OrdinalIgnoreCase)
                              || candidate.StartsWith("@end", StringComparison.OrdinalIgnoreCase)
                              || candidate.Length == 0;
                if (!isFrame) sourceLine = candidate;
            }

            var meaning = "PlantUML non e' riuscito a leggere il diagramma.";
            if (!string.IsNullOrEmpty(assumedType))
            {
                meaning += $" Ha provato a interpretarlo come '{assumedType}': se non e' il tipo che volevi, "
                           + "l'errore e' prima di questa riga, dove il tipo si sarebbe dovuto riconoscere.";
            }
            if (sourceLine == null)
            {
                meaning += " La riga indicata e' approssimativa: guarda le righe successive a quella.";
            }

            problems.Add(new PlantumlProblem
            {
                Severity = Error,
                Line = line,
                Source = sourceLine,
                Message = message,
                Meaning = meaning,
                Fix = "Correggi la sintassi alla riga indicata, poi richiama la verifica."
            });
        }

        private static string ExtractAssumedType(string stderr)
        {
            if (string.IsNullOrEmpty(stderr)) return null;
            var m = AssumedTypeRegex.Match(stderr);
            return m.Success ? m.Groups["type"].Value.ToLowerInvariant() : null;
        }

        private static string Expand(string hex)
            => hex.Length == 3
                ? string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2])
                : hex;
    }
}
