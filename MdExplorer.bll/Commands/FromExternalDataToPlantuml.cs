using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Configuration.Models;
using MdExplorer.Features.Interfaces;
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

            var currentIncrement = 0;
            foreach (Match match in matches)
            {
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
        /// Same path semantics as <see cref="FromTextCodeBlockToPreview"/>: relative (./, ../)
        /// and plain names resolve against the folder of the .md being rendered, a leading /
        /// resolves against the PROJECT root, and anything landing outside the project is refused.
        ///
        /// The collapsing of . and .. is left to <see cref="Path.GetFullPath(string)"/> rather
        /// than to IHelper.NormalizePath, which rejoins the segments with a hardcoded backslash
        /// and therefore only resolves on Windows.
        ///
        /// Returns null and fills <paramref name="error"/> when the file cannot be used.
        /// </summary>
        private string ReadExternalFile(string fileName, RequestInfo requestInfo, out string absoluteFilePath, out string error)
        {
            absoluteFilePath = null;
            error = null;
            try
            {
                // Documents get written on Windows and read on Linux (and the other way round),
                // so accept both separators in the declared path.
                var declaredPath = fileName.Replace('\\', '/').Trim();

                string relativePath;
                if (declaredPath.StartsWith("/"))
                {
                    relativePath = declaredPath.TrimStart('/');
                }
                else
                {
                    // Folder of the .md currently being rendered, relative to the project root.
                    var currentFolder = Path.GetDirectoryName(requestInfo.CurrentQueryRequest) ?? string.Empty;
                    relativePath = Path.Combine(currentFolder, declaredPath.Replace('/', Path.DirectorySeparatorChar));
                }

                relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);

                var projectRoot = Path.GetFullPath(requestInfo.CurrentRoot);
                var absolutePath = Path.GetFullPath(Path.Combine(projectRoot, relativePath));

                // Trailing separator on the root, so that a sibling folder whose name merely
                // starts with the root's name is not mistaken for something inside the project.
                var rootWithSeparator = projectRoot.EndsWith(Path.DirectorySeparatorChar)
                    ? projectRoot
                    : projectRoot + Path.DirectorySeparatorChar;

                if (!absolutePath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("[FromExternalDataToPlantuml] Path traversal blocked: {Path}", fileName);
                    error = "il percorso esce dalla cartella del progetto";
                    return null;
                }

                if (!File.Exists(absolutePath))
                {
                    _logger.LogWarning("[FromExternalDataToPlantuml] External file not found: {Path}", absolutePath);
                    error = $"file non trovato — cercato in {absolutePath}";
                    return null;
                }

                var fileInfo = new FileInfo(absolutePath);
                if (fileInfo.Length > MaxExternalFileSizeBytes)
                {
                    _logger.LogWarning("[FromExternalDataToPlantuml] External file too large ({Size} bytes): {Path}", fileInfo.Length, absolutePath);
                    error = $"file troppo grande ({fileInfo.Length / 1024} KB): il limite è {MaxExternalFileSizeBytes / 1024} KB";
                    return null;
                }

                absoluteFilePath = absolutePath;
                return File.ReadAllText(absolutePath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FromExternalDataToPlantuml] Error reading external file: {FileName}", fileName);
                error = $"errore leggendo il file: {ex.Message}";
                return null;
            }
        }
    }
}
