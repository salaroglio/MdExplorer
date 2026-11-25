using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MdExplorer.Features.Refactoring
{
    /// <summary>Un link scritto in un markdown, come Markdig lo vede: dove sta la sua destinazione nel testo.</summary>
    public sealed class MarkdownLinkSpan
    {
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
            var links = Read(markdown);
            var sb = new StringBuilder(markdown);
            var changed = 0;
            foreach (var link in links.OrderByDescending(l => l.Start))
            {
                var path = newTarget(link);
                if (path == null) continue;
                var pointy = link.Raw.StartsWith('<') && link.Raw.EndsWith('>');
                var written = pointy ? path : path.Replace(" ", "%20");
                var replacement = pointy ? "<" + written + link.Suffix + ">" : written + link.Suffix;
                if (replacement == link.Raw) continue;
                sb.Remove(link.Start, link.Length).Insert(link.Start, replacement);
                changed++;
            }
            return (sb.ToString(), changed);
        }

        /// <summary>
        /// I link da cambiare quando un documento si sposta da <paramref name="oldPath"/> a <paramref name="newPath"/>: per
        /// il documento spostato, ogni link relativo che puntava altrove torna a puntarci dalla cartella nuova; per gli
        /// altri, ogni link che puntava al vecchio posto porta al nuovo. I link dalla radice restano dalla radice.
        /// </summary>
        public static string RelinkForMove(string markdown, string documentPath, string projectRoot, string oldPath, string newPath,
            bool isMovedDocument, out int changed)
        {
            var sameAs = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            // Il documento spostato si legge come stava nel posto vecchio: i suoi link relativi erano relativi a quello.
            var readAs = isMovedDocument ? oldPath : documentPath;
            var (text, count) = Rewrite(markdown, link =>
            {
                var target = ResolveTarget(projectRoot, readAs, link.Target);
                if (target == null) return null;
                var fromRoot = link.Target.StartsWith('/');
                var pointsToMoved = string.Equals(target, Path.GetFullPath(oldPath), sameAs);
                if (pointsToMoved) return WriteTarget(projectRoot, documentPath, newPath, fromRoot);
                if (!isMovedDocument || fromRoot) return null;
                return WriteTarget(projectRoot, documentPath, target, false);
            });
            changed = count;
            return text;
        }
    }
}
