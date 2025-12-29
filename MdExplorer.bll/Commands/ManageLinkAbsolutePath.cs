using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands.html;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Commands
{
    /// <summary>
    /// Manage absolute path and and add SignalR connectionId
    /// </summary>
    internal class ManageLinkAbsolutePath : ICommand
    {
        private readonly ILogger<ManageLinkAbsolutePath> _logger;

        public ManageLinkAbsolutePath(ILogger<ManageLinkAbsolutePath> logger)
        {
            _logger = logger;
        }

        public int Priority { get; set; } = 50;
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "ManageLinkAbsolutePath";
        public List<Configuration.Models.CompatibilityMode> SupportedModes => null; // Supports all modes

        public MatchCollection GetMatches(string markdown)
        {
            var reg = @"\[([^\]]*)\]\(([^\)]*)\)";
            Regex rx = new Regex(reg,
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);
            return matches;
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
            var links = GetMatches(markdown);
            foreach (Match link in links)
            {
                var linkValue = link.Groups[2].Value;

                // Skip external links, anchors, and non-markdown files
                if (linkValue.StartsWith("http://") ||
                    linkValue.StartsWith("https://") ||
                    linkValue.StartsWith("mailto:") ||
                    linkValue.StartsWith("#"))
                {
                    continue;
                }

                // Process links that need connectionId: absolute paths, relative paths (..), or .md files
                bool isAbsoluteOrRelative = linkValue.StartsWith("/") || linkValue.StartsWith("../");
                bool isMdFile = linkValue.EndsWith(".md") || linkValue.Contains(".md#");

                if (isAbsoluteOrRelative || isMdFile)
                {
                    string newlink;

                    if (isAbsoluteOrRelative)
                    {
                        // Absolute or parent-relative paths: prepend /api/mdexplorer
                        newlink = "/api/mdexplorer" + linkValue;
                    }
                    else
                    {
                        // Simple relative .md links: just add connectionId query param
                        newlink = linkValue;
                    }

                    // Handle anchor fragments (#section)
                    Regex rxSharp = new Regex(@"([^#]*)(?:(#.*))?",
                             RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    var matchesSharp = rxSharp.Matches(newlink);

                    var firstPart = matchesSharp.First().Groups[1].Value;
                    var secondPart = matchesSharp.First().Groups[2]?.Value;

                    newlink = $@"{firstPart}" + "?connectionId=" + requestInfo.ConnectionId + secondPart;

                    // Build new link preserving original text: [originalText](newUrl)
                    var linkText = link.Groups[1].Value;
                    var newFullLink = $"[{linkText}]({newlink})";
                    markdown = markdown.Replace(link.Groups[0].Value, newFullLink);
                }
            }
            return markdown;
        }
    }
}
