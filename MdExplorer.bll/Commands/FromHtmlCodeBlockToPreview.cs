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
    /// Transforms ```html code blocks into a 2-tab UI (Preview + Source).
    /// Also supports ```html(file.html) to load content from external files.
    /// Stage 1 (TransformInNewMDFromMD): replaces ```html blocks with a placeholder div before Markdig processes them.
    /// Stage 3 (TransformAfterConversion): replaces placeholder divs with Bootstrap tabbed UI (iframe preview + syntax-highlighted source).
    /// </summary>
    public class FromHtmlCodeBlockToPreview : CommandBase, ICommand
    {
        protected readonly ILogger<FromHtmlCodeBlockToPreview> _logger;
        protected readonly IHelper _helper;

        private const int MaxExternalFileSizeBytes = 512_000; // 500 KB

        public bool Enabled { get; set; } = true;
        public int Priority { get; set; } = 15;
        public string Name { get; set; } = "FromHtmlCodeBlockToPreview";

        public override List<CompatibilityMode> SupportedModes => new List<CompatibilityMode>
        {
            CompatibilityMode.MdExplorer,
            CompatibilityMode.CommonMark
        };

        public FromHtmlCodeBlockToPreview(ILogger<FromHtmlCodeBlockToPreview> logger, IHelper helper)
        {
            _logger = logger;
            _helper = helper;
        }

        public MatchCollection GetMatches(string markdown)
        {
            Regex rx = new Regex(@"```html(?:\(([^)]+)\))?\s*\r?\n([\s\S]*?)```",
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
                    var externalFile = match.Groups[1].Value; // filename from ```html(file.html)
                    var inlineContent = match.Groups[2].Value; // content between ```html and ```

                    string htmlContent;

                    string resolvedFilePath = null;

                    if (!string.IsNullOrWhiteSpace(externalFile))
                    {
                        // External file reference: resolve and read
                        htmlContent = ReadExternalFile(externalFile, requestInfo, out resolvedFilePath);
                        if (htmlContent == null)
                        {
                            // File not found or error — leave the block untouched
                            continue;
                        }
                    }
                    else
                    {
                        // Inline content
                        htmlContent = inlineContent;
                    }

                    var guid = Guid.NewGuid().ToString("N");

                    // Pack filepath (if any) together with content using a separator
                    // Format: "filepath|||base64content" or just "base64content"
                    var base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(htmlContent));
                    var payload = resolvedFilePath != null
                        ? Convert.ToBase64String(Encoding.UTF8.GetBytes(resolvedFilePath)) + "|||" + base64Content
                        : base64Content;

                    // Blank lines before/after are mandatory so Markdig treats it as raw HTML block
                    var placeholder = $"\n\n<div class=\"mde-html-preview-placeholder\" data-id=\"{guid}\" style=\"display:none;\">{payload}</div>\n\n";

                    (markdown, currentIncrement) = ManageReplaceOnMD(markdown, currentIncrement, match, placeholder);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[FromHtmlCodeBlockToPreview] Error processing html code block");
                }
            }

            return markdown;
        }

        public virtual string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            // Find all placeholder divs (same regex as before — no data-filepath attribute needed)
            Regex rx = new Regex(
                @"<div class=""mde-html-preview-placeholder"" data-id=""([^""]+)"" style=""display:none;"">([^<]+)</div>",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var matches = rx.Matches(html);
            if (matches.Count == 0) return html;

            foreach (Match match in matches)
            {
                try
                {
                    var guid = match.Groups[1].Value;
                    var payload = match.Groups[2].Value;

                    // Parse payload: "base64filepath|||base64content" or just "base64content"
                    string htmlContent;
                    string filePath = null;
                    var separatorIndex = payload.IndexOf("|||");
                    if (separatorIndex > -1)
                    {
                        var base64FilePath = payload.Substring(0, separatorIndex);
                        var base64Content = payload.Substring(separatorIndex + 3);
                        filePath = Encoding.UTF8.GetString(Convert.FromBase64String(base64FilePath));
                        htmlContent = Encoding.UTF8.GetString(Convert.FromBase64String(base64Content));
                    }
                    else
                    {
                        htmlContent = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                    }

                    var tabbedHtml = BuildTabbedHtml(guid, htmlContent, filePath);
                    html = html.Replace(match.Value, tabbedHtml);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[FromHtmlCodeBlockToPreview] Error building tabbed HTML");
                }
            }

            return html;
        }

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            return markdown;
        }

        private string BuildTabbedHtml(string guid, string htmlContent, string filePath = null)
        {
            // For source tab: HTML-entity encode so it shows as text
            var sourceContent = HttpUtility.HtmlEncode(htmlContent);

            // Build srcdoc: attribute-escape for HTML attribute
            var srcdocFull = htmlContent
                .Replace("&", "&amp;")
                .Replace("\"", "&quot;");

            // Copy path button (only when there's an external file reference)
            var copyPathButton = !string.IsNullOrWhiteSpace(filePath)
                ? $@"<li class=""nav-item ms-auto mde-html-preview-copy-path"">
      <a class=""nav-link mde-copy-path-btn"" href=""#"" title=""{HttpUtility.HtmlAttributeEncode(filePath)}"" data-filepath=""{HttpUtility.HtmlAttributeEncode(filePath)}""><svg xmlns=""http://www.w3.org/2000/svg"" width=""14"" height=""14"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><rect x=""9"" y=""9"" width=""13"" height=""13"" rx=""2"" ry=""2""></rect><path d=""M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1""></path></svg></a>
    </li>"
                : "";

            return $@"<div class=""mde-html-preview-container"">
  <ul class=""nav nav-tabs mde-html-preview-tabs"" role=""tablist"">
    <li class=""nav-item"">
      <a class=""nav-link active"" data-bs-toggle=""tab"" href=""#mde-preview-{guid}"" role=""tab"">Preview</a>
    </li>
    <li class=""nav-item"">
      <a class=""nav-link"" data-bs-toggle=""tab"" href=""#mde-source-{guid}"" role=""tab"">Source</a>
    </li>
    {copyPathButton}
  </ul>
  <div class=""tab-content mde-html-preview-content"">
    <div class=""tab-pane fade show active"" id=""mde-preview-{guid}"" role=""tabpanel"">
      <iframe class=""mde-html-preview-iframe"" data-preview-id=""{guid}"" sandbox=""allow-scripts allow-same-origin"" srcdoc=""{srcdocFull}""></iframe>
    </div>
    <div class=""tab-pane fade"" id=""mde-source-{guid}"" role=""tabpanel"">
      <pre><code class=""language-markup"">{sourceContent}</code></pre>
    </div>
  </div>
</div>";
        }

        /// <summary>
        /// Resolves and reads an external HTML file, using the same path resolution logic as MDShowMDHtml.
        /// Supports: relative paths (./file.html, ../file.html), absolute from project root (/file.html), and plain names.
        /// </summary>
        private string ReadExternalFile(string fileName, RequestInfo requestInfo, out string absoluteFilePath)
        {
            absoluteFilePath = null;
            try
            {
                string resolvedPath;

                if (fileName.StartsWith("../") || fileName.StartsWith("./"))
                {
                    // Relative to current .md file
                    var listOfItem = requestInfo.CurrentQueryRequest
                        .Split(Path.DirectorySeparatorChar, options: StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                    listOfItem.RemoveAt(listOfItem.Count - 1); // remove filename

                    var currentFolder = string.Join(Path.DirectorySeparatorChar, listOfItem.ToArray());
                    var relativePath = fileName.Replace('/', Path.DirectorySeparatorChar);
                    resolvedPath = Path.Combine(currentFolder, relativePath);
                    resolvedPath = _helper.NormalizePath(resolvedPath);
                }
                else if (fileName.StartsWith("/"))
                {
                    // Absolute from project root
                    resolvedPath = fileName.Remove(0, 1).Replace('/', Path.DirectorySeparatorChar);
                }
                else
                {
                    // Plain name: relative to current .md file
                    var listOfItem = requestInfo.CurrentQueryRequest
                        .Split(Path.DirectorySeparatorChar, options: StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                    listOfItem.RemoveAt(listOfItem.Count - 1);

                    var currentFolder = string.Join(Path.DirectorySeparatorChar, listOfItem.ToArray());
                    resolvedPath = Path.Combine(currentFolder, fileName.Replace('/', Path.DirectorySeparatorChar));
                    resolvedPath = _helper.NormalizePath(resolvedPath);
                }

                // Build absolute path from project root
                var absolutePath = Path.Combine(requestInfo.CurrentRoot, resolvedPath);
                absolutePath = Path.GetFullPath(absolutePath);

                // Security: ensure the resolved path is within the project root
                var projectRoot = Path.GetFullPath(requestInfo.CurrentRoot);
                if (!absolutePath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("[FromHtmlCodeBlockToPreview] Path traversal blocked: {Path}", fileName);
                    return null;
                }

                if (!File.Exists(absolutePath))
                {
                    _logger.LogWarning("[FromHtmlCodeBlockToPreview] External file not found: {Path}", absolutePath);
                    return null;
                }

                var fileInfo = new FileInfo(absolutePath);
                if (fileInfo.Length > MaxExternalFileSizeBytes)
                {
                    _logger.LogWarning("[FromHtmlCodeBlockToPreview] External file too large ({Size} bytes): {Path}", fileInfo.Length, absolutePath);
                    return null;
                }

                absoluteFilePath = absolutePath;
                return File.ReadAllText(absolutePath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FromHtmlCodeBlockToPreview] Error reading external file: {FileName}", fileName);
                return null;
            }
        }
    }
}
