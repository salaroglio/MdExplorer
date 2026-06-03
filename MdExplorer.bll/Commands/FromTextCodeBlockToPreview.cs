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

        private const int MaxExternalFileSizeBytes = 512_000; // 500 KB — same as HTML preview

        // File extension → Prism language identifier. Unknown extensions fall through
        // to plain <pre><code> without a language class (Prism will leave it untouched).
        private static readonly Dictionary<string, string> ExtensionToLanguage =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [".ttl"] = "turtle",     [".nt"] = "turtle",      [".n3"] = "turtle",     [".nq"] = "turtle",
                [".rdf"] = "markup",     [".owl"] = "markup",
                [".json"] = "json",      [".jsonld"] = "json",
                [".yaml"] = "yaml",      [".yml"] = "yaml",
                [".xml"] = "markup",     [".xsd"] = "markup",     [".xslt"] = "markup",
                [".sql"] = "sql",        [".cypher"] = "cypher",  [".sparql"] = "sparql",
                [".cs"] = "csharp",      [".ts"] = "typescript",  [".js"] = "javascript",
                [".java"] = "java",      [".kt"] = "kotlin",
                [".py"] = "python",
                [".sh"] = "bash",        [".bash"] = "bash",      [".ps1"] = "powershell",
                [".css"] = "css",        [".scss"] = "scss",
                [".md"] = "markdown",
                [".cob"] = "cobol",      [".cbl"] = "cobol",      [".cpy"] = "cobol",
            };

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

            var currentIncrement = 0;
            foreach (Match match in matches)
            {
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

                    var language = LanguageFromExtension(resolvedFilePath);
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

                    var rendered = BuildContainerHtml(guid, filePath, language, content);
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

        private string BuildContainerHtml(string guid, string filePath, string language, string content)
        {
            var encodedContent = HttpUtility.HtmlEncode(content);
            var encodedPath    = HttpUtility.HtmlAttributeEncode(filePath);
            var encodedFileNameForHeader = HttpUtility.HtmlEncode(Path.GetFileName(filePath));

            var languageClass = string.IsNullOrEmpty(language) ? "" : $" class=\"language-{language}\"";

            // SVG icons (kept inline to avoid extra HTTP requests and to match html-preview style)
            var copyIcon       = @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""14"" height=""14"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><rect x=""9"" y=""9"" width=""13"" height=""13"" rx=""2"" ry=""2""></rect><path d=""M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1""></path></svg>";
            var fullscreenIcon = @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""14"" height=""14"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><polyline points=""15 3 21 3 21 9""></polyline><polyline points=""9 21 3 21 3 15""></polyline><line x1=""21"" y1=""3"" x2=""14"" y2=""10""></line><line x1=""3"" y1=""21"" x2=""10"" y2=""14""></line></svg>";

            return $@"<div class=""mde-text-include-container"" data-id=""{guid}"">
  <div class=""mde-text-include-header"">
    <span class=""mde-text-include-filename"">{encodedFileNameForHeader}</span>
    <span class=""mde-text-include-toolbar"">
      <a class=""mde-text-include-btn mde-copy-path-btn"" href=""#"" title=""{encodedPath}"" data-filepath=""{encodedPath}"">{copyIcon}</a>
      <a class=""mde-text-include-btn mde-text-include-fullscreen-btn"" href=""#"" title=""Fullscreen"">{fullscreenIcon}</a>
    </span>
  </div>
  <pre class=""mde-text-include-pre""><code{languageClass}>{encodedContent}</code></pre>
</div>";
        }

        private static string LanguageFromExtension(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath)) return string.Empty;
            var ext = Path.GetExtension(absolutePath);
            return ExtensionToLanguage.TryGetValue(ext, out var lang) ? lang : string.Empty;
        }

        /// <summary>
        /// Mirrors the path-resolution + sandbox logic of <see cref="FromHtmlCodeBlockToPreview"/>.
        /// Supports relative (./, ../), project-root absolute (/), and plain-name paths.
        /// Returns null when the file is not found, too large, or escapes the project root.
        /// </summary>
        private string ReadExternalFile(string fileName, RequestInfo requestInfo, out string absoluteFilePath)
        {
            absoluteFilePath = null;
            try
            {
                string resolvedPath;

                if (fileName.StartsWith("../") || fileName.StartsWith("./"))
                {
                    var listOfItem = requestInfo.CurrentQueryRequest
                        .Split(Path.DirectorySeparatorChar, options: StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                    listOfItem.RemoveAt(listOfItem.Count - 1); // drop the .md filename

                    var currentFolder = string.Join(Path.DirectorySeparatorChar, listOfItem.ToArray());
                    var relativePath = fileName.Replace('/', Path.DirectorySeparatorChar);
                    resolvedPath = Path.Combine(currentFolder, relativePath);
                    resolvedPath = _helper.NormalizePath(resolvedPath);
                }
                else if (fileName.StartsWith("/"))
                {
                    resolvedPath = fileName.Remove(0, 1).Replace('/', Path.DirectorySeparatorChar);
                }
                else
                {
                    var listOfItem = requestInfo.CurrentQueryRequest
                        .Split(Path.DirectorySeparatorChar, options: StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                    listOfItem.RemoveAt(listOfItem.Count - 1);

                    var currentFolder = string.Join(Path.DirectorySeparatorChar, listOfItem.ToArray());
                    resolvedPath = Path.Combine(currentFolder, fileName.Replace('/', Path.DirectorySeparatorChar));
                    resolvedPath = _helper.NormalizePath(resolvedPath);
                }

                var absolutePath = Path.Combine(requestInfo.CurrentRoot, resolvedPath);
                absolutePath = Path.GetFullPath(absolutePath);

                var projectRoot = Path.GetFullPath(requestInfo.CurrentRoot);
                if (!absolutePath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("[FromTextCodeBlockToPreview] Path traversal blocked: {Path}", fileName);
                    return null;
                }

                if (!File.Exists(absolutePath))
                {
                    _logger.LogWarning("[FromTextCodeBlockToPreview] External file not found: {Path}", absolutePath);
                    return null;
                }

                var fileInfo = new FileInfo(absolutePath);
                if (fileInfo.Length > MaxExternalFileSizeBytes)
                {
                    _logger.LogWarning("[FromTextCodeBlockToPreview] External file too large ({Size} bytes): {Path}", fileInfo.Length, absolutePath);
                    return null;
                }

                absoluteFilePath = absolutePath;
                return File.ReadAllText(absolutePath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FromTextCodeBlockToPreview] Error reading external file: {FileName}", fileName);
                return null;
            }
        }
    }
}
