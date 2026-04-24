using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Configuration.Models;
using MdExplorer.Features.Execution;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        private static readonly Regex FenceRegex = new(
            @"<pre(?:\s+[^>]*)?><code\s+class=""language-(bash|sh|shell|powershell|pwsh|ps1|cmd|bat|batch)"">([\s\S]*?)</code></pre>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
                    var blockId = ComputeBlockId(language, rawCode);

                    var parameters = ParameterExtractor.Extract(rawCode, language);
                    var paramsJson = JsonConvert.SerializeObject(parameters);
                    var paramsBase64 = Base64(paramsJson);
                    var codeBase64 = Base64(rawCode);

                    var replacement = BuildRunnableHtml(
                        language,
                        blockId,
                        codeBase64,
                        paramsBase64,
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
            string originalPreCode)
        {
            // The original <pre><code class="language-X">...</code></pre> is preserved verbatim
            // so Prism.js picks it up client-side for syntax highlighting.
            return
                $@"<div class=""mde-exec-block"" data-lang=""{HttpUtility.HtmlAttributeEncode(language)}"" data-block-id=""{blockId}"" data-code=""{codeBase64}"" data-params=""{paramsBase64}"">" +
                $@"<div class=""mde-exec-toolbar"">" +
                $@"<span class=""mde-exec-lang"">{HttpUtility.HtmlEncode(language)}</span>" +
                $@"<button class=""mde-run-btn"" type=""button"" aria-label=""Run""><span class=""mde-run-icon"">&#9654;</span><span class=""mde-run-label""> Run</span></button>" +
                $@"</div>" +
                originalPreCode +
                $@"<div class=""mde-exec-output"" hidden>" +
                $@"<div class=""mde-exec-output-header""><span class=""mde-exec-output-label"">Output</span><span class=""mde-exec-output-status""></span></div>" +
                $@"<pre class=""mde-exec-output-content""></pre>" +
                $@"</div>" +
                $@"</div>";
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
