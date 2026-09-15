using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Configuration.Models;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MdExplorer.Features.Commands
{
    /// <summary>
    /// Embeds text files referenced via ```text(./path/to/file.ext) into a single-pane
    /// syntax-highlighted source view (no preview tab, no iframe).
    /// Language for highlighting is deduced from the file extension.
    /// Parentheses with a path are MANDATORY: ```text without parens passes through
    /// to Markdig as a regular plain-text code block.
    /// </summary>
    public class FromTextCodeBlockToPreview : CommandBase, ICommand
    {
        protected readonly ILogger<FromTextCodeBlockToPreview> _logger;
        protected readonly IHelper _helper;

        private const int MaxExternalFileSizeBytes = TextFileView.MaxBytes;

        public bool Enabled { get; set; } = true;
        public int Priority { get; set; } = 16; // right after FromHtmlCodeBlockToPreview (15)
        public string Name { get; set; } = "FromTextCodeBlockToPreview";

        public override List<CompatibilityMode> SupportedModes => new List<CompatibilityMode>
        {
            CompatibilityMode.MdExplorer,
            CompatibilityMode.CommonMark
        };

        public FromTextCodeBlockToPreview(ILogger<FromTextCodeBlockToPreview> logger, IHelper helper)
        {
            _logger = logger;
            _helper = helper;
        }

        public MatchCollection GetMatches(string markdown)
        {
            // Parens with a path are mandatory: ```text(path) only.
            // Plain ```text blocks pass through to Markdig untouched.
            Regex rx = new Regex(@"```text\(([^)]+)\)\s*\r?\n([\s\S]*?)```",
                                 RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return rx.Matches(markdown);
        }

        public virtual string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var matches = GetMatches(markdown);
            if (matches.Count == 0) return markdown;

            var regions = MarkdownCodeRegions.Of(markdown);
            var currentIncrement = 0;
            foreach (Match match in matches)
            {
                // The syntax shown as an example inside a ```` block is text, not a file to embed.
                if (regions.IsCode(match.Index))
                {
                    continue;
                }

                try
                {
                    var externalFile = match.Groups[1].Value;
                    var fileContent = ReadExternalFile(externalFile, requestInfo, out var resolvedFilePath);
                    if (fileContent == null)
                    {
                        // File not found / too big / out of project: leave the block untouched.
                        continue;
                    }

                    var guid = Guid.NewGuid().ToString("N");

                    var language = TextFileView.LanguageFor(resolvedFilePath);
                    var base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent));
                    var base64Path = Convert.ToBase64String(Encoding.UTF8.GetBytes(resolvedFilePath));

                    // Payload format: "base64path|||base64lang|||base64content"
                    var payload = $"{base64Path}|||{Convert.ToBase64String(Encoding.UTF8.GetBytes(language))}|||{base64Content}";

                    // Blank lines around so Markdig treats it as a raw HTML block.
                    var placeholder = $"\n\n<div class=\"mde-text-include-placeholder\" data-id=\"{guid}\" style=\"display:none;\">{payload}</div>\n\n";

                    (markdown, currentIncrement) = ManageReplaceOnMD(markdown, currentIncrement, match, placeholder);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[FromTextCodeBlockToPreview] Error processing text code block");
                }
            }

            return markdown;
        }

        public virtual string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            Regex rx = new Regex(
                @"<div class=""mde-text-include-placeholder"" data-id=""([^""]+)"" style=""display:none;"">([^<]+)</div>",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var matches = rx.Matches(html);
            if (matches.Count == 0) return html;

            foreach (Match match in matches)
            {
                try
                {
                    var guid = match.Groups[1].Value;
                    var payload = match.Groups[2].Value;

                    var parts = payload.Split(new[] { "|||" }, StringSplitOptions.None);
                    if (parts.Length != 3) continue;

                    var filePath = Encoding.UTF8.GetString(Convert.FromBase64String(parts[0]));
                    var language = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
                    var content  = Encoding.UTF8.GetString(Convert.FromBase64String(parts[2]));

                    var rendered = TextFileView.ContainerHtml(guid, filePath, language, content);
                    html = html.Replace(match.Value, rendered);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[FromTextCodeBlockToPreview] Error building include container");
                }
            }

            return html;
        }

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            return markdown;
        }

        /// <summary>
        /// Path resolution and sandboxing live in <see cref="ExternalFileResolver"/>, shared with
        /// the other include commands. Returns null when the file is not found, too large, or
        /// escapes the project root.
        /// </summary>
        private string ReadExternalFile(string fileName, RequestInfo requestInfo, out string absoluteFilePath)
        {
            try
            {
                var content = ExternalFileResolver.ReadInsideProject(
                    fileName, requestInfo, MaxExternalFileSizeBytes, out absoluteFilePath, out var error);

                if (content == null)
                {
                    _logger.LogWarning("[FromTextCodeBlockToPreview] {Reason} ({FileName})", error, fileName);
                }

                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FromTextCodeBlockToPreview] Error reading external file: {FileName}", fileName);
                absoluteFilePath = null;
                return null;
            }
        }
    }
}
