using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Configuration.Models;
using MdExplorer.Features.Execution;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MdExplorer.Features.Commands
{
    /// <summary>
    /// Detects fenced shell blocks in the MarkDig-rendered HTML
    /// (<c>&lt;pre&gt;&lt;code class="language-{bash|sh|shell|powershell|pwsh|ps1|cmd|bat|batch}"&gt;</c>)
    /// and wraps them with a runnable UI: toolbar with Run button, detected parameters, and output pane.
    /// Client-side JS (mde-exec-blocks.js) reads the data-* attributes and orchestrates the actual run.
    /// </summary>
    public class FromExecutableCodeBlockToRunnable : CommandBase, ICommand
    {
        protected readonly ILogger<FromExecutableCodeBlockToRunnable> _logger;
        protected readonly IHelper _helper;

        public bool Enabled { get; set; } = true;
        public int Priority { get; set; } = 25;
        public string Name { get; set; } = "FromExecutableCodeBlockToRunnable";

        public override List<CompatibilityMode> SupportedModes => new List<CompatibilityMode>
        {
            CompatibilityMode.MdExplorer,
            CompatibilityMode.CommonMark
        };

        // Matches <pre><code class="language-X">...</code></pre> where X is one of the supported shells.
        // Non-greedy body; single-line class attribute. Supports both <pre> and <pre class="..."> prefixes.
        // The <code> tag may carry extra attributes after the class: MarkdownSourceMapService
        // (ai-selection feature) decorates code blocks with data-mde-line-start/end.
        private static readonly Regex FenceRegex = new(
            @"<pre(?:\s+[^>]*)?><code\s+class=""language-(bash|sh|shell|powershell|pwsh|ps1|cmd|bat|batch)""(?:\s+[^>]*)?>([\s\S]*?)</code></pre>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Serialize the parameter list with camelCase property names so the iframe JS
        // (mde-exec-blocks.js) — and the postMessage payload it builds for the Angular
        // ExecutionService — read `p.name` / `p.defaultValue` consistently. Without this,
        // Newtonsoft's default PascalCase output would make every `p.name` undefined
        // downstream, and parameter values would never reach the backend Run endpoint.
        private static readonly JsonSerializerSettings ParamsJsonSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };

        public FromExecutableCodeBlockToRunnable(
            ILogger<FromExecutableCodeBlockToRunnable> logger,
            IHelper helper)
        {
            _logger = logger;
            _helper = helper;
        }

        public MatchCollection GetMatches(string markdown) => FenceRegex.Matches(markdown);

        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo) => markdown;

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo) => markdown;

        public virtual string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            var matches = FenceRegex.Matches(html);
            if (matches.Count == 0) return html;

            // Replace back-to-front to keep earlier match indices stable.
            var buffer = new StringBuilder(html);
            for (var i = matches.Count - 1; i >= 0; i--)
            {
                var match = matches[i];
                try
                {
                    var language = NormalizeLanguage(match.Groups[1].Value);
                    var escapedCode = match.Groups[2].Value;
                    var rawCode = HttpUtility.HtmlDecode(escapedCode);
                    // Suffix with the occurrence index so two blocks with identical
                    // language+code don't collide on the same id. A pure content hash
                    // would make duplicates share one id, which breaks output/state
                    // routing (the clicked block hangs on "Running…" while its twin
                    // shows the output). `i` is the document-order index of the match.
                    var blockId = ComputeBlockId(language, rawCode) + "-" + i;

                    var parameters = ParameterExtractor.Extract(rawCode, language);
                    var paramsJson = JsonConvert.SerializeObject(parameters, ParamsJsonSettings);
                    var paramsBase64 = Base64(paramsJson);
                    var codeBase64 = Base64(rawCode);

                    var replacement = BuildRunnableHtml(
                        language,
                        blockId,
                        codeBase64,
                        paramsBase64,
                        parameters,
                        match.Value);

                    buffer.Remove(match.Index, match.Length);
                    buffer.Insert(match.Index, replacement);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[FromExecutableCodeBlockToRunnable] Failed to wrap fenced block");
                }
            }
            return buffer.ToString();
        }

        private static string BuildRunnableHtml(
            string language,
            string blockId,
            string codeBase64,
            string paramsBase64,
            List<ExecutionParameter> parameters,
            string originalPreCode)
        {
            // The original <pre><code class="language-X">...</code></pre> is preserved verbatim
            // so Prism.js picks it up client-side for syntax highlighting.
            var paramsHtml = (parameters == null || parameters.Count == 0)
                ? string.Empty
                : @"<div class=""mde-exec-params"" data-params-inline=""1"">"
                  + string.Concat(parameters.Select(BuildParamControl))
                  + @"</div>";

            return
                $@"<div class=""mde-exec-block"" data-lang=""{HttpUtility.HtmlAttributeEncode(language)}"" data-block-id=""{blockId}"" data-code=""{codeBase64}"" data-params=""{paramsBase64}"">" +
                $@"<div class=""mde-exec-toolbar"">" +
                $@"<span class=""mde-exec-lang"">{HttpUtility.HtmlEncode(language)}</span>" +
                paramsHtml +
                $@"<span class=""mde-run-split"">" +
                $@"<button class=""mde-run-btn"" type=""button"" aria-label=""Run""><span class=""mde-run-icon"">&#9654;</span><span class=""mde-run-label""> Run</span></button>" +
                $@"<button class=""mde-run-caret"" type=""button"" aria-label=""More run options"" aria-haspopup=""true"">&#9662;</button>" +
                $@"<div class=""mde-run-menu"" hidden><button class=""mde-run-service"" type=""button"">Run as service</button></div>" +
                $@"</span>" +
                $@"</div>" +
                originalPreCode +
                $@"<div class=""mde-exec-output"" hidden>" +
                $@"<div class=""mde-exec-output-header""><span class=""mde-exec-output-label"">Output</span><span class=""mde-exec-output-status""></span></div>" +
                $@"<pre class=""mde-exec-output-content""></pre>" +
                $@"</div>" +
                $@"</div>";
        }

        private static string BuildParamControl(ExecutionParameter p)
        {
            var name = HttpUtility.HtmlAttributeEncode(p.Name ?? string.Empty);
            var defaultValue = p.DefaultValue ?? string.Empty;
            var defaultValueAttr = HttpUtility.HtmlAttributeEncode(defaultValue);
            var description = p.Description ?? string.Empty;
            var placeholder = HttpUtility.HtmlAttributeEncode(description);
            var title = string.IsNullOrEmpty(description)
                ? string.Empty
                : $@" title=""{HttpUtility.HtmlAttributeEncode(description)}""";

            var nameSpan = $@"<span class=""mde-param-name"">{HttpUtility.HtmlEncode(p.Name ?? string.Empty)}</span>";

            // Path-picker variant: a button + hidden input that the harvester still reads.
            if (!string.IsNullOrEmpty(p.Picker))
            {
                var pickerType = p.Picker; // "file" | "dir" | "out-file"
                string icon;
                string emptyLabel;
                switch (pickerType)
                {
                    case "dir":
                        icon = "&#128193;"; /* 📁 */
                        emptyLabel = "Choose folder…";
                        break;
                    case "out-file":
                        icon = "&#128190;"; /* 💾 */
                        emptyLabel = "Choose output…";
                        break;
                    default: // "file"
                        icon = "&#128196;"; /* 📄 */
                        emptyLabel = "Choose file…";
                        break;
                }
                var labelText = string.IsNullOrEmpty(defaultValue) ? emptyLabel : ShortenPath(defaultValue);
                var button = $@"<button type=""button"" class=""mde-param-picker"" data-param-name=""{name}"" data-picker-type=""{pickerType}""><span class=""mde-param-picker-icon"">{icon}</span><span class=""mde-param-picker-label"">{HttpUtility.HtmlEncode(labelText)}</span></button>";
                var hidden = $@"<input class=""mde-param-input"" type=""hidden"" data-param-name=""{name}"" value=""{defaultValueAttr}"">";
                return $@"<label class=""mde-param mde-param-path""{title}>{nameSpan}{button}{hidden}</label>";
            }

            // Plain text / password variant.
            var inputType = p.IsSecret ? "password" : "text";
            var cssClass = p.IsSecret ? "mde-param mde-param-secret" : "mde-param";
            var input = $@"<input class=""mde-param-input"" type=""{inputType}"" data-param-name=""{name}"" value=""{defaultValueAttr}"" placeholder=""{placeholder}"" autocomplete=""off"" spellcheck=""false"">";
            var toggle = p.IsSecret
                ? @"<button type=""button"" class=""mde-param-toggle"" aria-label=""Toggle visibility"" tabindex=""-1"">&#128065;</button>"
                : string.Empty;
            return $@"<label class=""{cssClass}""{title}>{nameSpan}{input}{toggle}</label>";
        }

        // Display only — the actual value sent to the runner is the full path in the hidden input.
        private static string ShortenPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            const int max = 38;
            if (path.Length <= max) return path;
            return "…" + path.Substring(path.Length - (max - 1));
        }

        private static string NormalizeLanguage(string raw)
        {
            var lower = raw.ToLowerInvariant();
            return lower switch
            {
                "ps1" => "powershell",
                "bat" => "cmd",
                "batch" => "cmd",
                _ => lower
            };
        }

        private static string ComputeBlockId(string language, string rawCode)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(language + "\n" + rawCode);
            var hash = sha.ComputeHash(bytes);
            var sb = new StringBuilder(12);
            for (var i = 0; i < 6; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        private static string Base64(string s) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(s ?? string.Empty));
    }
}
