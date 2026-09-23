using HtmlAgilityPack;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Slides
{
    /// <summary>How to render a slide deck.</summary>
    public sealed class SlideDeckRenderOptions
    {
        /// <summary>The Markdig pipeline of the document view: slides and documents read markdown the same way.</summary>
        public MarkdownPipeline Pipeline { get; init; }

        /// <summary>MdExplorer shows a dark theme: the default reveal.js theme is then <c>black</c>.</summary>
        public bool DarkTheme { get; init; }

        /// <summary>MdExplorer's commands on the markdown, run once on the whole body before it is cut into slides.</summary>
        public Func<string, string> BeforeMarkdown { get; init; } = markdown => markdown;

        /// <summary>MdExplorer's commands on the HTML, run on each slide and on each speaker note.</summary>
        public Func<string, string> AfterMarkdown { get; init; } = html => html;
    }

    /// <summary>
    /// Turns a markdown slide deck into a reveal.js page. The markdown is read by MdExplorer's own
    /// Markdig pipeline, not by reveal.js's markdown plugin: one markdown dialect for documents and
    /// slides, with PlantUML, images and MdExplorer's commands. The rest follows reveal.js's
    /// documented conventions (separators, notes, attribute comments, line numbers), so reveal.js's
    /// documentation stays true for MdExplorer.
    /// </summary>
    public static class SlideDeckRenderer
    {
        /// <summary>The plugins' globals, in the order their scripts are loaded.</summary>
        private static readonly string[] Plugins = { "highlight", "notes", "math", "search", "zoom" };

        // reveal.js 6.0.2, plugin/markdown: what may follow the language of a fence.
        private static readonly Regex LineNumbers = new(@"\[\s*((\d*):)?\s*([\s\d,|-]*)\]", RegexOptions.Compiled);

        public static string Render(string markdown, SlideDeckRenderOptions options)
        {
            if (options?.Pipeline == null)
            {
                throw new ArgumentException("A Markdig pipeline is required.", nameof(options));
            }

            var (settings, body) = SlideDeckFrontMatter.Read(markdown, options.DarkTheme);
            var slides = new StringBuilder();
            foreach (var stack in SlideSplitter.Split(options.BeforeMarkdown(body)))
            {
                if (stack.Count == 1)
                {
                    slides.Append(RenderSlide(stack[0], options));
                }
                else
                {
                    slides.Append("<section>");
                    foreach (var slide in stack)
                    {
                        slides.Append(RenderSlide(slide, options));
                    }
                    slides.Append("</section>");
                }
                slides.Append('\n');
            }

            return Page(settings, slides.ToString());
        }

        private static string RenderSlide(SlideSource slide, SlideDeckRenderOptions options)
        {
            var content = options.AfterMarkdown(ToHtml(slide.Markdown, options.Pipeline));
            var notes = slide.Notes == null
                ? string.Empty
                : $"<aside class=\"notes\">{options.AfterMarkdown(ToHtml(slide.Notes, options.Pipeline))}</aside>";

            if (!SlideAttributes.AnyIn(content))
            {
                return $"<section>{content}{notes}</section>";
            }

            var document = new HtmlDocument { OptionOutputOriginalCase = true };
            document.LoadHtml("<section>" + content + "</section>");
            var section = document.DocumentNode.SelectSingleNode("section");
            SlideAttributes.Apply(section);
            var attributes = string.Concat(section.Attributes.Select(a =>
                $" {a.OriginalName}=\"{WebUtility.HtmlEncode(a.Value)}\""));
            return $"<section{attributes}>{section.InnerHtml}{notes}</section>";
        }

        private static string ToHtml(string markdown, MarkdownPipeline pipeline)
        {
            var document = Markdown.Parse(MarkCommentsAfterLists(markdown, pipeline), pipeline);

            foreach (var code in document.Descendants<FencedCodeBlock>())
            {
                if (code.Info == "mermaid")
                {
                    // Markdig would write <pre class="mermaid">, and the slide page loads no mermaid.
                    throw new SlideDeckException(
                        "Mermaid diagrams are not shown in slides: draw the diagram with PlantUML (```plantuml), which becomes an SVG in the slide.");
                }
                ApplyLineNumbers(code);
            }

            using var writer = new StringWriter();
            var renderer = new HtmlRenderer(writer);
            pipeline.Setup(renderer);
            renderer.Render(document);
            writer.Flush();
            return writer.ToString();
        }

        /// <summary>
        /// reveal.js gives an attribute comment written on the line after a list to the list's last
        /// item, and to the whole list after a blank line. The HTML keeps no blank lines: the
        /// comment is marked here, where the markdown still has them.
        /// </summary>
        private static string MarkCommentsAfterLists(string markdown, MarkdownPipeline pipeline)
        {
            var document = Markdown.Parse(markdown, pipeline);
            var lines = markdown.Split('\n');
            var inserts = new List<int>();
            foreach (var comment in document.Descendants<HtmlBlock>())
            {
                if (comment.Type != HtmlBlockType.Comment || comment.Parent == null)
                {
                    continue;
                }
                var index = comment.Parent.IndexOf(comment);
                if (index <= 0 || comment.Parent[index - 1] is not ListBlock list)
                {
                    continue;
                }
                // Not the list's span: Markdig counts a trailing blank line as part of the list.
                if (comment.Line > 0 && !string.IsNullOrWhiteSpace(lines[comment.Line - 1]))
                {
                    inserts.Add(comment.Span.Start + "<!--".Length);
                }
            }
            if (inserts.Count == 0)
            {
                return markdown;
            }
            var marked = new StringBuilder(markdown);
            foreach (var position in inserts.OrderByDescending(p => p))
            {
                marked.Insert(position, SlideAttributes.AfterListItemMarker);
            }
            return marked.ToString();
        }

        /// <summary><c>```js [1-2|3]</c> → <c>data-line-numbers="1-2|3"</c>, <c>[5: 1-2]</c> also <c>data-ln-start-from="5"</c>, as reveal.js's markdown plugin does.</summary>
        private static void ApplyLineNumbers(FencedCodeBlock code)
        {
            var info = string.Join(" ", new[] { code.Info, code.Arguments }.Where(s => !string.IsNullOrWhiteSpace(s)));
            var match = LineNumbers.Match(info);
            if (!match.Success)
            {
                return;
            }
            var attributes = code.GetAttributes();
            attributes.AddProperty("data-line-numbers", match.Groups[3].Value.Trim());
            if (match.Groups[2].Value.Length > 0)
            {
                attributes.AddProperty("data-ln-start-from", match.Groups[2].Value.Trim());
            }
            var language = info.Remove(match.Index, match.Length).Trim();
            code.Info = language.Split(' ', 2)[0];
            code.Arguments = language.Contains(' ') ? language.Split(' ', 2)[1] : null;
        }

        private static string Page(SlideDeckSettings settings, string slides)
        {
            var title = WebUtility.HtmlEncode(settings.Title ?? "Slides");
            var scripts = string.Concat(Plugins.Select(p => $"<script src=\"/reveal/dist/plugin/{p}.js\"></script>\n"));
            return $@"<!DOCTYPE html>
<html>
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
<meta name=""mdexplorer-view"" content=""slides"">
<title>{title}</title>
<link rel=""stylesheet"" href=""/reveal/dist/reset.css"">
<link rel=""stylesheet"" href=""/reveal/dist/reveal.css"">
<link rel=""stylesheet"" href=""/reveal/dist/theme/{settings.Theme}.css"">
<link rel=""stylesheet"" href=""/reveal/dist/plugin/highlight/{settings.HighlightTheme}.css"">
</head>
<body>
<div class=""reveal""><div class=""slides"">
{slides}</div></div>
<script src=""/reveal/dist/reveal.js""></script>
{scripts}<script>
Reveal.initialize(Object.assign({Configuration(settings.Config).ToJsonString()}, {{ plugins: [RevealHighlight, RevealNotes, RevealMath.KaTeX, RevealSearch, RevealZoom] }}));
</script>
</body>
</html>
";
        }

        /// <summary>
        /// MdExplorer's defaults, then <c>reveal.config</c> over them. Two things stay MdExplorer's:
        /// the plugins (functions, which YAML cannot write) and where KaTeX comes from, which is
        /// <c>wwwroot/katex</c>: without <c>local</c> the math plugin downloads KaTeX from a CDN.
        /// </summary>
        public static JsonObject Configuration(JsonObject config)
        {
            var result = new JsonObject
            {
                ["hash"] = true,
                // reveal.js 5+ turns the deck into a scrolling page below 435 px, and MdExplorer's
                // pane can be that narrow (measured: a 400 px iframe switches, 23/09/2026).
                ["scrollActivationWidth"] = null,
            };
            foreach (var (key, value) in config ?? new JsonObject())
            {
                if (key == "plugins")
                {
                    throw new SlideDeckException(
                        "'reveal.config.plugins' is set by MdExplorer (highlight, notes, math, search, zoom): remove it.");
                }
                result[key] = value?.DeepClone();
            }

            if (result["katex"] is not (JsonObject or null))
            {
                throw new SlideDeckException("'reveal.config.katex' must be a block of KaTeX options.");
            }
            var katex = (JsonObject)result["katex"] ?? new JsonObject();
            if (katex.ContainsKey("local"))
            {
                throw new SlideDeckException(
                    "'reveal.config.katex.local' is set by MdExplorer, which ships KaTeX: remove it.");
            }
            katex["local"] = "/katex";
            if (!katex.ContainsKey("delimiters"))
            {
                // What Markdig writes for $…$ and $$…$$: a '$' in the text stays a '$'.
                katex["delimiters"] = new JsonArray(
                    new JsonObject { ["left"] = "\\(", ["right"] = "\\)", ["display"] = false },
                    new JsonObject { ["left"] = "\\[", ["right"] = "\\]", ["display"] = true });
            }
            result["katex"] = katex;
            return result;
        }
    }
}
