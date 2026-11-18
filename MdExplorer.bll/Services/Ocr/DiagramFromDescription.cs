using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MdExplorer.Features.Utilities;

namespace MdExplorer.Features.Services.Ocr
{
    /// <summary>
    /// Il secondo tentativo su uno schema che dall'immagine non si è riusciti a ridisegnare: il modello non guarda
    /// più l'immagine, corregge la descrizione con il testo del documento e disegna da lì (utente, 18/11/2026).
    /// Oltre alla descrizione corretta e al diagramma dichiara gli elementi e i collegamenti: è con quelli che
    /// MdExplorer confronta il diagramma (<see cref="DiagramFidelity"/>).
    /// </summary>
    public sealed class RedrawVerdict
    {
        private static readonly Regex PlantumlFence = new Regex(@"```+\s*plantuml\s*\n(.*?)\n\s*```+", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private static readonly Regex Marker = new Regex(@"^[ \t]*(DESCRIPTION|ELEMENTS|RELATIONS|DOUBTS):[ \t]*", RegexOptions.Multiline);

        public string Description { get; set; }
        public List<string> Elements { get; } = new List<string>();
        public List<(string From, string To)> Relations { get; } = new List<(string From, string To)>();

        /// <summary>Dove la descrizione e il documento non vanno d'accordo, o che cosa resta incerto: lo dice il modello.</summary>
        public List<string> Doubts { get; } = new List<string>();

        /// <summary>Il diagramma, da <c>@start…</c> a <c>@end…</c>; null se nemmeno dalla descrizione si disegna.</summary>
        public string PlantUml { get; set; }

        /// <summary>Legge la risposta nel formato chiesto dal prompt <c>mde-doc-tidy-redraw</c>.</summary>
        /// <exception cref="FormatException">La risposta non ha il formato chiesto: dice che cosa manca.</exception>
        public static RedrawVerdict Parse(string answer)
        {
            answer = (answer ?? string.Empty).Replace("\r\n", "\n");
            var verdict = new RedrawVerdict();

            var fence = PlantumlFence.Match(answer);
            if (fence.Success)
            {
                var source = fence.Groups[1].Value.Trim();
                if (!source.StartsWith("@start", StringComparison.Ordinal) || !source.Contains("@end"))
                {
                    throw new FormatException("il blocco plantuml deve andare da @startuml a @enduml");
                }
                if (source.Contains("```"))
                {
                    throw new FormatException("il diagramma contiene tre backtick di fila");
                }
                verdict.PlantUml = source;
                answer = answer.Remove(fence.Index, fence.Length);
            }

            var parts = new Dictionary<string, string>();
            var markers = Marker.Matches(answer);
            for (var i = 0; i < markers.Count; i++)
            {
                var start = markers[i].Index + markers[i].Length;
                var end = i + 1 < markers.Count ? markers[i + 1].Index : answer.Length;
                parts[markers[i].Groups[1].Value] = answer.Substring(start, end - start).Trim();
            }

            verdict.Description = parts.TryGetValue("DESCRIPTION", out var description) ? description : string.Empty;
            if (verdict.Description.Length == 0)
            {
                throw new FormatException("manca la parte DESCRIPTION:");
            }
            if (verdict.Description.IndexOf("</details", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                throw new FormatException("la DESCRIPTION contiene un </details>");
            }
            if (Regex.IsMatch(verdict.Description, @"^\s*```", RegexOptions.Multiline))
            {
                throw new FormatException("la DESCRIPTION contiene un blocco di codice: deve essere solo testo");
            }

            verdict.Elements.AddRange(Items(parts, "ELEMENTS"));
            if (verdict.Elements.Count == 0)
            {
                throw new FormatException("manca l'elenco ELEMENTS: (una riga «- nome» per elemento)");
            }
            foreach (var relation in Items(parts, "RELATIONS"))
            {
                var ends = relation.Split(new[] { "->" }, 2, StringSplitOptions.None);
                if (ends.Length != 2 || ends[0].Trim().Length == 0 || ends[1].Trim().Length == 0)
                {
                    throw new FormatException($"in RELATIONS la riga «{relation}» non è «- A -> B»");
                }
                verdict.Relations.Add((ends[0].Trim(), ends[1].Trim()));
            }
            verdict.Doubts.AddRange(Items(parts, "DOUBTS").Select(doubt => Regex.Replace(doubt, @"\s+", " ")));
            return verdict;
        }

        // Le righe «- voce» di una parte; «none» da sola vuol dire nessuna.
        private static IEnumerable<string> Items(Dictionary<string, string> parts, string name)
        {
            if (!parts.TryGetValue(name, out var text))
            {
                yield break;
            }
            foreach (var line in text.Split('\n'))
            {
                var item = line.Trim().TrimStart('-', '*').Trim().Trim('`').Trim();
                if (item.Length > 0 && !item.Equals("none", StringComparison.OrdinalIgnoreCase))
                {
                    yield return item;
                }
            }
        }
    }

    /// <summary>
    /// Che cosa di un diagramma disegnato da una descrizione si può controllare senza modello: sono confronti di
    /// NOMI. Dicono se nel diagramma c'è un nome che né la descrizione né il documento portano, e se un elemento
    /// o un collegamento dichiarato non c'è. Non dicono che il diagramma è giusto: per questo un problema qui è
    /// un avviso a chi legge, e il diagramma resta.
    /// <para>
    /// Dei collegamenti si controlla che i due capi stiano sulla stessa riga del diagramma, non il verso. Nei
    /// diagrammi dove i collegamenti non stanno su una riga (attività, mappe mentali) non si controllano.
    /// </para>
    /// </summary>
    public static class DiagramFidelity
    {
        private const string Identifier = @"[\p{L}\p{N}_][\p{L}\p{N}_.]*";

        private static readonly Regex Quoted = new Regex("\"([^\"\\n]+)\"");
        private static readonly Regex Bracketed = new Regex(@"\[([^\[\]\n]+)\]");
        private static readonly Regex Declaration = new Regex(
            @"^(?:abstract\s+class|participant|actor|boundary|control|entity|database|collections|queue|class|interface|enum|object|component|rectangle|node|cloud|folder|frame|package|usecase|state|artifact|storage|agent|card|file|person|system|container)\s+(?<rest>.+)$",
            RegexOptions.IgnoreCase);
        private static readonly Regex AliasAfter = new Regex(@"\sas\s+(?<alias>" + Identifier + ")");
        private static readonly Regex AliasBefore = new Regex(@"(?<alias>" + Identifier + @")\s+as\s+""");
        private const string Heads = @"[<>*#|/\\}{]";
        private const string Shaft = @"[-.=~]+(?:\[[^\]\n]*\])?(?:up|down|left|right|u|d|l|r)?[-.=~]*";
        private static readonly Regex Arrow = new Regex(@"\s*(?:" + Heads + "+" + Shaft + Heads + "*|" + Shaft + Heads + @"+|[-=~]{2,}|\.{2,})\s*");
        private static readonly Regex Parenthesized = new Regex(@"\(([^()\n]+)\)");
        private static readonly Regex FreeText = new Regex(@"^(?:skinparam|title|caption|header|footer|legend|note|end\s*note|hide|show|scale|left\s+to|top\s+to|autonumber|activate|deactivate|return|!|')", RegexOptions.IgnoreCase);
        private static readonly Regex MindmapNode = new Regex(@"^[*+\-]+(?:\[#[^\]]*\])?_?\s+(?<text>.+)$");

        /// <returns>Che cosa non torna, una voce per problema; vuoto se i confronti passano tutti.</returns>
        public static List<string> Check(string plantUml, IReadOnlyList<string> elements, IReadOnlyList<(string From, string To)> relations,
            string description, string document)
        {
            var problems = new List<string>();
            var lines = plantUml.Replace("\r\n", "\n").Split('\n').Select(line => line.Trim()).Where(line => line.Length > 0).ToList();
            var (names, aliases) = Names(lines);

            var known = Padded(description + "\n" + document + "\n" + string.Join("\n", elements));
            var invented = names.Where(name => !known.Contains(" " + Normalize(name) + " ")).Distinct().ToList();
            if (invented.Count > 0)
            {
                problems.Add(UiText.Say(
                    $"nello schema ci sono nomi che né la descrizione né il documento portano: {Join(invented)}",
                    $"the diagram has names that neither the description nor the document carry: {Join(invented)}"));
            }

            var drawn = Padded(plantUml);
            var missing = elements.Where(element => !drawn.Contains(" " + Normalize(element) + " ")).Distinct().ToList();
            if (missing.Count > 0)
            {
                problems.Add(UiText.Say($"nello schema mancano: {Join(missing)}", $"the diagram lacks: {Join(missing)}"));
            }

            // Le righe che nominano un elemento, per nome o per sigla.
            bool Mentions(string line, string element)
            {
                var wanted = Normalize(element);
                var padded = Padded(line);
                return padded.Contains(" " + wanted + " ")
                    || aliases.Any(alias => Normalize(alias.Value) == wanted && padded.Contains(" " + Normalize(alias.Key) + " "));
            }
            var linking = lines.Where(line => !FreeText.IsMatch(line) && Arrow.IsMatch(line)).ToList();
            var present = elements.Where(element => !missing.Contains(element)).ToList();
            if (linking.Any(line => present.Count(element => Mentions(line, element)) >= 2))
            {
                var lost = relations
                    .Where(relation => !missing.Contains(relation.From) && !missing.Contains(relation.To))
                    .Where(relation => !linking.Any(line => Mentions(line, relation.From) && Mentions(line, relation.To)))
                    .Select(relation => relation.From + " → " + relation.To).Distinct().ToList();
                if (lost.Count > 0)
                {
                    problems.Add(UiText.Say($"nello schema mancano i collegamenti: {Join(lost)}", $"the diagram lacks the connections: {Join(lost)}"));
                }
            }
            return problems;
        }

        // I nomi che il diagramma mostra e le sigle con cui li richiama («"Nome lungo" as NL»).
        private static (List<string> Names, Dictionary<string, string> Aliases) Names(List<string> lines)
        {
            var names = new List<string>();
            var aliases = new Dictionary<string, string>(StringComparer.Ordinal);
            var mindmap = lines.Count > 0 && Regex.IsMatch(lines[0], @"^@start(mindmap|wbs)", RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                if (line.StartsWith("@", StringComparison.Ordinal) || FreeText.IsMatch(line))
                {
                    continue;
                }
                if (mindmap)
                {
                    var node = MindmapNode.Match(line);
                    if (node.Success)
                    {
                        names.Add(node.Groups["text"].Value);
                    }
                    continue;
                }
                if (line.StartsWith(":", StringComparison.Ordinal))
                {
                    // Un passo di un diagramma di attività: «:testo;».
                    names.Add(line.Substring(1).TrimEnd(';', '|', '<', '>', '/', ']', '}').Trim());
                    continue;
                }

                // Dopo i due punti c'è l'etichetta di una freccia o il corpo di un campo: testo libero.
                var colon = line.IndexOf(" : ", StringComparison.Ordinal);
                var head = colon >= 0 ? line.Substring(0, colon) : line;

                var quoted = Quoted.Matches(head).Cast<Match>().Select(match => match.Groups[1].Value).ToList();
                names.AddRange(quoted);
                var bare = Quoted.Replace(head, " ");
                names.AddRange(Bracketed.Matches(Arrow.Replace(bare, " ")).Cast<Match>().Select(match => match.Groups[1].Value));

                // Le sigle: «"Nome lungo" as NL», «[Componente] as C», «(Caso d'uso) as UC», «NL as "Nome lungo"».
                var declaration = Declaration.Match(head);
                var rest = declaration.Success ? declaration.Groups["rest"].Value : head;
                var first = declaration.Success ? Regex.Match(rest, "^(?:" + Identifier + ")") : Match.Empty;
                var shown = quoted.Count > 0 ? quoted[0]
                    : Bracketed.IsMatch(rest) ? Bracketed.Match(rest).Groups[1].Value
                    : Parenthesized.IsMatch(rest) ? Parenthesized.Match(rest).Groups[1].Value
                    : first.Success ? first.Value : null;
                var before = AliasBefore.Match(rest);
                var after = AliasAfter.Match(Quoted.Replace(rest, "\"\""));
                if (before.Success && quoted.Count > 0)
                {
                    aliases[before.Groups["alias"].Value] = quoted[0];
                    continue;
                }
                if (after.Success && shown != null)
                {
                    aliases[after.Groups["alias"].Value] = shown;
                }
                if (declaration.Success || after.Success)
                {
                    if (quoted.Count == 0 && first.Success)
                    {
                        names.Add(first.Value);
                    }
                    continue;
                }

                // «A -> B»: i due capi, quando non sono sigle già dichiarate.
                var withoutBrackets = Bracketed.Replace(bare, " ");
                var ends = Arrow.Split(withoutBrackets).Select(end => end.Trim()).Where(end => end.Length > 0).ToList();
                if (ends.Count >= 2 && Arrow.IsMatch(withoutBrackets))
                {
                    names.AddRange(ends.Where(end => Regex.IsMatch(end, "^" + Identifier + "$")));
                }
            }

            // Numeri e molteplicità («1», «0..*») non sono nomi; una sigla dichiarata nemmeno.
            var shownNames = names
                .Select(name => name.Replace("\\n", " ").Trim())
                .Where(name => name.Any(char.IsLetter) && Normalize(name).Length > 1 && !aliases.ContainsKey(name))
                .ToList();
            return (shownNames, aliases);
        }

        private static string Padded(string text) => " " + Normalize(text) + " ";

        // Minuscole, e tutto ciò che non è lettera o cifra diventa uno spazio: «Gestione-Ordini» e «gestione ordini» sono lo stesso nome.
        internal static string Normalize(string text)
        {
            var builder = new StringBuilder();
            var space = false;
            foreach (var c in (text ?? string.Empty).Replace("\\n", " "))
            {
                if (char.IsLetterOrDigit(c))
                {
                    if (space && builder.Length > 0)
                    {
                        builder.Append(' ');
                    }
                    builder.Append(char.ToLowerInvariant(c));
                    space = false;
                }
                else
                {
                    space = true;
                }
            }
            return builder.ToString();
        }

        private static string Join(IEnumerable<string> items) => string.Join(", ", items.Select(item => "«" + Regex.Replace(item, @"\s+", " ") + "»"));
    }

    /// <summary>Il pezzo di documento che serve a capire un'immagine: i titoli di tutto il documento e il capitolo che la contiene.</summary>
    public static class DocumentContext
    {
        /// <summary>Oltre questa misura del capitolo si passa solo il tratto intorno all'immagine, e lo si dice.</summary>
        public const int MaxSectionChars = 12000;

        private static readonly Regex Heading = new Regex(@"^(#{1,6})[ \t]+\S.*$", RegexOptions.Multiline);

        public static (string Outline, string Section) Of(string markdown, DocumentImage image)
        {
            var code = MarkdownCodeRegions.Of(markdown);
            var headings = Heading.Matches(markdown).Cast<Match>().Where(match => !code.IsCode(match.Index)).ToList();

            var own = headings.LastOrDefault(heading => heading.Index < image.Start);
            var start = own?.Index ?? 0;
            var level = own?.Groups[1].Length ?? 0;
            var next = headings.FirstOrDefault(heading => heading.Index > image.Start && heading.Groups[1].Length <= level);
            var end = next?.Index ?? markdown.Length;

            var cut = false;
            if (end - start > MaxSectionChars)
            {
                cut = true;
                start = Math.Max(start, image.Start - MaxSectionChars / 2);
                end = Math.Min(end, image.Start + image.Length + MaxSectionChars / 2);
            }
            var section = markdown.Substring(start, image.Start - start) + "[THE IMAGE IS HERE]"
                + markdown.Substring(image.Start + image.Length, end - image.Start - image.Length);
            if (cut)
            {
                section = "(the section is long: this is only the part around the image)\n" + section;
            }
            return (string.Join("\n", headings.Select(heading => heading.Value.TrimEnd())), section.Trim());
        }
    }
}
