using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Configuration.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Commands
{
    /// <summary>
    /// Protects emoji patterns in URLs from Markdig's EmojiAndSmiley extension.
    /// URL-encodes colons within URLs to prevent :x:, :o:, :p: etc. from being converted to emoji.
    /// </summary>
    internal class ProtectEmojiInUrls : ICommand
    {
        private readonly ILogger<ProtectEmojiInUrls> _logger;

        public ProtectEmojiInUrls(ILogger<ProtectEmojiInUrls> logger)
        {
            _logger = logger;
        }

        public int Priority { get; set; } = 3;  // Execute before other commands
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "ProtectEmojiInUrls";
        public List<CompatibilityMode> SupportedModes => null; // Supports all modes

        // Regex for markdown links: [text](https://...)
        private static readonly Regex MarkdownLinkRegex = new Regex(
            @"\[([^\]]*)\]\((https?://[^\)]+)\)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Regex for autolinks: <https://...>
        private static readonly Regex AutolinkRegex = new Regex(
            @"<(https?://[^>]+)>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public MatchCollection GetMatches(string markdown)
        {
            return MarkdownLinkRegex.Matches(markdown);
        }

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            // DO NOTHING
            return markdown;
        }

        public string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            // DO NOTHING
            return html;
        }

        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            // 1. Protect markdown links [text](url)
            markdown = MarkdownLinkRegex.Replace(markdown, match =>
            {
                var text = match.Groups[1].Value;
                var url = EncodeColonsInUrl(match.Groups[2].Value);
                return $"[{text}]({url})";
            });

            // 2. Protect autolinks <url>
            markdown = AutolinkRegex.Replace(markdown, match =>
            {
                var url = EncodeColonsInUrl(match.Groups[1].Value);
                return $"<{url}>";
            });

            return markdown;
        }

        /// <summary>
        /// Encodes colons in the URL path (after host:port) to prevent emoji conversion.
        /// Example: https://sharepoint.com/:x:/file -> https://sharepoint.com/%3Ax%3A/file
        /// </summary>
        private string EncodeColonsInUrl(string url)
        {
            // Find protocol separator
            var protocolEnd = url.IndexOf("://");
            if (protocolEnd == -1) return url;

            var protocol = url.Substring(0, protocolEnd + 3);  // "https://"
            var afterProtocol = url.Substring(protocolEnd + 3); // "example.com:8080/:x:/path"

            // Find where host:port ends (first /, ?, or # after protocol)
            var pathStart = afterProtocol.IndexOfAny(new[] { '/', '?', '#' });
            if (pathStart == -1)
            {
                // No path, just host - nothing to encode
                return url;
            }

            var hostPart = afterProtocol.Substring(0, pathStart);  // "example.com:8080"
            var pathPart = afterProtocol.Substring(pathStart);     // "/:x:/path?query#anchor"

            // Encode colons only in the path part (not in host:port)
            var encodedPath = pathPart.Replace(":", "%3A");

            return protocol + hostPart + encodedPath;
        }
    }
}
