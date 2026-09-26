using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Configuration.Models;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace MdExplorer.Features.Commands
{
    /// <summary>
    /// Expands <c>```plantuml(@json, ./path/file.json)</c> blocks into a plain <c>```plantuml</c>
    /// block holding <c>@startjson</c> + the content of the external file, so that the whole
    /// existing PlantUML pipeline (SVG generation, hash cache, interactive highlight, export)
    /// takes it from there untouched. <c>@yaml</c> works the same way with @startyaml.
    ///
    /// Priority is BELOW FromPlantumlToSvg/FromPlantumlToPng (20) on purpose: their regex
    /// (```plantuml followed by anything up to the closing fence) would otherwise swallow the
    /// block and hand "(@json, ./path.json)" to PlantUML as if it were diagram source.
    ///
    /// The body of the block, when present, is injected as extra PlantUML directives
    /// (#highlight, &lt;style&gt;, ...) right before the data: the document keeps what
    /// *comments* the diagram, the file keeps the data.
    /// </summary>
    public class FromExternalDataToPlantuml : CommandBase, ICommand
    {
        protected readonly ILogger<FromExternalDataToPlantuml> _logger;

        private const int MaxExternalFileSizeBytes = 512_000; // 500 KB — same as the other include commands

        // Directive inside the parens → the PlantUML block that renders that kind of data.
        private static readonly Dictionary<string, (string Open, string Close)> DirectiveToBlock =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["json"] = ("@startjson", "@endjson"),
                ["yaml"] = ("@startyaml", "@endyaml"),
            };

        public bool Enabled { get; set; } = true;
        public int Priority { get; set; } = 14; // before FromHtmlCodeBlockToPreview (15) and, above all, before PlantUML (20)
        public string Name { get; set; } = "FromExternalDataToPlantuml";

        public override List<CompatibilityMode> SupportedModes => new List<CompatibilityMode>
        {
            CompatibilityMode.MdExplorer,
            CompatibilityMode.CommonMark,
            CompatibilityMode.GitHub
        };

        public FromExternalDataToPlantuml(ILogger<FromExternalDataToPlantuml> logger)
        {
            _logger = logger;
        }

        public MatchCollection GetMatches(string markdown)
        {
            // ```plantuml(@json, ./file.json)  [optional body]  ```
            // The directive is captured loosely (@ + word) so that a wrong one gets a readable
            // message instead of falling through to PlantUML as diagram source.
            Regex rx = new Regex(@"```plantuml\(\s*@([A-Za-z]+)\s*,\s*([^)\r\n]+?)\s*\)[ \t]*\r?\n([\s\S]*?)```",
                                 RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return rx.Matches(markdown);
        }

        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var matches = GetMatches(markdown);
            if (matches.Count == 0) return markdown;

            var regions = MarkdownCodeRegions.Of(markdown);
            var currentIncrement = 0;
            foreach (Match match in matches)
            {
                // The syntax shown as an example inside a ```` block (as the skill that documents
                // it does) is text: expanding it drew an error box, or a diagram, inside the example.
                if (regions.IsCode(match.Index))
                {
                    continue;
                }

                string replacement;
                try
                {
                    replacement = BuildPlantumlBlock(match, requestInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[FromExternalDataToPlantuml] Error expanding block: {Block}", match.Groups[0].Value);
                    replacement = ErrorBlock(match, $"errore inatteso: {ex.Message}");
                }

                (markdown, currentIncrement) = ManageReplaceOnMD(markdown, currentIncrement, match, replacement);
            }

            return markdown;
        }

        public string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            // Nothing to do: by this point the block is an ordinary PlantUML diagram,
            // already handled by the PlantUML commands.
            return html;
        }

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            return markdown;
        }

        /// <summary>
        /// Turns one matched block into the ```plantuml block to put in its place, or into a
        /// visible error block. Never returns the original text unchanged: leaving it there
        /// would let the PlantUML command render "(@json, ...)" as if it were a diagram.
        /// </summary>
        private string BuildPlantumlBlock(Match match, RequestInfo requestInfo)
        {
            var directive = match.Groups[1].Value;
            var fileName = match.Groups[2].Value;
            var extraDirectives = match.Groups[3].Value;

            if (!DirectiveToBlock.TryGetValue(directive, out var block))
            {
                var supported = string.Join(", ", DirectiveToBlock.Keys.Select(_ => "@" + _));
                return ErrorBlock(match, $"direttiva @{directive} sconosciuta — quelle disponibili sono {supported}");
            }

            var fileContent = ReadExternalFile(fileName, requestInfo, out var absolutePath, out var readError);
            if (fileContent == null)
            {
                return ErrorBlock(match, readError);
            }

            // A backtick anywhere inside would close the fence early: the PlantUML command reads
            // ```plantuml up to the FIRST backtick, so the diagram would silently come out truncated.
            if (fileContent.IndexOf('`') > -1 || extraDirectives.IndexOf('`') > -1)
            {
                return ErrorBlock(match, "il contenuto include un backtick (`), che chiuderebbe il blocco in anticipo — " +
                                         "toglilo dal file oppure incolla il diagramma a mano");
            }

            if (string.Equals(directive, "json", StringComparison.OrdinalIgnoreCase))
            {
                var jsonError = ValidateJson(fileContent);
                if (jsonError != null)
                {
                    return ErrorBlock(match, $"il file non è JSON valido: {jsonError}");
                }
            }

            var sb = new StringBuilder();
            sb.Append("\n\n```plantuml\n");
            sb.Append(block.Open).Append('\n');

            // The body of the original block, when there is one, comments the diagram:
            // #highlight, <style>, title, ... They must come before the data.
            var trimmedDirectives = extraDirectives.Trim('\r', '\n', ' ', '\t');
            if (trimmedDirectives.Length > 0)
            {
                sb.Append(trimmedDirectives).Append('\n');
            }

            sb.Append(fileContent.TrimEnd('\r', '\n')).Append('\n');
            sb.Append(block.Close).Append('\n');
            sb.Append("```\n\n");

            _logger.LogInformation("[FromExternalDataToPlantuml] @{Directive} expanded from {Path}", directive, absolutePath);
            return sb.ToString();
        }

        /// <summary>
        /// A visible, self-explaining block: the author has to see WHY the diagram is not there.
        /// Blank lines around it so Markdig treats it as a raw HTML block.
        /// </summary>
        private static string ErrorBlock(Match match, string reason)
        {
            var declaration = $"plantuml(@{match.Groups[1].Value}, {match.Groups[2].Value})";
            // Inline style instead of a stylesheet of its own: only the border is coloured, so
            // the block stays readable in the light theme as well as in the dark one.
            return "\n\n<pre class=\"mde-plantuml-include-error\" " +
                   "style=\"border-left:4px solid #f44336;padding:8px 12px;white-space:pre-wrap;\">⚠️ " +
                   HttpUtility.HtmlEncode(declaration) + "\n" +
                   HttpUtility.HtmlEncode(reason) + "</pre>\n\n";
        }

        private static string ValidateJson(string content)
        {
            try
            {
                using var _ = JsonDocument.Parse(content, new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });
                return null;
            }
            catch (JsonException ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Path resolution and sandboxing live in <see cref="ExternalFileResolver"/>, shared with
        /// ```text(path) and ```html(path). Returns null and fills <paramref name="error"/> with
        /// the reason to show to the author.
        /// </summary>
        private string ReadExternalFile(string fileName, RequestInfo requestInfo, out string absoluteFilePath, out string error)
        {
            try
            {
                var content = ExternalFileResolver.ReadInsideProject(
                    fileName, requestInfo, MaxExternalFileSizeBytes, out absoluteFilePath, out error);

                if (content == null)
                {
                    _logger.LogWarning("[FromExternalDataToPlantuml] {Reason} ({FileName})", error, fileName);
                }

                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FromExternalDataToPlantuml] Error reading external file: {FileName}", fileName);
                absoluteFilePath = null;
                error = $"errore leggendo il file: {ex.Message}";
                return null;
            }
        }
    }
}
