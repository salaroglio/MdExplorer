using MdExplorer.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Commands.GitHub
{
    /// <summary>
    /// GitHub-compatible version of emoji command
    /// In GitHub mode, emoji codes like :smile: are left as-is (GitHub renders them natively)
    /// This command does NOT convert emoji to PNG images or interactive widgets
    /// </summary>
    public class FromEmojiToTextGitHub : ICommand, IDisposable
    {
        protected readonly ILogger<FromEmojiToTextGitHub> _logger;

        public int Priority { get; set; } = 20;
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "FromEmojiToTextGitHub";

        public FromEmojiToTextGitHub(ILogger<FromEmojiToTextGitHub> logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }

        public MatchCollection GetMatches(string markdown)
        {
            // We match emoji patterns but don't transform them
            Regex rx = new Regex(@":([^:^ ]*):",
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = rx.Matches(markdown);
            return matches;
        }

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            // Nothing to do
            return markdown;
        }

        public string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            // Nothing to do
            return html;
        }

        public virtual string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            // In GitHub mode, we preserve emoji codes as-is
            // GitHub natively supports emoji rendering for standard codes
            // No transformation needed - just log that we're preserving them

            var matches = GetMatches(markdown);
            if (matches.Count > 0)
            {
                _logger.LogDebug($"[Emoji GitHub] Preserving {matches.Count} emoji codes as-is for GitHub compatibility");
            }

            // Return markdown unchanged - GitHub will render emoji natively
            return markdown;
        }
    }
}
