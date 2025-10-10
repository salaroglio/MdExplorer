using MdExplorer.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Commands.GitHub
{
    /// <summary>
    /// GitHub-compatible version of link-to-application command
    /// In GitHub mode, links remain as standard markdown/HTML links
    /// No onclick handlers or application launching functionality
    /// </summary>
    public class FromLinkToMarkdownGitHub : ICommand
    {
        private readonly ILogger<FromLinkToMarkdownGitHub> _logger;

        public int Priority { get; set; } = 30;
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "FromLinkToMarkdownGitHub";

        public FromLinkToMarkdownGitHub(ILogger<FromLinkToMarkdownGitHub> logger)
        {
            _logger = logger;
        }

        public MatchCollection GetMatches(string markdown)
        {
            // Match standard markdown links
            Regex rx = new Regex(@"\[([^\]]+)\]\(([^)]+)\)",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = rx.Matches(markdown);
            return matches;
        }

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            return markdown;
        }

        public string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            // In GitHub mode, we DON'T transform links to onclick handlers
            // Links should remain as standard HTML <a> tags
            // GitHub will handle them as regular links (possibly with file preview)

            _logger.LogDebug("[Link GitHub] Preserving standard HTML links for GitHub compatibility");
            return html;
        }

        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            // In GitHub mode, markdown links remain unchanged
            // No transformation to application launchers

            var matches = GetMatches(markdown);
            if (matches.Count > 0)
            {
                _logger.LogDebug($"[Link GitHub] Preserving {matches.Count} markdown links as-is for GitHub compatibility");
            }

            return markdown;
        }
    }
}
