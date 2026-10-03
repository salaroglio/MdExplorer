using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

namespace MdExplorer.Features.Tests.Slides
{
    /// <summary>Reads a page written by SlideDeckRenderer, for the tests.</summary>
    internal static class SlidePage
    {
        public static HtmlNode Slides(string page)
        {
            var document = new HtmlDocument { OptionOutputOriginalCase = true };
            document.LoadHtml(page);
            return document.DocumentNode.SelectSingleNode("//div[@class='slides']");
        }

        /// <summary>The top-level sections: a slide, or a stack of vertical slides.</summary>
        public static HtmlNode[] Sections(string page)
            => Slides(page).ChildNodes.Where(n => n.Name == "section").ToArray();

        /// <summary>The object handed to Reveal.initialize.</summary>
        public static JsonObject Config(string page)
        {
            const string start = "Reveal.initialize(Object.assign(";
            var from = page.IndexOf(start, StringComparison.Ordinal) + start.Length;
            var to = page.IndexOf(", { plugins:", from, StringComparison.Ordinal);
            return (JsonObject)JsonNode.Parse(page.Substring(from, to - from));
        }

        /// <summary>The repository's root, found from the test's output folder.</summary>
        public static string RepositoryRoot()
        {
            var root = new DirectoryInfo(AppContext.BaseDirectory);
            while (root != null && !File.Exists(Path.Combine(root.FullName, "MdExplorer.sln")))
            {
                root = root.Parent;
            }
            return root?.FullName ?? throw new InvalidOperationException("repository root not found");
        }
    }
}
