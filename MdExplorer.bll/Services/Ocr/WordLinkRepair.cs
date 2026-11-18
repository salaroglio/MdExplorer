using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using MdExplorer.Features.Services.SourceMapping;

namespace MdExplorer.Features.Services.Ocr
{
    /// <summary>
    /// Rimette in piedi l'indice e i rimandi interni di un Word convertito.
    /// <para>
    /// In Word un rimando punta a un segnalibro (<c>_Toc…</c>, <c>_Ref…</c>); in markdown un titolo si
    /// raggiunge con l'ancora che il rendering gli dà (<c>#introduzione</c>). Qui ogni rimando viene
    /// portato sull'ancora vera del suo titolo, chiesta allo stesso Markdig che disegna la pagina; i
    /// segnalibri che non stanno in un titolo restano come ancore esplicite. L'indice di Word diventa
    /// un elenco annidato, senza i numeri di pagina.
    /// </para>
    /// <para>
    /// Si lavora in due tempi attorno alla conversione HTML → markdown: <see cref="PrepareHtml"/> lascia
    /// dei segnaposto nell'HTML, <see cref="FinishMarkdown"/> li scioglie nel markdown.
    /// </para>
    /// </summary>
    public sealed class WordLinkRepair
    {
        private const string TocClass = "mde-toc-";
        private const string TocHeadingClass = "mde-toc-heading";

        private static readonly Regex HeadingToken = new Regex(@"MDEHEAD\d{4}X", RegexOptions.Compiled);
        private static readonly Regex HeadingTokenAsText = new Regex(@"(?<!#)MDEHEAD\d{4}X", RegexOptions.Compiled);
        private static readonly Regex HeadingTokenAsLink = new Regex(@"#(MDEHEAD\d{4}X)", RegexOptions.Compiled);
        private static readonly Regex AnchorToken = new Regex(@"MDEANCHOR\d{4}X", RegexOptions.Compiled);
        private static readonly Regex PageNumber = new Regex(@"^\s*([0-9]+|[ivxlcdm]+)?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly List<string> _warnings;
        // segnaposto di un titolo → il primo segnalibro di Word che gli stava dentro (per dire quale rimando è rimasto appeso)
        private readonly Dictionary<string, string> _bookmarkOfHeadingToken = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _bookmarkOfAnchorToken = new Dictionary<string, string>();

        public WordLinkRepair(List<string> warnings)
        {
            _warnings = warnings;
        }

        /// <summary>
        /// Le righe di stile che fanno riconoscere a Mammoth i paragrafi dell'indice. I nomi sono quelli
        /// che Word scrive nel file, uguali in ogni lingua dell'interfaccia.
        /// </summary>
        public static string StyleMap
        {
            get
            {
                var map = new StringBuilder();
                map.Append("p[style-name='TOC Heading'] => p.").Append(TocHeadingClass).Append(":fresh\n");
                for (var level = 1; level <= 9; level++)
                {
                    map.Append("p[style-name='toc ").Append(level).Append("'] => p.").Append(TocClass).Append(level).Append(":fresh\n");
                }
                return map.ToString();
            }
        }

        public string PrepareHtml(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            var root = document.DocumentNode;

            var links = (root.SelectNodes("//a[@href]") ?? Enumerable.Empty<HtmlNode>())
                .Where(link => link.GetAttributeValue("href", string.Empty).StartsWith("#", StringComparison.Ordinal))
                .ToList();
            var referenced = new HashSet<string>(links.Select(Target));

            var known = new HashSet<string>();
            var tokenOfHeading = new Dictionary<HtmlNode, string>();
            var headingTokenOfBookmark = new Dictionary<string, string>();
            foreach (var anchor in (root.SelectNodes("//a[@id and not(@href)]") ?? Enumerable.Empty<HtmlNode>()).ToList())
            {
                var bookmark = HtmlEntity.DeEntitize(anchor.GetAttributeValue("id", string.Empty));
                known.Add(bookmark);
                var heading = anchor.Ancestors().FirstOrDefault(IsHeading);
                if (referenced.Contains(bookmark) && heading == null)
                {
                    var token = $"MDEANCHOR{_bookmarkOfAnchorToken.Count + 1:D4}X";
                    _bookmarkOfAnchorToken[token] = bookmark;
                    anchor.ParentNode.InsertBefore(document.CreateTextNode(token), anchor);
                }
                else if (referenced.Contains(bookmark))
                {
                    if (!tokenOfHeading.TryGetValue(heading, out var token))
                    {
                        token = $"MDEHEAD{tokenOfHeading.Count + 1:D4}X";
                        tokenOfHeading[heading] = token;
                        _bookmarkOfHeadingToken[token] = bookmark;
                    }
                    headingTokenOfBookmark[bookmark] = token;
                }
                anchor.ParentNode.RemoveChild(anchor, true);
            }
            foreach (var heading in tokenOfHeading)
            {
                heading.Key.PrependChild(document.CreateTextNode(heading.Value));
            }

            var missing = new HashSet<string>();
            foreach (var link in links)
            {
                var bookmark = Target(link);
                if (headingTokenOfBookmark.TryGetValue(bookmark, out var token))
                {
                    link.SetAttributeValue("href", "#" + token);
                }
                else if (!known.Contains(bookmark))
                {
                    missing.Add(bookmark);
                }
            }
            // Un avviso solo, con i nomi: uno per rimando erano decine di righe uguali.
            if (missing.Count > 0)
            {
                _warnings.Add(UiText.Say(
                    $"{missing.Count} rimandi puntano a segnalibri che nel documento non ci sono ({Some(missing)}): i link restano ma non portano da nessuna parte.",
                    $"{missing.Count} cross-references point to bookmarks that are not in the document ({Some(missing)}): the links stay but lead nowhere."));
            }

            RebuildIndex(root);
            return root.OuterHtml;
        }

        public string FinishMarkdown(string markdown)
        {
            var pipeline = DocumentViewPipeline.Build(null);

            // A quale titolo del markdown è arrivato ogni segnaposto (un titolo dentro una tabella non lo è più).
            var tokenOfHeading = Markdown.Parse(markdown, pipeline).Descendants<HeadingBlock>()
                .Select(heading => HeadingToken.Match(markdown.Substring(heading.Span.Start, heading.Span.Length)))
                .Select(match => match.Success ? match.Value : null)
                .ToList();

            var clean = HeadingTokenAsText.Replace(markdown, string.Empty);
            var headings = Markdown.Parse(clean, pipeline).Descendants<HeadingBlock>().ToList();
            if (headings.Count != tokenOfHeading.Count)
            {
                throw new InvalidOperationException(
                    $"WordLinkRepair: {tokenOfHeading.Count} headings before removing the placeholders, {headings.Count} after.");
            }

            var anchorOfToken = new Dictionary<string, string>();
            for (var i = 0; i < headings.Count; i++)
            {
                var id = headings[i].TryGetAttributes()?.Id;
                if (tokenOfHeading[i] != null && !string.IsNullOrEmpty(id))
                {
                    anchorOfToken[tokenOfHeading[i]] = id;
                }
            }

            var unresolved = new HashSet<string>();
            clean = HeadingTokenAsLink.Replace(clean, match =>
            {
                var token = match.Groups[1].Value;
                if (anchorOfToken.TryGetValue(token, out var anchor))
                {
                    return "#" + anchor;
                }
                var bookmark = _bookmarkOfHeadingToken[token];
                unresolved.Add(bookmark);
                return "#" + bookmark;
            });

            if (unresolved.Count > 0)
            {
                _warnings.Add(UiText.Say(
                    $"{unresolved.Count} rimandi puntano a titoli che in markdown non sono più titoli, per esempio perché stanno in una tabella (segnalibri {Some(unresolved)}): i link restano ma non portano da nessuna parte.",
                    $"{unresolved.Count} cross-references point to headings that are no longer headings in markdown, for example because they sit in a table (bookmarks {Some(unresolved)}): the links stay but lead nowhere."));
            }

            return AnchorToken.Replace(clean, match => $"<a id=\"{_bookmarkOfAnchorToken[match.Value]}\"></a>");
        }

        // I primi nomi, e quanti ne restano: bastano a ritrovarli nel Word.
        private static string Some(ICollection<string> names)
        {
            const int shown = 5;
            var first = string.Join(", ", names.Take(shown).Select(name => "«" + name + "»"));
            return names.Count <= shown ? first : first + UiText.Say($" e altri {names.Count - shown}", $" and {names.Count - shown} more");
        }

        // I paragrafi dell'indice di Word, che arrivano uno per voce con il numero di pagina in coda,
        // diventano un elenco annidato secondo il livello della voce.
        private static void RebuildIndex(HtmlNode root)
        {
            foreach (var title in (root.SelectNodes($"//p[@class='{TocHeadingClass}']") ?? Enumerable.Empty<HtmlNode>()).ToList())
            {
                title.ParentNode.ReplaceChild(HtmlNode.CreateNode($"<p><strong>{title.InnerHtml}</strong></p>"), title);
            }

            var entries = (root.SelectNodes($"//p[starts-with(@class,'{TocClass}')]") ?? Enumerable.Empty<HtmlNode>()).ToList();
            var position = 0;
            while (position < entries.Count)
            {
                var run = new List<HtmlNode> { entries[position++] };
                while (position < entries.Count && NextElement(run[run.Count - 1]) == entries[position])
                {
                    run.Add(entries[position++]);
                }

                var list = new StringBuilder();
                var open = new Stack<int>();
                foreach (var entry in run)
                {
                    DropPageNumber(entry);
                    if (string.IsNullOrWhiteSpace(entry.InnerText))
                    {
                        continue;
                    }
                    var level = int.Parse(entry.GetAttributeValue("class", string.Empty).Substring(TocClass.Length));
                    while (open.Count > 0 && open.Peek() > level)
                    {
                        list.Append("</li></ul>");
                        open.Pop();
                    }
                    if (open.Count > 0 && open.Peek() == level)
                    {
                        list.Append("</li><li>");
                    }
                    else
                    {
                        list.Append("<ul><li>");
                        open.Push(level);
                    }
                    list.Append(entry.InnerHtml);
                }
                while (open.Count > 0)
                {
                    list.Append("</li></ul>");
                    open.Pop();
                }

                if (list.Length > 0)
                {
                    run[0].ParentNode.InsertBefore(HtmlNode.CreateNode(list.ToString()), run[0]);
                }
                foreach (var entry in run)
                {
                    entry.Remove();
                }
            }
        }

        // «1.1 ⇥ Scopo ⇥ 3»: l'ultima tabulazione separa il numero di pagina, le altre diventano spazi.
        private static void DropPageNumber(HtmlNode entry)
        {
            var texts = entry.Descendants().OfType<HtmlTextNode>().ToList();
            var last = texts.FindLastIndex(text => text.Text.Contains("\t"));
            if (last >= 0)
            {
                var tab = texts[last].Text.LastIndexOf('\t');
                var tail = texts[last].Text.Substring(tab + 1) + string.Concat(texts.Skip(last + 1).Select(text => text.Text));
                if (PageNumber.IsMatch(HtmlEntity.DeEntitize(tail)))
                {
                    texts[last].Text = texts[last].Text.Substring(0, tab);
                    foreach (var text in texts.Skip(last + 1))
                    {
                        text.Text = string.Empty;
                    }
                }
            }
            foreach (var text in texts)
            {
                text.Text = text.Text.Replace('\t', ' ');
            }
        }

        private static HtmlNode NextElement(HtmlNode node)
        {
            var next = node.NextSibling;
            while (next != null && next.NodeType == HtmlNodeType.Text && string.IsNullOrWhiteSpace(next.InnerText))
            {
                next = next.NextSibling;
            }
            return next;
        }

        private static string Target(HtmlNode link)
        {
            return HtmlEntity.DeEntitize(link.GetAttributeValue("href", string.Empty)).Substring(1);
        }

        private static bool IsHeading(HtmlNode node)
        {
            return node.Name.Length == 2 && node.Name[0] == 'h' && node.Name[1] >= '1' && node.Name[1] <= '6';
        }
    }
}
