using HtmlAgilityPack;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Slides
{
    /// <summary>
    /// Makes the files a slide loads by itself reachable. The page is served at
    /// <c>/api/mdexplorer/&lt;file&gt;.md?connectionId=…</c>, and the service finds a project's file
    /// only with the connectionId: a relative <c>src="img/a.png"</c> written in HTML, or a
    /// <c>data-background-image</c>, reached the service without it and got a 404 (measured: every
    /// image of an <c>r-stack</c> and the slide background). MdExplorer's commands add it to markdown
    /// images only. Here the page's query (<c>connectionId=…</c>, plus <c>agent=…</c> for a worktree)
    /// is added to every relative URL; the browser still resolves the path against the markdown file.
    /// </summary>
    internal static class SlideResources
    {
        /// <summary>Attributes that load a file (reveal.js's own ones included).</summary>
        private static readonly string[] UrlAttributes =
        {
            "src", "data-src", "poster", "href",
            "data-background-image", "data-background-iframe",
        };

        /// <summary>A comma-separated list of URLs (reveal.js: several formats of the same video).</summary>
        private const string VideoAttribute = "data-background-video";

        private static readonly Regex Scheme = new(@"^[a-zA-Z][a-zA-Z0-9+.\-]*:", RegexOptions.Compiled);

        /// <summary>Whether <paramref name="html"/> may hold such a URL: most slides do not, and are left untouched.</summary>
        public static bool AnyIn(string html)
            => UrlAttributes.Append(VideoAttribute).Any(a => html.Contains(a + "=", StringComparison.OrdinalIgnoreCase));

        /// <summary>Adds <paramref name="query"/> to the relative URLs of <paramref name="section"/> and of what it contains.</summary>
        public static void Rewrite(HtmlNode section, string query)
        {
            foreach (var node in section.DescendantsAndSelf().Where(n => n.NodeType == HtmlNodeType.Element))
            {
                foreach (var name in UrlAttributes)
                {
                    var value = node.GetAttributeValue(name, null);
                    if (IsRelative(value))
                    {
                        node.SetAttributeValue(name, WithQuery(value, query));
                    }
                }
                var videos = node.GetAttributeValue(VideoAttribute, null);
                if (videos != null)
                {
                    node.SetAttributeValue(VideoAttribute, string.Join(",",
                        videos.Split(',').Select(v => v.Trim()).Select(v => IsRelative(v) ? WithQuery(v, query) : v)));
                }
            }
        }

        /// <summary>
        /// A path the browser resolves against the page: not absolute, not a scheme (http:, data:,
        /// mailto:…), not an anchor, and not already carrying the connectionId (markdown images
        /// and links, which MdExplorer's commands have rewritten).
        /// </summary>
        private static bool IsRelative(string url)
            => !string.IsNullOrWhiteSpace(url)
               && !url.StartsWith("/")
               && !url.StartsWith("#")
               && !Scheme.IsMatch(url)
               && url.IndexOf("connectionId=", StringComparison.OrdinalIgnoreCase) < 0;

        private static string WithQuery(string url, string query)
        {
            var hash = url.IndexOf('#');
            var path = hash < 0 ? url : url.Substring(0, hash);
            var fragment = hash < 0 ? string.Empty : url.Substring(hash);
            return path + (path.Contains('?') ? "&" : "?") + query + fragment;
        }
    }
}
