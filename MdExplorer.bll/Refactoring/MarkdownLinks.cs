using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MdExplorer.Features.Refactoring
{
    /// <summary>Che tipo di link è: dice da dove si legge il percorso e come si riscrive.</summary>
    public enum MarkdownLinkKind
    {
        /// <summary><c>[t](p)</c>, <c>![t](p)</c>, <c>[r]: p</c>.</summary>
        Markdown,
        /// <summary><c>href="p"</c> o <c>src="p"</c> in un pezzo di HTML del documento.</summary>
        Html,
        /// <summary>Il file di <c>```text(p)</c>, <c>```html(p)</c>, <c>```plantuml(@json, p)</c>.</summary>
        IncludeCommand,
        /// <summary><c>[[p etichetta]]</c> dentro un diagramma PlantUML.</summary>
        PlantumlLink,
        /// <summary><c>&lt;img:p&gt;</c> dentro un diagramma PlantUML: PlantUML la cerca dalla cartella del documento.</summary>
        PlantumlImage,
        /// <summary><c>mdShowMd(p)</c>, <c>MDShowH2(p, …)</c>.</summary>
        MdShow,
    }

    /// <summary>Un link scritto in un markdown, come Markdig lo vede: dove sta la sua destinazione nel testo.</summary>
    public sealed class MarkdownLinkSpan
    {
        public MarkdownLinkKind Kind { get; init; } = MarkdownLinkKind.Markdown;
        /// <summary>Posizione della destinazione nel testo, parentesi angolari comprese se ci sono.</summary>
        public int Start { get; init; }
        public int Length { get; init; }
        /// <summary>Il testo della destinazione così com'è scritto (<c>docs/a.md#s</c>, <c>&lt;a b.md&gt;</c>).</summary>
        public string Raw { get; init; }
        /// <summary>La destinazione senza ancora né query, decodificata (<c>%20</c> → spazio): il percorso del bersaglio.</summary>
        public string Target { get; init; }
        /// <summary><c>#ancora</c> o <c>?query</c> da tenere com'è; vuoto se non c'è.</summary>
        public string Suffix { get; init; }
        public bool IsImage { get; init; }
        /// <summary>La definizione di un link stile riferimento (<c>[r]: docs/a.md</c>).</summary>
        public bool IsDefinition { get; init; }
        /// <summary>Il pezzo di markdown del link (per l'indice: <c>LinkInsideMarkdown.LinkedCommand</c>).</summary>
        public string Command { get; init; }
    }

    /// <summary>
    /// I link di un documento markdown letti con Markdig, il parser che fa la pagina (sprint 2026-11-25-Spostamento-File-E-Link).
    /// Le espressioni regolari di prima non vedevano un link in prima riga, mettevano titolo e parentesi angolari nel percorso,
    /// prendevano i link scritti come esempio nel codice; e la riscrittura era un <c>Replace</c> su tutto il testo, che
    /// cambiava anche il testo visibile e le copie nel codice. Qui ogni link ha la posizione della sua destinazione, e si
    /// riscrive solo quella.
    /// </summary>
    public static class MarkdownLinks
    {
        // Lo stesso di MarkdownCodeRegions (la pagina), più le posizioni precise.
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseYamlFrontMatter()
            .UsePreciseSourceLocation()
            .Build();

        /// <summary>I link e le immagini del documento, in ordine di posizione; i link stile riferimento una volta, sulla definizione.</summary>
        public static IReadOnlyList<MarkdownLinkSpan> Read(string markdown)
        {
            if (string.IsNullOrEmpty(markdown)) return Array.Empty<MarkdownLinkSpan>();
            var doc = Markdown.Parse(markdown, Pipeline);
            var result = new List<MarkdownLinkSpan>();
            var seen = new HashSet<int>();

            foreach (var d in doc.Descendants<LinkReferenceDefinition>())
            {
                var span = Make(markdown, d.UrlSpan.Start, d.UrlSpan.Length, false, true, d.Span);
                if (span != null && seen.Add(span.Start)) result.Add(span);
            }
            foreach (var l in doc.Descendants<LinkInline>())
            {
                if (l.IsAutoLink || l.Reference != null) continue;   // il riferimento sta sulla definizione
                var span = Make(markdown, l.UrlSpan.Start, l.UrlSpan.Length, l.IsImage, false, l.Span);
                if (span != null && seen.Add(span.Start)) result.Add(span);
            }
            return result.OrderBy(s => s.Start).ToList();
        }

        // ```text(p)  ```html(p)  ```plantuml(@json, p): la prima riga del blocco, come la leggono i comandi.
        private static readonly Regex IncludeHead = new(@"^[ \t]*(?:`{3,}|~{3,})[ \t]*(?:(?:text|html)\((?<p>[^)\r\n]+)\)|plantuml\(\s*@[A-Za-z]+\s*,\s*(?<p>[^)\r\n]+?)\s*\))",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex PlantumlHead = new(@"^[ \t]*(?:`{3,}|~{3,})[ \t]*plantuml\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex PlantumlLinkRx = new(@"\[\[\s*(?<p>[^\]\s{]+)", RegexOptions.Compiled);
        private static readonly Regex PlantumlImageRx = new(@"<img:(?<p>[^>{]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex HtmlAttributeRx = new(@"\b(?:href|src)\s*=\s*(?:""(?<p>[^""]*)""|'(?<p>[^']*)')", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex MdShowRx = new(@"(?:mdShowMd|MDShowH2)\((?<p>[^,)\r\n]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Tutti i link che portano a un file, non solo quelli markdown: i comandi che includono un file, i link e le
        /// immagini dei diagrammi PlantUML, gli attributi <c>href</c>/<c>src</c> dell'HTML, <c>mdShow</c>. Il codice
        /// d'esempio resta fuori, come per i link markdown. In ordine di posizione.
        /// </summary>
        public static IReadOnlyList<MarkdownLinkSpan> ReadAll(string markdown)
        {
            if (string.IsNullOrEmpty(markdown)) return Array.Empty<MarkdownLinkSpan>();
            var result = Read(markdown).ToList();
            var doc = Markdown.Parse(markdown, Pipeline);

            foreach (var block in doc.Descendants<FencedCodeBlock>())
            {
                var start = block.Span.Start;
                if (start < 0 || start >= markdown.Length) continue;
                var lineEnd = markdown.IndexOf('\n', start);
                if (lineEnd < 0) lineEnd = markdown.Length;
                var head = markdown.Substring(start, lineEnd - start);
                var inc = IncludeHead.Match(head);
                if (inc.Success)
                {
                    var g = inc.Groups["p"];
                    var raw = g.Value.TrimEnd();
                    result.Add(Plain(MarkdownLinkKind.IncludeCommand, start + g.Index, raw, head));
                    continue;
                }
                if (!PlantumlHead.IsMatch(head)) continue;
                var end = Math.Min(markdown.Length, block.Span.End + 1);
                var body = markdown.Substring(lineEnd, Math.Max(0, end - lineEnd));
                foreach (Match m in PlantumlLinkRx.Matches(body))
                    result.Add(Linkish(MarkdownLinkKind.PlantumlLink, lineEnd + m.Groups["p"].Index, m.Groups["p"].Value, m.Value));
                foreach (Match m in PlantumlImageRx.Matches(body))
                {
                    var raw = m.Groups["p"].Value.TrimEnd();
                    result.Add(Plain(MarkdownLinkKind.PlantumlImage, lineEnd + m.Groups["p"].Index, raw, m.Value));
                }
            }

            foreach (var html in doc.Descendants<HtmlBlock>().Select(b => b.Span)
                         .Concat(doc.Descendants<HtmlInline>().Select(i => i.Span)))
            {
                if (html.Start < 0 || html.End >= markdown.Length || html.Length <= 0) continue;
                var text = markdown.Substring(html.Start, html.Length);
                foreach (Match m in HtmlAttributeRx.Matches(text))
                {
                    var g = m.Groups["p"];
                    if (g.Length == 0) continue;
                    result.Add(Linkish(MarkdownLinkKind.Html, html.Start + g.Index, g.Value, m.Value));
                }
            }

            var code = MdExplorer.Features.Utilities.MarkdownCodeRegions.Of(markdown);
            foreach (Match m in MdShowRx.Matches(markdown))
            {
                var g = m.Groups["p"];
                if (code.IsCode(m.Index)) continue;
                var raw = g.Value.Trim();
                result.Add(Plain(MarkdownLinkKind.MdShow, g.Index + (g.Value.Length - g.Value.TrimStart().Length), raw, m.Value));
            }

            return result.Where(l => l != null).GroupBy(l => l.Start).Select(g => g.First()).OrderBy(l => l.Start).ToList();
        }

        /// <summary>Un percorso scritto com'è, senza ancora né codifica (comandi, immagini di PlantUML, mdShow).</summary>
        private static MarkdownLinkSpan Plain(MarkdownLinkKind kind, int start, string raw, string command)
            => string.IsNullOrWhiteSpace(raw) ? null : new MarkdownLinkSpan
            {
                Kind = kind, Start = start, Length = raw.Length, Raw = raw, Target = raw, Suffix = string.Empty,
                IsImage = kind == MarkdownLinkKind.PlantumlImage, Command = command,
            };

        /// <summary>Un percorso con ancora o query possibili e la codifica degli URL (HTML, link di PlantUML).</summary>
        private static MarkdownLinkSpan Linkish(MarkdownLinkKind kind, int start, string raw, string command)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var cut = raw.IndexOfAny(new[] { '#', '?' });
            var path = cut >= 0 ? raw[..cut] : raw;
            string target;
            try { target = Uri.UnescapeDataString(path); }
            catch (UriFormatException) { target = path; }
            return new MarkdownLinkSpan
            {
                Kind = kind, Start = start, Length = raw.Length, Raw = raw, Target = target,
                Suffix = cut >= 0 ? raw[cut..] : string.Empty, Command = command,
            };
        }

        private static MarkdownLinkSpan Make(string markdown, int start, int length, bool image, bool definition, SourceSpan whole)
        {
            if (length <= 0 || start < 0 || start + length > markdown.Length) return null;
            var raw = markdown.Substring(start, length);
            var core = raw.StartsWith('<') && raw.EndsWith('>') ? raw[1..^1] : raw;
            var cut = core.IndexOfAny(new[] { '#', '?' });
            var path = cut >= 0 ? core[..cut] : core;
            var suffix = cut >= 0 ? core[cut..] : string.Empty;
            string target;
            try { target = Uri.UnescapeDataString(path.Replace("\\", string.Empty)); }
            catch (UriFormatException) { target = path; }
            var command = whole.Length > 0 && whole.Start >= 0 && whole.Start + whole.Length <= markdown.Length
                ? markdown.Substring(whole.Start, whole.Length) : raw;
            return new MarkdownLinkSpan
            {
                Start = start, Length = length, Raw = raw, Target = target, Suffix = suffix,
                IsImage = image, IsDefinition = definition, Command = command,
            };
        }

        /// <summary>Un link che non porta a un file del progetto: web, posta, ancora sola, dati in riga.</summary>
        public static bool IsExternal(string target)
        {
            if (string.IsNullOrWhiteSpace(target)) return true;
            var t = target.Trim();
            if (t.StartsWith("//")) return true;
            var colon = t.IndexOf(':');
            // «http:», «mailto:», «data:»… ma non «C:\…» (una lettera sola è un'unità di Windows).
            return colon > 1 && t[..colon].All(c => char.IsLetterOrDigit(c) || c == '+' || c == '-' || c == '.');
        }

        /// <summary>
        /// Il file a cui porta un link: dalla radice del progetto se comincia con <c>/</c>, altrimenti dalla cartella del
        /// documento. Percorso completo con il separatore del sistema; null per i link che non portano a un file.
        /// </summary>
        public static string ResolveTarget(string projectRoot, string documentPath, string target)
        {
            if (IsExternal(target)) return null;
            var t = target.Replace('\\', '/');
            var cut = t.IndexOfAny(new[] { '#', '?' });
            if (cut >= 0) t = t[..cut];
            if (t.Length == 0) return null;
            var rel = t.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var baseDir = t.StartsWith('/') && !string.IsNullOrEmpty(projectRoot) ? projectRoot : Path.GetDirectoryName(documentPath) ?? string.Empty;
            return Path.GetFullPath(Path.Combine(baseDir, rel));
        }

        /// <summary>
        /// Il file a cui porta un link del suo tipo. Le immagini di PlantUML le cerca PlantUML dalla cartella del documento:
        /// per loro <c>/</c> è la radice del disco, non del progetto.
        /// </summary>
        public static string ResolveTarget(string projectRoot, string documentPath, MarkdownLinkSpan link)
        {
            if (link.Kind != MarkdownLinkKind.PlantumlImage) return ResolveTarget(projectRoot, documentPath, link.Target);
            if (IsExternal(link.Target) && !Path.IsPathRooted(link.Target)) return null;
            var t = link.Target.Trim();
            if (t.Length == 0) return null;
            return Path.IsPathRooted(t)
                ? Path.GetFullPath(t)
                : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(documentPath) ?? string.Empty, t.Replace('\\', '/').Replace('/', Path.DirectorySeparatorChar)));
        }

        /// <summary>
        /// Come scrivere nel documento <paramref name="documentPath"/> un link verso <paramref name="targetFullPath"/>:
        /// dalla radice se il link era dalla radice, altrimenti relativo alla cartella del documento, con le <c>/</c>.
        /// </summary>
        public static string WriteTarget(string projectRoot, string documentPath, string targetFullPath, bool fromRoot)
        {
            var from = fromRoot && !string.IsNullOrEmpty(projectRoot) ? projectRoot : Path.GetDirectoryName(documentPath);
            var rel = Path.GetRelativePath(from, targetFullPath).Replace(Path.DirectorySeparatorChar, '/');
            return fromRoot ? "/" + rel : rel;
        }

        /// <summary>
        /// Riscrive le destinazioni dei link per posizione: <paramref name="newTarget"/> riceve ogni link e dà il nuovo
        /// percorso (senza ancora: quella resta) o null per lasciarlo com'è. Il testo visibile, il titolo, le parentesi
        /// angolari e il codice restano come sono. Restituisce il testo e quanti link ha cambiato.
        /// </summary>
        public static (string Text, int Changed) Rewrite(string markdown, Func<MarkdownLinkSpan, string> newTarget)
        {
            var links = ReadAll(markdown);
            var sb = new StringBuilder(markdown);
            var changed = 0;
            foreach (var link in links.OrderByDescending(l => l.Start))
            {
                var path = newTarget(link);
                if (path == null) continue;
                var pointy = link.Kind == MarkdownLinkKind.Markdown && link.Raw.StartsWith('<') && link.Raw.EndsWith('>');
                var encoded = link.Kind is MarkdownLinkKind.Markdown or MarkdownLinkKind.Html or MarkdownLinkKind.PlantumlLink;
                var written = pointy || !encoded ? path : path.Replace(" ", "%20");
                var replacement = pointy ? "<" + written + link.Suffix + ">" : written + link.Suffix;
                if (replacement == link.Raw) continue;
                sb.Remove(link.Start, link.Length).Insert(link.Start, replacement);
                changed++;
            }
            return (sb.ToString(), changed);
        }

        /// <summary>
        /// Il caso generale (un documento, una cartella intera, più cose insieme): <paramref name="newLocationOf"/> dice
        /// dove è finito un percorso spostato, o null se non si è mosso. Il documento si legge come stava in
        /// <paramref name="documentOldPath"/> (i suoi link relativi erano relativi a quel posto) e si scrive per dove sta ora,
        /// <paramref name="documentPath"/>. Si cambia un link solo se il documento o il suo bersaglio si sono mossi.
        /// </summary>
        public static string RelinkForMoves(string markdown, string documentPath, string documentOldPath, string projectRoot,
            Func<string, string> newLocationOf, out int changed)
        {
            var sameAs = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var (text, count) = Rewrite(markdown, link =>
            {
                var target = ResolveTarget(projectRoot, documentOldPath, link);
                if (target == null) return null;
                var shouldReach = Path.GetFullPath(newLocationOf(target) ?? target);
                var written = link.Kind == MarkdownLinkKind.PlantumlImage
                    ? (Path.IsPathRooted(link.Target.Trim())
                        ? shouldReach
                        : Path.GetRelativePath(Path.GetDirectoryName(documentPath), shouldReach).Replace(Path.DirectorySeparatorChar, '/'))
                    : WriteTarget(projectRoot, documentPath, shouldReach, link.Target.StartsWith('/'));
                // Letto da dove sta ora il documento, il link porta già lì (due documenti spostati insieme, un link dalla
                // radice a un bersaglio fermo): resta scritto com'è, «./» compreso. Ma non un giro che esce e rientra
                // (`../archivio/assets/x.png` da dentro archivio), lasciato da uno spostamento precedente dello stesso gruppo.
                var moved = !string.Equals(documentPath, documentOldPath, sameAs) || newLocationOf(target) != null;
                if (string.Equals(ResolveTarget(projectRoot, documentPath, link), shouldReach, sameAs)
                    && !(moved && UpLevels(link.Target) > UpLevels(written)))
                    return null;
                return written;
            });
            changed = count;
            return text;
        }

        private static int UpLevels(string path)
            => (path ?? string.Empty).Replace('\\', '/').Split('/').Count(seg => seg == "..");

        /// <summary>I file a cui portano i link del documento (percorsi completi), di ogni tipo.</summary>
        public static IEnumerable<string> TargetsOf(string markdown, string documentPath, string projectRoot)
            => ReadAll(markdown).Select(l => ResolveTarget(projectRoot, documentPath, l)).Where(t => t != null);

        /// <summary>Dove è finito <paramref name="path"/> se stava dentro la cartella spostata; null se era fuori.</summary>
        public static string InsideMovedFolder(string path, string oldDir, string newDir)
        {
            var sameAs = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var oldPrefix = Path.GetFullPath(oldDir).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (string.Equals(path, oldPrefix.TrimEnd(Path.DirectorySeparatorChar), sameAs)) return Path.GetFullPath(newDir);
            return path.StartsWith(oldPrefix, sameAs)
                ? Path.Combine(Path.GetFullPath(newDir), path.Substring(oldPrefix.Length))
                : null;
        }
    }
}
