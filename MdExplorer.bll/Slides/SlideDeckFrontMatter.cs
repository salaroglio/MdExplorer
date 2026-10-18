using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace MdExplorer.Features.Slides
{
    /// <summary>What the front matter of a slide deck asks for.</summary>
    public sealed class SlideDeckSettings
    {
        public string Title { get; init; }

        /// <summary>A reveal.js theme, one of <see cref="SlideDeckFrontMatter.Themes"/>.</summary>
        public string Theme { get; init; }

        /// <summary>A highlight.js style shipped with reveal.js, one of <see cref="SlideDeckFrontMatter.HighlightThemes"/>.</summary>
        public string HighlightTheme { get; init; }

        /// <summary><c>reveal.config</c> as written, for <c>Reveal.initialize</c>.</summary>
        public JsonObject Config { get; init; }
    }

    /// <summary>
    /// Reads the front matter of a slide deck: <c>title</c> and the <c>reveal:</c> block.
    ///
    /// <para>
    /// <c>reveal.config</c> is handed to <c>Reveal.initialize</c> as it is, with no list of allowed
    /// options: an option reveal.js adds tomorrow works without touching MdExplorer. Its scalars are
    /// typed as YAML reads them (<c>controls: false</c> is the boolean false, <c>'false'</c> the
    /// string): YamlDotNet's untyped deserialization turns every scalar into a string, and the
    /// string "false" is true for reveal.js.
    /// </para>
    /// </summary>
    public static class SlideDeckFrontMatter
    {
        /// <summary>The themes in <c>wwwroot/reveal/dist/theme</c> (reveal.js 6.0.2).</summary>
        public static readonly IReadOnlyList<string> Themes = new[]
        {
            "beige", "black", "black-contrast", "blood", "dracula", "league", "moon", "night",
            "serif", "simple", "sky", "solarized", "white", "white-contrast",
        };

        /// <summary>The styles in <c>wwwroot/reveal/dist/plugin/highlight</c>.</summary>
        public static readonly IReadOnlyList<string> HighlightThemes = new[] { "monokai", "zenburn" };

        private static readonly string[] RevealKeys = { "theme", "highlight_theme", "config" };

        private static readonly MarkdownPipeline FrontMatterPipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .Build();

        /// <summary>
        /// The settings, and the markdown after the front matter. The theme defaults to the one that
        /// matches MdExplorer's: <c>black</c> on a dark theme, <c>white</c> otherwise.
        /// </summary>
        public static (SlideDeckSettings Settings, string Body) Read(string markdown, bool darkTheme)
        {
            var document = Markdown.Parse(markdown ?? string.Empty, FrontMatterPipeline);
            var frontMatter = document.OfType<YamlFrontMatterBlock>().FirstOrDefault();
            if (frontMatter == null)
            {
                throw new SlideDeckException(
                    "A slide deck starts with a front matter between two '---' lines, holding 'document_type: slides'.");
            }

            var body = BodyAfter(markdown, frontMatter);
            var root = Parse(frontMatter.Lines.ToString());
            var reveal = root != null && root.Children.TryGetValue(new YamlScalarNode("reveal"), out var node) ? node : null;

            return (new SlideDeckSettings
            {
                Title = root != null && root.Children.TryGetValue(new YamlScalarNode("title"), out var title)
                    ? (title as YamlScalarNode)?.Value
                    : null,
                Theme = ReadChoice(reveal, "theme", Themes, darkTheme ? "black" : "white"),
                HighlightTheme = ReadChoice(reveal, "highlight_theme", HighlightThemes, "monokai"),
                Config = ReadConfig(reveal),
            }, body);
        }

        private static string BodyAfter(string markdown, YamlFrontMatterBlock frontMatter)
        {
            // The span ends on the closing '---': the body starts on the next line.
            var endOfLine = markdown.IndexOf('\n', frontMatter.Span.End);
            return endOfLine < 0 ? string.Empty : markdown.Substring(endOfLine + 1);
        }

        private static YamlMappingNode Parse(string yaml)
        {
            if (string.IsNullOrWhiteSpace(yaml))
            {
                return null;
            }
            try
            {
                var stream = new YamlStream();
                stream.Load(new StringReader(yaml));
                return stream.Documents.Count > 0 ? stream.Documents[0].RootNode as YamlMappingNode : null;
            }
            catch (YamlException ex)
            {
                // Mark.Line counts the front matter's lines from 1; the file has '---' above them.
                throw new SlideDeckException(
                    $"The front matter is not valid YAML (line {ex.Start.Line + 1} of the file): {ex.Message}", ex);
            }
        }

        private static YamlMappingNode RevealBlock(YamlNode reveal)
        {
            if (reveal == null)
            {
                return null;
            }
            if (reveal is not YamlMappingNode mapping)
            {
                throw new SlideDeckException("'reveal:' must be a block of keys: theme, highlight_theme, config.");
            }
            foreach (var key in mapping.Children.Keys.OfType<YamlScalarNode>())
            {
                if (!RevealKeys.Contains(key.Value))
                {
                    // The usual slip: a reveal.js option written one level too high.
                    throw new SlideDeckException(
                        $"Unknown key 'reveal.{key.Value}'. 'reveal:' accepts theme, highlight_theme and config; " +
                        $"the options of reveal.js go under 'reveal.config' (for example 'reveal.config.{key.Value}').");
                }
            }
            return mapping;
        }

        private static string ReadChoice(YamlNode reveal, string key, IReadOnlyList<string> allowed, string byDefault)
        {
            var mapping = RevealBlock(reveal);
            if (mapping == null || !mapping.Children.TryGetValue(new YamlScalarNode(key), out var node))
            {
                return byDefault;
            }
            var value = (node as YamlScalarNode)?.Value;
            if (value == null || !allowed.Contains(value))
            {
                throw new SlideDeckException(
                    $"'reveal.{key}: {value}' does not exist. Choose one of: {string.Join(", ", allowed)}.");
            }
            return value;
        }

        private static JsonObject ReadConfig(YamlNode reveal)
        {
            var mapping = RevealBlock(reveal);
            if (mapping == null || !mapping.Children.TryGetValue(new YamlScalarNode("config"), out var node))
            {
                return new JsonObject();
            }
            if (node is not YamlMappingNode config)
            {
                throw new SlideDeckException("'reveal.config' must be a block of reveal.js options, one per line.");
            }
            return (JsonObject)ToJson(config);
        }

        private static JsonNode ToJson(YamlNode node)
        {
            switch (node)
            {
                case YamlMappingNode mapping:
                    var obj = new JsonObject();
                    foreach (var (key, value) in mapping.Children)
                    {
                        obj[((YamlScalarNode)key).Value] = ToJson(value);
                    }
                    return obj;
                case YamlSequenceNode sequence:
                    return new JsonArray(sequence.Children.Select(ToJson).ToArray());
                case YamlScalarNode scalar:
                    return ToJson(scalar);
                default:
                    throw new SlideDeckException($"'reveal.config' contains a YAML construct MdExplorer cannot read: {node}.");
            }
        }

        private static readonly Regex Integer = new(@"^[-+]?[0-9]+$", RegexOptions.Compiled);
        private static readonly Regex Float = new(@"^[-+]?(\.[0-9]+|[0-9]+(\.[0-9]*)?)([eE][-+]?[0-9]+)?$", RegexOptions.Compiled);

        /// <summary>A scalar typed as YAML 1.2's core schema does: only a plain (unquoted) scalar can be a boolean, a number or null.</summary>
        private static JsonNode ToJson(YamlScalarNode scalar)
        {
            var text = scalar.Value ?? string.Empty;
            if (scalar.Style != ScalarStyle.Plain)
            {
                return JsonValue.Create(text);
            }
            switch (text)
            {
                case "" or "~" or "null" or "Null" or "NULL":
                    return null;
                case "true" or "True" or "TRUE":
                    return JsonValue.Create(true);
                case "false" or "False" or "FALSE":
                    return JsonValue.Create(false);
            }
            if (Integer.IsMatch(text) && long.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var integer))
            {
                return JsonValue.Create(integer);
            }
            if (Float.IsMatch(text) && double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var real))
            {
                return JsonValue.Create(real);
            }
            return JsonValue.Create(text);
        }
    }
}
